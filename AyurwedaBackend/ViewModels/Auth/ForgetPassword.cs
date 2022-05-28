using System.ComponentModel.DataAnnotations;

namespace AyurwedaBackend.ViewModels.Auth
{
    public class ForgetPassword
    {
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
    }
}
