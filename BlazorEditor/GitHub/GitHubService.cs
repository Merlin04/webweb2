using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BlazorEditor.Data;
using BlazorEditor.Shared;
using BlazorEditor.Utils;
using Octokit;
using Octokit.Internal;

namespace BlazorEditor.GitHub
{
    public enum StatusType
    {
        Error,
        Status,
        Success
    }
    
    public class GitHubService
    {
        private ApplicationDbContext _context;
        private GitHubClient _github;
        private string _githubToken;
        private string _repoName;
        private string _repoDestination;
        private string _repoBranch;
        private string _repoDirectory;
        
        public event Action<string, StatusType> StatusUpdate;
        
        public GitHubService(ApplicationDbContext context)
        {
            _context = context;
            RefreshConfig();
        }

        public async Task DeploySite()
        {
            StatusUpdate?.Invoke("Starting deploy...", StatusType.Status);
            if (_githubToken == string.Empty)
            {
                // TODO: Send an error message
                StatusUpdate?.Invoke("No GitHub token configured.", StatusType.Error);
                return;
            }

            if (_repoName == string.Empty)
            {
                StatusUpdate?.Invoke("No repository configured.", StatusType.Error);
                return;
            }

            string[] splitRepoName = _repoName.Split('/');
            if (splitRepoName.Length != 2)
            {
                StatusUpdate?.Invoke("Repository name format invalid.", StatusType.Error);
                return;
            }

            if (_repoDestination == string.Empty)
            {
                StatusUpdate?.Invoke("No destination configured.", StatusType.Error);
                return;
            }
            
            StatusUpdate?.Invoke("Getting repository...", StatusType.Status);
            long repoId = (await _github.Repository.Get(splitRepoName[0], splitRepoName[1])).Id;

            string headMasterRef = "heads/" + _repoBranch;
            Reference masterReference;
            try
            {
                masterReference = await _github.Git.Reference.Get(repoId, headMasterRef);
            }
            catch (Exception ex)
            {
                if (ex is NotFoundException)
                {
                    // This probably means the branch hasn't been created yet
                    // https://github.com/octokit/octokit.net/issues/1098
                    StatusUpdate?.Invoke("Creating branch " + _repoBranch + "...", StatusType.Status);
                    Reference master = await _github.Git.Reference.Get(repoId, "heads/master");
                    masterReference = await _github.Git.Reference.Create(repoId,
                        new NewReference("refs/" + headMasterRef, master.Object.Sha));
                }
                else
                {
                    throw;
                }
            }

            StatusUpdate?.Invoke("Getting current commit...", StatusType.Status);
            GitHubCommit currentCommit = (await _github.Repository.Commit.GetAll(repoId)).First();
            string currentTreeSha = currentCommit.Commit.Tree.Sha;

            NewTree newTree = new NewTree();
            
            if (_repoDirectory != string.Empty)
            {
                TreeResponse currentTree = await _github.Git.Tree.Get(repoId, currentTreeSha);
                // Copy the current tree to the new tree
                StatusUpdate?.Invoke("Copying current tree to new tree...", StatusType.Status);
                newTree = (await CopySubtree(_github, currentTree, repoId, _repoDirectory)).Tree;
            }

            // Now, create all of the files in the tree
            StatusUpdate?.Invoke("Uploading compiled pages and adding to tree...", StatusType.Status);
            await _context.CompiledPages.ToList().ForEachAsync(async page =>
            {
                NewBlob newBlob = new NewBlob
                {
                    Content = MiscUtils.Base64Encode(page.Contents),
                    Encoding = EncodingType.Base64
                };
                BlobReference newBlobCreated = await _github.Git.Blob.Create(repoId, newBlob);
                string newBlobSha = newBlobCreated.Sha;
                newTree.Tree.Add(new NewTreeItem
                {
                    Mode = "100644",
                    Type = TreeType.Blob,
                    Sha = newBlobSha,
                    Path = Path.Join(_repoDirectory, page.Title + ".html")
                });
            });
            
            // Upload all of the files from the wwwroot directory
            StatusUpdate?.Invoke("Uploading other files and adding to tree...", StatusType.Status);
            (await CopyDirectoryIntoTree(_github, repoId, _repoDirectory)).ForEach(item => { newTree.Tree.Add(item); });

            StatusUpdate?.Invoke("Creating commit...", StatusType.Status);
            string newTreeSha = (await _github.Git.Tree.Create(repoId, newTree)).Sha;
            NewCommit newCommit = new NewCommit("Automated deploy from webweb", newTreeSha, masterReference.Object.Sha);
            Commit createdCommit = await _github.Git.Commit.Create(repoId, newCommit);
            await _github.Git.Reference.Update(repoId, headMasterRef, new ReferenceUpdate(createdCommit.Sha));

            StatusUpdate?.Invoke("Deploy complete.", StatusType.Success);
        }

        public void RefreshConfig()
        {
            _githubToken = _context.Configuration.Find("github-token").Contents;
            if (_githubToken != string.Empty)
            {
                _github = new GitHubClient(new ProductHeaderValue("webweb2"),
                    new InMemoryCredentialStore(new Credentials(_githubToken)));
            }

            _repoName = _context.Configuration.Find("github-reponame").Contents;
            _repoDestination = _context.Configuration.Find("github-destination").Contents;

            switch (_repoDestination)
            {
                case "master":
                {
                    _repoBranch = "master";
                    _repoDirectory = string.Empty;
                    break;
                }
                case "docs":
                {
                    _repoBranch = "master";
                    _repoDirectory = "docs";
                    break;
                }
                case "gh-pages":
                {
                    _repoBranch = "gh-pages";
                    _repoDirectory = string.Empty;
                    break;
                }
            }
        }

        private static async Task<NewTreeWithPath> CopySubtree(GitHubClient github, TreeResponse tree, long repoId, string excludePath, string treePath = "", int recursionLevel = 0)
        {
            NewTree newTree = new NewTree();
            var trees = tree.Tree.Where(x => x.Type == TreeType.Tree);
            var blobs = tree.Tree.Where(x => x.Type != TreeType.Tree);
            blobs.Select(x => new NewTreeItem
            {
                Path = x.Path,
                Mode = x.Mode,
                Type = x.Type.Value,
                Sha = x.Sha
            }).ToList().ForEach(x => newTree.Tree.Add(x));
            await (await trees.SelectAsync(async x => await CopySubtree(github, await github.Git.Tree.Get(repoId, x.Sha), repoId, excludePath, x.Path, recursionLevel + 1)))
                .ToList().ForEachAsync(async x =>
                {
                    NewTreeWithPath newTreeWithPath = x;
                    if (newTreeWithPath.Path == newTreeWithPath.ExcludePath) return;
                    string newSha = (await github.Git.Tree.Create(repoId, newTreeWithPath.Tree)).Sha;
                    newTree.Tree.Add(new NewTreeItem
                    {
                        Mode = "040000",
                        Type = TreeType.Tree,
                        Sha = newSha,
                        Path = newTreeWithPath.Path
                    });
                });
            return new NewTreeWithPath
            {
                Tree = newTree,
                Path = treePath,
                ExcludePath = recursionLevel > 1 ? string.Join('/', excludePath.Split("/").Skip(1)) : excludePath
            };
        }

        private static async Task<List<NewTreeItem>> CopyDirectoryIntoTree(GitHubClient github, long repoId, string repoDirectory, string path = "")
        {
            List<NewTreeItem> result = new List<NewTreeItem>();
            string fullPath = MiscUtils.MapPath(Path.Join("wwwroot", path));
            await Directory.GetFiles(fullPath).ToList().ForEachAsync(async file =>
            {
                NewBlob newBlob = new NewBlob
                {
                    Content = Convert.ToBase64String(await File.ReadAllBytesAsync(file)),
                    Encoding = EncodingType.Base64
                };
                BlobReference newBlobCreated = await github.Git.Blob.Create(repoId, newBlob);
                string newBlobSha = newBlobCreated.Sha;
                result.Add(new NewTreeItem
                {
                    Mode = "100644",
                    Type = TreeType.Blob,
                    Sha = newBlobSha,
                    Path = Path.Join(repoDirectory, MiscUtils.UnmapPath(file))
                });
            });
            await Directory.GetDirectories(fullPath).ToList().ForEachAsync(async directory =>
            {
                string unmappedPath = MiscUtils.UnmapPath(directory);
                if (unmappedPath == "webwebResources") return;
                List<NewTreeItem> directoryContents = await CopyDirectoryIntoTree(github, repoId, repoDirectory, unmappedPath);
                result = result.Concat(directoryContents).ToList();
            });

            return result;
        }
    }
}