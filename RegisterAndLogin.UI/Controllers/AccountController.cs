using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using RegisterAndLogin.UI.EMailServices;
using RegisterAndLogin.UI.Models;

namespace RegisterAndLogin.UI.Controllers
{
    [AutoValidateAntiforgeryToken]


    public class AccountController : Controller
    {
        private UserManager<User> userManager;
        private SignInManager<User> signInManager;
        private IEmailSmtp emailSmtp;

        public AccountController(UserManager<User> _userManager, SignInManager<User> _signInManager, IEmailSmtp _emailSmtp)
        {
            userManager = _userManager;
            signInManager = _signInManager;
            emailSmtp = _emailSmtp;
        }

        public IActionResult Login(string ReturnUrl=null)
        {
            return View(new LoginModel()
            {
                ReturnUrl = ReturnUrl
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel user)
        {
            if(!ModelState.IsValid)
            {
                return View(user);
            }

            var model = await userManager.FindByEmailAsync(user.Email);

            if(model==null)
            {
                ModelState.AddModelError("", "No account has been created before with this email.");
                return View(user);
            }

            if(!await userManager.IsEmailConfirmedAsync(model))
            {
                ModelState.AddModelError("", "Please confirm email");
                return View(user);
            }

            var result = await signInManager.PasswordSignInAsync(model, user.Password, false, false);

            if(result.Succeeded)
            {
                return Redirect(user.ReturnUrl ?? "~/");
            }

            ModelState.AddModelError("", "Email or password is incorrect.");

            return View(user);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel user)
        {
            if(!ModelState.IsValid)
            {
                return View(user);
            }

            var model = new User()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.Email,
            };

            var eMail = await userManager.FindByEmailAsync(user.Email);

            if (eMail != null)
            {
                ModelState.AddModelError("", "This email account is registered.");
                return View(user);
            }

            var result = await userManager.CreateAsync(model, user.Password);

            if (result.Succeeded)
            {
                var code = await userManager.GenerateEmailConfirmationTokenAsync(model);
                var url = Url.Action("ConfirmEmail", "Account", new
                {
                    userId = model.Id,
                    token = code,
                });

                await emailSmtp.SendEmailAsync(user.Email, "Please confirm account", $"Please <a href='https://localhost:44346{url}'>click</a> the link for account verify");

                return RedirectToAction("Login", "Account");
            }

            ModelState.AddModelError("Password", "The password must be at least 6 characters and contain uppercase letter.");

            return View(user);
         }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Redirect("~/");
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if(userId == null || token == null)
            {
                ModelState.AddModelError("", "Invalid token");
                return View();
            }

            var user = await userManager.FindByIdAsync(userId);

            if(user!=null)
            {
                var result = await userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    ModelState.AddModelError("", "No user");
                    return View();
                }
            }
            ModelState.AddModelError("", "No user");
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if(string.IsNullOrEmpty(email))
            {
                return View();
            }

            var user = await userManager.FindByEmailAsync(email);

            if(user==null)
            {
                return View();
            }

            var code = await userManager.GeneratePasswordResetTokenAsync(user);

            var url = Url.Action("ResetPassword", "Account", new
            {
                userId = user.Id,
                token = code,
            });

            await emailSmtp.SendEmailAsync(email, "Reset Password", $"Please <a href='https://localhost:44346{url}'>click</a> the link for reset password");

            return View();
        }

        public IActionResult ResetPassword(string userId, string token)
        {
            if(userId==null || token==null)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new ResetPassword
            {
                UserId = userId,
                Token = token
            };

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPassword user)
        {
            if(!ModelState.IsValid)
            {
                return View(user);
            }

            var model = await userManager.FindByIdAsync(user.UserId);

            if(model==null)
            {
                return RedirectToAction("Index", "Home");
            }

            var result = await userManager.ResetPasswordAsync(model, user.Token, user.Password);

            if(result.Succeeded)
            {
                return RedirectToAction("Login", "Account");
            }
            
            return View(user);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}