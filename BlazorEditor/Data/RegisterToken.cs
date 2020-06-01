using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorEditor.Data
{
    public class RegisterToken
    {
        [Key]
        public string Token { get; set; }
        public DateTime Expire { get; set; }
    }
}