using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AyurwedaBackend.Models;
using AyurwedaBackend.ViewModels.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Linq;

namespace AyurwedaBackend.Repository
{
    //inherit IAccountRepository class using this AccountRepository:IAccountRepository
    public class AccountRepository:IAccountRepository
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;
        public AccountRepository(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this.userManager = userManager; 
            this.signInManager = signInManager;
            this.roleManager = roleManager; 
            _configuration = configuration;
        }
        public async Task<IdentityResult> SignUpAsync(SignUpModel signupmodel)
        {
            ApplicationUser user = new ApplicationUser()
            {
                UserName = signupmodel.UserName,
                Email = signupmodel.Email,
                PhoneNumber = signupmodel.PhoneNumber,
                Gender = signupmodel.Gender,
                MedicalCounsilRegID = signupmodel.MedicalCounsilRegID,
                Specialization = signupmodel.Specialization,
                Hospital = signupmodel.Hospital,
                Lane = signupmodel.Lane,
                Province = signupmodel.Province,
                District = signupmodel.District,
                AvailableTimeFrom = signupmodel.AvailableTimeFrom,
                AvailableTimeTo = signupmodel.AvailableTimeTo,
                ServiceType = signupmodel.ServiceType,
                Address = signupmodel.Address,

            };

             var regUser = await userManager.CreateAsync(user,signupmodel.Password);
           if(!await roleManager.RoleExistsAsync(UserRoles.Admin))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
           if(!await roleManager.RoleExistsAsync(UserRoles.Doctor))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.Doctor));
           if (!await roleManager.RoleExistsAsync(UserRoles.Patient))
                await roleManager.CreateAsync(new IdentityRole(UserRoles.Patient));
           if(signupmodel.MyRole== "Admin")
            {
                await userManager.AddToRoleAsync(user, UserRoles.Admin);
            }
            else if (signupmodel.MyRole == "Doctor")
            {
                await userManager.AddToRoleAsync(user, UserRoles.Doctor);
            }
            else if(signupmodel.MyRole == "Patient")
            {
                await userManager.AddToRoleAsync(user, UserRoles.Patient);
            }
            else
            {
                await userManager.AddToRoleAsync(user, UserRoles.Patient);
            }
            return  regUser;

        }

        public async Task<string> LoginAsync(LoginViewModel loginViewModel)
        {
            var my_role = "";
            //check user email in database
            var user = await userManager.FindByEmailAsync(loginViewModel.Email);
            //doing authentication logic
            if (user != null && await userManager.CheckPasswordAsync(user, loginViewModel.Password))
            {
                var userRoles = await userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                };
                foreach(var userRole in userRoles)
                {
                    my_role =  userRole;
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(24),
                    claims: authClaims,
                    signingCredentials:new SigningCredentials(authSigningKey,SecurityAlgorithms.HmacSha256)
                );
                //return CreatedAtActionResult();
                /*var user  = Ok(new
                 {
                     "token" = new JwtSecurityTokenHandler().WriteToken(token),

                 });*/
                var opts = new Dictionary<string, string>();
                opts.Add("token", new JwtSecurityTokenHandler().WriteToken(token));
                opts.Add("id", user.Id);
                opts.Add("name", user.UserName);
                opts.Add("email", user.Email);
                opts.Add("status", user.Status.ToString());
                opts.Add("MedicalCounsilRegID", user.MedicalCounsilRegID);
                opts.Add("Gender", user.Gender.ToString());
                opts.Add("Specialization", user.Specialization);
                opts.Add("Hospital", user.Hospital);
                opts.Add("Lane", user.Lane);
                opts.Add("Province", user.Province);
                opts.Add("District", user.District);
                opts.Add("AvailableTimeFrom", user.AvailableTimeFrom.ToString());
                opts.Add("AvailableTimeTo", user.AvailableTimeTo.ToString());
                opts.Add("ServiceType", user.ServiceType);
                opts.Add("Address", user.Address);
                opts.Add("Profile", user.Profile);
                opts.Add("isCompleteProfile", user.isCompleteProfile.ToString());
                opts.Add("Role", my_role);
                var list = opts.Select(p => new Dictionary<string, string>() { { p.Key, p.Value } });
                var user_info = JsonConvert.SerializeObject(list);
                return user_info;



            }
            return null;
        }
    }
}
