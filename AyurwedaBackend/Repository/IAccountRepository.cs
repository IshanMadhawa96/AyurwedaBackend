using AyurwedaBackend.Models;
using AyurwedaBackend.ViewModels.Auth;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace AyurwedaBackend.Repository
{
    public interface IAccountRepository
    {
        Task<IdentityResult> SignUpAsync(SignUpModel signupmodel);
        Task<string> LoginAsync(LoginViewModel loginViewModel);
        Task<IdentityResult> EmailConfirmAsync(ConfirmEmail confirmEmail);
        Task<string> ForgetPasswordAsync(ForgetPassword forgetPassword);
        Task<string> ResetPasswordAsync(ResetPassword resetPassword);
    }
}
