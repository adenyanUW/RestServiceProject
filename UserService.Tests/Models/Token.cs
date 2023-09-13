using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UserService.Tests.Models
{
    public class Token
    {
        [JsonPropertyName("token")]
        public string TokenString { get; set; }
    }
}
