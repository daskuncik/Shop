using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Models.News
{
    public class IndexModel
    {
        [Required]
        [DataType(DataType.Text, ErrorMessage = "Input username")]
        [RegularExpression(@"[a-zA-Z0-9]+", ErrorMessage = "Input correct username (Only letters and numbers are allowed)")]
        public string Username { get; set; }
        public List<Tuple<string, string, string>> News { get; set; } = new List<Tuple<string, string, string>>();
        public int Page { get; set; }
        public int Size { get; set; }
        public int MaxPage { get; set; }
    }
}
