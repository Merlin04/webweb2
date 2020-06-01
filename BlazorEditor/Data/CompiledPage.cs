using System.ComponentModel.DataAnnotations;

namespace BlazorEditor.Data
{
    public class CompiledPage
    {
        [Key]
        public string Title { get; set; }
        public string Contents { get; set; }
    }
}