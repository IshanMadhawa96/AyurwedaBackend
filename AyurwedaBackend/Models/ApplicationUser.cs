using AyurwedaBackend.Enums;
using Microsoft.AspNetCore.Identity;
using System;

namespace AyurwedaBackend.Models
{
    public class ApplicationUser:IdentityUser
    {
        //set default value
        public ApplicationUser()
        {
            isCompleteProfile = false;
            Status = false;
        }
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
        public string Profile { get; set; }
        public bool isCompleteProfile { get; set; }
    }
}
