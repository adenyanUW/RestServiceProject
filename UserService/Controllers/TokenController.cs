using Microsoft.AspNetCore.Mvc;
using UserService.Data;
using UserService.Security;

namespace UserService.Controllers
{
    public class TokenRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    [Route("api/token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public TokenController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // This should require SSL
        [HttpPost]
        public dynamic? Post([FromBody] TokenRequest tokenRequest)
        {
            string? token = TokenHelper.GetToken(tokenRequest.Email, tokenRequest.Password, _userRepository);

            if (token != null)
            {
                // ToDo: try return Ok(null);
                return Ok(new { Token = token });
            }
            else
            {
                return NotFound(null);
            }
        }

        // This should require SSL
        [HttpGet]
        public dynamic Get(string userName, string password)
        {
            string? token = TokenHelper.GetToken(userName, password, _userRepository);

            if (token != null)
            {
                // ToDo: try return Ok(null);
                return Ok(new { Token = token });
            }
            else
            {
                return NotFound(null);
            }
        }
    }
}
