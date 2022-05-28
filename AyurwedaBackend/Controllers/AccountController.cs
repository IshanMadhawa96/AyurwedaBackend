
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
           /*var user = await userManager.GetUserAsync(HttpContext.User);
            
            if ()
            {
                return BadRequest();
            }*/
           

            var result = await _accountRepository.LoginAsync(loginViewModel);
            if (string.IsNullOrEmpty(result))
            {
                return Unauthorized();
            }
            if (result == "0")
            {
                return Forbid(); 
            }
            return Ok(result);

        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> EmailConfirm([FromBody] ConfirmEmail confirmEmail)
        {

            var result = await _accountRepository.EmailConfirmAsync(confirmEmail);
            if (result==null)
            {
                return BadRequest();
                //return StatusCode(StatusCodes.Status500InternalServerError);
            }
           
            return Ok(result);

        }
        [HttpPost("forgot-Password")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPassword forgetPassword)
        {
            var result = await _accountRepository.ForgetPasswordAsync(forgetPassword);
            if (result == null)
            {
                return BadRequest();
                //return StatusCode(StatusCodes.Status500InternalServerError);
            }
            if (result == "0")
            {
                return Forbid();
            }
            return Ok();

        }
        [HttpPost("reset-Password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPassword resetPassword)
        {
            var result = await _accountRepository.ResetPasswordAsync(resetPassword);
            if (result == null)
            {
                return Forbid();
            }
            return Ok();
        }
       


    }
}
