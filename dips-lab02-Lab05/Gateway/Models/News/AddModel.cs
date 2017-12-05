using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Models.News
{
    public class AddModel
    {
        public string Username { get; set; }
        [Required]
        [DataType(DataType.Text, ErrorMessage = "Input header")]
        [RegularExpression(@"[a-zA-Z0-9 ]+", ErrorMessage = "Input correct header (only letters, numbers and spaces are allowed)")]
        public string Header { get; set; }
        [Required]
        [DataType(DataType.Text, ErrorMessage = "Input body")]
        public string Body { get; set; }
    }
}
