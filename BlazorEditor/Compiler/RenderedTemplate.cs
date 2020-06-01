using System.Collections.Generic;

namespace BlazorEditor.Compiler
{
    public class RenderedTemplate
    {
        public string HtmlContents { get; set; }
        public string InnerJsContents { get; set; }
        public List<string> ComponentDependencies { get; set; }
        public string Layout { get; set; }
        public Dictionary<string, dynamic> ViewData { get; set; }
    }
}