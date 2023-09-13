using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserService.Data;
using UserService.Models;
using UserService.Security;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UserService.Controllers
{
    [Route("api/users")]
    [ApiController]
    // [Authenticator]
    public class UserServiceController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserServiceController> _logger;

        public UserServiceController(ILogger<UserServiceController> logger, IUserRepository userRepository)
        {
            _logger = logger;
            _logger.LogInformation("INFO");
            _logger.LogWarning("WARNING");
            _logger.LogError("ERROR");
            _userRepository = userRepository;
        }

        // GET: api/<UserServiceController>
        [HttpGet]
        [Authenticator]
        public IActionResult Get()
        {
            return Ok(_userRepository.Users);
        }

        // GET api/<UserServiceController>/5
        [HttpGet("{id}")]
        [Authenticator]
        public IActionResult Get(int id)
        {
            User? requestedUser = _userRepository.GetUser(id);

            if (requestedUser == null) 
            {
                return NotFound(null);
            }
            else
            {
                return Ok(requestedUser);
            }
        }

        // POST api/<UserServiceController>
        [HttpPost]
        public IActionResult Post([FromBody] User value)
        {
            _userRepository.Add(value);
            return CreatedAtAction(nameof(Post), new { id = value.Id }, value);
        }

        // PUT api/<UserServiceController>/5
        [HttpPut("{id}")]
        [Authenticator]
        public IActionResult Put(int id, [FromBody] User value)
        {
            User? updatedUser = _userRepository.Update(id, value);

            if (updatedUser == null)
            {
                return NotFound(null);
            }

            return Ok(updatedUser);
        }

        // DELETE api/<UserServiceController>/5
        [HttpDelete("{id}")]
        [Authenticator]
        public IActionResult Delete(int id)
        {
            if (!_userRepository.Remove(id))
            {
                return NotFound(null);
            }
            else
            {
                return Ok();
            }
        }
    }
}
