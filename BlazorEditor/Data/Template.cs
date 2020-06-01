using System.ComponentModel.DataAnnotations;

namespace BlazorEditor.Data
{
    public class Template
    {
        [Key]
        public string Title { get; set; }
        public string HtmlContents { get; set; }
        public string CssContents { get; set; }
        public string JsContents { get; set; }
    }
}