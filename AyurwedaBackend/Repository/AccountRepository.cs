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
using AyurwedaBackend.Services.Email;

namespace AyurwedaBackend.Repository
{
    //inherit IAccountRepository class using this AccountRepository:IAccountRepository
    public class AccountRepository:IAccountRepository
    {
        //constructer initilize varibles
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        //constructer
        public AccountRepository(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,RoleManager<IdentityRole> roleManager, IConfiguration configuration,IEmailService emailService)
        {
            this.userManager = userManager; 
            this.signInManager = signInManager;
            this.roleManager = roleManager; 
            _configuration = configuration;
            _emailService = emailService;
        }
        //Signup method
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
            string confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            SendEmail("verify", null, user, confirmationToken);
            return  regUser;

        }
        //Login Method
        public async Task<string> LoginAsync(LoginViewModel loginViewModel)
        {
            var my_role = "";
            //check user email in database
            var user = await userManager.FindByEmailAsync(loginViewModel.Email);

            //doing authentication logic
            if (user != null && await userManager.CheckPasswordAsync(user, loginViewModel.Password))
            {
                if (!user.EmailConfirmed)
                {
                    string confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    SendEmail("verify", null, user, confirmationToken);
                    return "0";
                }

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
        //Email confirm method
        public async Task<IdentityResult> EmailConfirmAsync(ConfirmEmail confirmEmail)
        {
            try
            {
                ApplicationUser user = await userManager.FindByIdAsync(confirmEmail.Userid.ToString());
                if (user == null)
                {
                    return null;
                }
                IdentityResult result = await userManager.ConfirmEmailAsync(user, confirmEmail.Token);

                if (!result.Succeeded)
                {
                    return null;
                }
                //SendEmail("verified", user.Email, null, null);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        //password reset link
        public async Task<string> ForgetPasswordAsync(ForgetPassword forgetPassword)
        {
            try
            {
                ApplicationUser user = await userManager.FindByEmailAsync(forgetPassword.Email);
                if (user == null)
                {
                    return null;
                }
                if (!user.EmailConfirmed)
                {
                    string confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    SendEmail("verify", null, user, confirmationToken);
                    return "0";
                }
                /*if (!(await userManager.IsEmailConfirmedAsync(user)))
                {
                    return null;
                }*/

                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                SendEmail("resetPass", null, user, token);
                return "1";
            }
            catch (Exception)
            {

                throw;
            }
        }
        //Reset Password
        public async Task<string> ResetPasswordAsync(ResetPassword resetPassword)
        {
            try
            {
                ApplicationUser user = await userManager.FindByIdAsync(resetPassword.Userid);
                if (user == null)
                {
                    return null;
                }
                IdentityResult result = await userManager.ResetPasswordAsync(user, resetPassword.Token, resetPassword.Password);
                if (!result.Succeeded)
                {
                    return null;
                }
                SendEmail("resetted", user.Email, null, null);
                return "1";
            }
            catch (Exception)
            {
                throw;
            }
        }
        //sending email
       
        private void SendEmail(string _type, string _email = null, ApplicationUser _user = null, string _token = null)
        {
            string subject = "";
            string html = "";
            string verifyUrl;
            string email = _user != null ? _user.Email : _email;

            try
            {
                switch (_type)
                {
                    case "verify":
                        verifyUrl = $"https://slvms.z13.web.core.windows.net/auth/confirm-email?userid={_user.Id}&token={_token}";
                        subject = "Sign-up Verification (Aurweda) - Verify Email";
                        html =
                        $@" <center><img
                                style=""width: 40%""
                                src='https://docs.google.com/uc?id=1kIvq5gRqlUM_y-Y-7KpQw3oGtuX7Im0A'
                                alt=''
                                />
                            <h2>
                                Please click the below button to <br /> verify your email
                            </h2>
                            <br />
                                <a
                                style=""
                                    border-radius: 5px;
                                    color: white;
                                    background-color: rgb(4, 128, 201);
                                    padding: 15px;
                                    border: none;
                                    letter-spacing: 0.1rem;
                                    text-transform: uppercase;
                                    text-decoration: none;
                                ""
                                href=""{verifyUrl}""
                                >
                                Verify email
                                </a></center>";
                        break;
                    case "resetPass":
                        verifyUrl = $"http://localhost:4200/confirm-email?userid={_user.Id}&token={_token}";
                        subject = "Aurweda - Reset password";
                        html = $@" <center>
                                <img
                                style=""width: 40%""
                                src='https://docs.google.com/uc?id=12MmOUkndXs65qf7kd6FCzV4iZGKPF16s'
                                alt=''
                                />
                            <h2 style=""
                                    color: black;
                                "">
                                Please click the below button to <br />
                                reset your password
                            </h2>
                            <br />
                                <a
                                style=""
                                    border-radius: 5px;
                                    color: white;
                                    background-color: rgb(255, 115, 0);
                                    padding: 15px;
                                    border: none;
                                    letter-spacing: 0.1rem;
                                    text-transform: uppercase;
                                    text-decoration: none;
                                ""
                                href=""{verifyUrl}""
                                >
                                Reset Password
                                </a>
                            </center>";
                        break;
                    case "verified":
                        verifyUrl = $"https://slvms.z13.web.core.windows.net/auth/reset-password?userid={_user.Id}&token={_token}";
                        subject = "Sign-up Verification-Ayurweda";
                        html =
                        $@" <center><img
                                style=""width: 40%""
                                src='https://docs.google.com/uc?id=1LqFsaoDVUdXQMUMoEZ8MkNTjDiYQp1FZ'
                                alt=''
                                />
                            <h2 style=""
                                    color: black;
                                "">
                                Your email verification is successfull
                            </h2>
                            <br />
                                <a
                                style=""
                                    border-radius: 5px;
                                    color: white;
                                    background-color: rgb(37, 199, 50);
                                    padding: 15px;
                                    border: none;
                                    letter-spacing: 0.1rem;
                                    text-transform: uppercase;
                                    text-decoration: none;
                                ""
                                href=""{verifyUrl}""
                                >
                                Continue to Login
                                </a></center>";
                        break;
                    case "resetted":
                        subject = "Password Reset Successfull";
                        html =
                        $@" <center>
                                <img
                                style=""width: 40%""
                                src='https://docs.google.com/uc?id=1tQNONuwfg5phj1teyBbG7W02lpQ6nPBi'
                                alt=''
                                />
                            <h2>
                                Password Resetted Successfully!
                            </h2>
                            <br />
                                <a
                                style=""
                                    border-radius: 5px;
                                    color: white;
                                    background-color: rgb(143, 179, 46);
                                    padding: 15px;
                                    border: none;
                                    letter-spacing: 0.1rem;
                                    text-transform: uppercase;
                                    text-decoration: none;
                                ""
                                href=""https://slvms.z13.web.core.windows.net/auth/login""
                                >
                                Continue to Login
                                </a>
                            </center>";
                        break;
                    case "newUser":
                        verifyUrl = $"https://slvms.z13.web.core.windows.net/auth/new-user-setup?userid={_user.Id}&token={_token}";
                        subject = "Ayurweda - New User Invitation";
                        html =
                        $@" <center>
                                <img
                                style=""width: 40%""
                                src='https://docs.google.com/uc?id=1ornFZghAE9F3kNLxmMYNo5F9H0azVKU3'
                                alt=''
                                />
                            <h2>
                                New User Setup
                            </h2>
                            <br />
                                <a
                                style=""
                                    border-radius: 5px;
                                    color: white;
                                    background-color: rgb(179, 80, 204);
                                    padding: 15px;
                                    border: none;
                                    letter-spacing: 0.1rem;
                                    text-transform: uppercase;
                                    text-decoration: none;
                                ""
                                href=""{verifyUrl}""
                                >
                                Continue to Login
                                </a>
                            </center>";
                        break;
                    case "newUserSetup":
                        subject = "Password Setup Successfull";
                        html =
                        $@" <center>
                                <img
                                style=""width: 40%""
                                src='https://docs.google.com/uc?id=1GhZJQfcGeJhxZ0_kPh2MrfSMe9izMsi-'
                                alt=''
                                />
                            <h2>
                                Password of new user account <br />
                                setup successfully!
                            </h2>
                            <br />
                                <a
                                style=""
                                    border-radius: 5px;
                                    color: white;
                                    background-color: rgb(104, 107, 109);
                                    padding: 15px;
                                    border: none;
                                    letter-spacing: 0.1rem;
                                    text-transform: uppercase;
                                    text-decoration: none;
                                ""
                                href=""https://slvms.z13.web.core.windows.net/auth/login""
                                >
                                Continue to Login
                                </a>
                        </center>";
                        break;
                    case "passwordChanged":
                        subject = "Password Change Successfull";
                        html =
                        $@" <center><img
                                style=""width: 40%""
                                src='https://docs.google.com/uc?id=10uumtpFjMuE7CIXYiQjeKmNPMIhr1YkX'
                                alt=''
                                />
                            <h2>
                                Password change successfull!
                            </h2>
                            <br />
                                <a
                                style=""
                                    border-radius: 5px;
                                    color: white;
                                    background-color: rgba(245, 55, 91, 1);
                                    padding: 15px;
                                    border: none;
                                    letter-spacing: 0.1rem;
                                    text-transform: uppercase;
                                    text-decoration: none;
                                ""
                                href=""https://slvms.z13.web.core.windows.net/auth/login""
                                >
                                Continue to Login
                                </a><center>";
                        break;

                }

                _emailService.Send(
                    to: email,
                    subject: subject,
                    html: html
                );

            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
