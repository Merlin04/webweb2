using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BlazorEditor.Shared;
using BlazorEditor.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlazorEditor.Pages
{
    public class UploadFile : PageModel
    {
        public IFormFile FileUpload { get; set; }
        [BindProperty]
        public string FileDirectory { get; set; }
        
        public void OnGet()
        {
            
        }

        public async Task OnPostAsync()
        {
            if (!User.Identity.IsAuthenticated) return;
            string savedFilePath = Path.Combine(MiscUtils.MapPath("wwwroot/"),
                Regex.Replace(FileDirectory, "^\\/*", string.Empty), FileUpload.FileName.Replace("/", string.Empty));
            if (MiscUtils.IsDirectoryTraversal(savedFilePath)) return;
            await using var stream = System.IO.File.Create(savedFilePath);
            await FileUpload.CopyToAsync(stream);
        }
    }
}