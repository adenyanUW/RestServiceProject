using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UserService.Tests.Models
{
    public class User
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("email")]
        [Required]
        [DisplayName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("password")]
        [Required]
        [DisplayName("password")]
        public string? Password { get; set; }

        [JsonPropertyName("createdDate")]
        public DateTime? CreatedDate { get; set; }
    }
}
