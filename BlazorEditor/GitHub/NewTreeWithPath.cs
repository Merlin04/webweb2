using Octokit;

namespace BlazorEditor.GitHub
{
    public class NewTreeWithPath
    {
        public NewTree Tree { get; set; }
        public string Path { get; set; }
        public string ExcludePath { get; set; }
    }
}