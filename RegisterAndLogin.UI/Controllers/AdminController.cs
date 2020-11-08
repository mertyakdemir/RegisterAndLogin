using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RegisterAndLogin.UI.Models;

namespace RegisterAndLogin.UI.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private RoleManager<IdentityRole> roleManager;
        private UserManager<User> userManager;


        public AdminController(RoleManager<IdentityRole> _roleManager, UserManager<User> _userManager)
        {
            roleManager = _roleManager;
            userManager = _userManager;
        }

        public IActionResult UserList()
        {
            return View(userManager.Users);
        }

        public IActionResult RoleList()
        {
            return View(roleManager.Roles);
        }

        public async Task<IActionResult> EditUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if(user!=null)
            {
                var selectedRoles = await userManager.GetRolesAsync(user);
                var roles = roleManager.Roles.Select(i => i.Name);

                ViewBag.Roles = roles;
                return View(new UserDetail()
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    SelectedRoles = selectedRoles
                });
            }
            return Redirect("~/admin/userlist");
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(UserDetail userDetail, string[] selectedRoles)
        {
            if(ModelState.IsValid)
            {
                var user = await userManager.FindByIdAsync(userDetail.UserId);
                if(user!=null)
                {
                    user.FirstName = userDetail.FirstName;
                    user.LastName = userDetail.LastName;
                    user.Email = userDetail.Email;
                    user.EmailConfirmed = userDetail.EmailConfirmed;

                    var result = await userManager.UpdateAsync(user);

                    if(result.Succeeded)
                    {
                        var userRoles = await userManager.GetRolesAsync(user);
                        selectedRoles = selectedRoles ?? new string[] { };
                        await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles).ToArray<string>());
                        await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles).ToArray<string>());

                        return Redirect("/admin/userlist");
                    }
                }
                return Redirect("/admin/userlist");
            }
            return View(userDetail);
        }

        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(RoleModel role)
        {
            if (ModelState.IsValid)
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role.Name));
                if (result.Succeeded)
                {
                    return RedirectToAction("RoleList");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(role);
        }

        public async Task<IActionResult> EditRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);

            var members = new List<User>();
            var nonmembers = new List<User>();

            foreach (var user in userManager.Users)
            {
                var list = await userManager.IsInRoleAsync(user, role.Name) ? members : nonmembers;
                list.Add(user);
            }

            var model = new RoleDetails()
            {
                Role = role,
                Users = members,
                NonUsers = nonmembers,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(RoleEditModel role)
        {
            if(ModelState.IsValid)
            {
                foreach (var userId in role.IdsToAdd ?? new string[]{})
                {
                    var user = await userManager.FindByIdAsync(userId);
                    if(user!=null)
                    {
                        var result = await userManager.AddToRoleAsync(user, role.RoleName);
                        if(!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                        }
                    }
                }

                foreach (var userId in role.IdsToRemove ?? new string[]{})
                {
                    var user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        var result = await userManager.RemoveFromRoleAsync(user, role.RoleName);
                        if (!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                        }
                    }
                }
            }

            return Redirect("/admin/editrole/"+role.RoleId);
        }
    }
}