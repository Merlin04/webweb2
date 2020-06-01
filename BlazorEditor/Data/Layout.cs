using System.ComponentModel.DataAnnotations;

namespace BlazorEditor.Data
{
    public class Layout
    {
        [Key]
        public string Title { get; set; }
        public string Contents { get; set; }
    }
}