using System.ComponentModel.DataAnnotations;

namespace AyurwedaBackend.ViewModels.Auth
{
    public class ConfirmEmail
    {
        [Required(ErrorMessage = "User Id is Required")]
        public string Userid { get; set; }

        [Required(ErrorMessage = "Token is Required")]
        public string Token { get; set; }
    }
}
