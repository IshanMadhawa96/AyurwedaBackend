using System.ComponentModel.DataAnnotations;

namespace AyurwedaBackend.ViewModels.Auth
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required"),EmailAddress(ErrorMessage = "Please Enter Valid Email")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
