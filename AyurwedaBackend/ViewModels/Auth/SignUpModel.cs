using AyurwedaBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace AyurwedaBackend.Models
{
    public class SignUpModel
    {
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        public  string Email { get; set; }
        [Required]
        [Compare("ConfirmPassword")]
        public string Password { get; set; }    
        public string ConfirmPassword { get; set; }
        public string PhoneNumber { get; set; }
        public Gender Gender { get; set; }
        public string MedicalCounsilRegID { get; set; }
        public string Specialization { get; set; }
        public string Hospital { get; set; }
        public string Lane { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public string AvailableTimeFrom { get; set; }
        public string AvailableTimeTo { get; set; }
        public string ServiceType { get; set; }
        public bool Status { get; set; }
        public string Address { get; set; }
        public string MyRole { get; set; }
    }
}
