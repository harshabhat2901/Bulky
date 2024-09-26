using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Bulky.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        public UserController(ApplicationDbContext dbContext, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _db = dbContext;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
         return View();
        }
        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {
            var userFromDB = _db.Users.FirstOrDefault(x => x.Id == id);
            if(userFromDB == null)
            {
                return Json(new { success = false, Message = "Error while Locking/unlocking" });
            }

            if(userFromDB.LockoutEnd != null && userFromDB.LockoutEnd > DateTime.Now)
            {
                //User is currently locked need to be unlocked
                userFromDB.LockoutEnd = DateTime.Now;
            }
            else
            {
                //User is currently  unlocked to be locked
                userFromDB.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _db.SaveChanges();
            return Json (new {success = true, message="Operation Successfull"});
        }

        public IActionResult RoleManagement(string userId)
        {
            var roleid = _db.UserRoles.FirstOrDefault(u => u.UserId == userId).RoleId;


            RoleManagementVM roleMgmtVM = new()
            {
                AppUser = _db.ApplicationUsers.Include(u=> u.Company).FirstOrDefault(u=> u.Id == userId),               
                RoleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
                {
                    Text = i,
                    Value = i

                }),
                CompanyList = _db.Companies.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()

                })

            };
            roleMgmtVM.AppUser.Role = _db.Roles.FirstOrDefault(u => u.Id == roleid).Name;
            return View(roleMgmtVM);
        }

        [HttpPost]
        public IActionResult RoleManagement(RoleManagementVM vM)
        {
            try
            {
               
                
                //var roleId = _db.Roles.FirstOrDefault(u => u.Name == vM.Role).Id;
                var userRole = _db.UserRoles.FirstOrDefault(u => u.UserId == vM.AppUser.Id).RoleId;

                string oldRole = _db.Roles.FirstOrDefault(u => u.Id == userRole).Name;

                if(!(vM.AppUser.Role == oldRole))
                {
                    //ROle is changed need to update and save
                    ApplicationUser applicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == vM.AppUser.Id);
                    if(vM.AppUser.Role == SD.ROLE_COMPANY)
                    {
                        applicationUser.CompanyId = vM.AppUser.CompanyId;
                    }
                    if (oldRole == SD.ROLE_COMPANY)
                        applicationUser.CompanyId = null;

                    _db.SaveChanges();

                    _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                    _userManager.AddToRoleAsync(applicationUser, vM.AppUser.Role).GetAwaiter().GetResult();
                }
                
                TempData["Success"] = "User Role Updated successfully";

            }
            catch (Exception ex)
            {
                //return Json(new { success = false, message = "Error while updating " });
            }
            return RedirectToAction("Index");

        }


        #region APICall
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> userList = _db.ApplicationUsers.Include(u=> u.Company).ToList();
            var userRoles = _db.UserRoles.ToList();
            var role = _db.Roles.ToList();

            foreach (var user in userList)
            {
                var roleId= userRoles.FirstOrDefault(u=> u.UserId == user.Id).RoleId;
                user.Role= role.FirstOrDefault(u=> u.Id==roleId).Name;
            }


            return Json(new {data = userList });
        }

        #endregion
        
       
    }
}
