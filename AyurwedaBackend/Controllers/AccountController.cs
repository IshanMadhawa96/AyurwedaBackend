
using AyurwedaBackend.Models;
using AyurwedaBackend.Repository;
using AyurwedaBackend.ViewModels.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AyurwedaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
       private readonly IAccountRepository _accountRepository;
       private readonly UserManager<ApplicationUser> userManager;
        public AccountController(IAccountRepository accountRepository, UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
            _accountRepository = accountRepository;
        }
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpModel signupmodel)
        {
            var userExist = await userManager.FindByEmailAsync(signupmodel.Email);
            if (userExist != null)
                return BadRequest();
            var result = await _accountRepository.SignUpAsync(signupmodel);
            if (result.Succeeded)
            {
                return Ok();
            }
            return Unauthorized();

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel loginViewModel)
        {
           
            var result = await _accountRepository.LoginAsync(loginViewModel);
            if (string.IsNullOrEmpty(result))
            {
                return Unauthorized();
            }
            return Ok(result);

        }
    }
}
