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
    }
}
