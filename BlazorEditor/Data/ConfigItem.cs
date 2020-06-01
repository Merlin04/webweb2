using System.ComponentModel.DataAnnotations;

namespace BlazorEditor.Data
{
    public class ConfigItem
    {
        [Key]
        public string Title { get; set; }
        public string Contents { get; set; }
    }
}