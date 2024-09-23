using Bulky.DataAccess.Data;
using Bulky.Models.Models;
using Bulky.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.DBInitializer
{
    public class DBInitilizer : IDBInitializer
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _db;

        public DBInitilizer(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, ApplicationDbContext db)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
        }

        public void Initialize()
        {
            //Complete all the migration activities
            try { 
               if(_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }

            } 
            catch (Exception ex) { }

            // Create Roles if not already created
            if (!_roleManager.RoleExistsAsync(SD.ROLE_CUSTOMER).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.ROLE_CUSTOMER)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.ROLE_ADMIN)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.ROLE_COMPANY)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.ROLE_EMPLOYEE)).GetAwaiter().GetResult();

                //Create Admin user if not already created

                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@HarshaUdemy.com",
                    Email = "admin@HarshaUdemy.com",
                    Name = "Harsha",
                    PhoneNumber = "9632217542",
                    StreetAddress = "LTTS Gate1",
                    State = "Karnataka",
                    PostalCode = "570017",
                    City = "Mysuru"
                }, "Admin123*").GetAwaiter().GetResult(); 
                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@HarshaUdemy.com");
                _userManager.AddToRoleAsync(user, SD.ROLE_ADMIN).GetAwaiter().GetResult();

            }
           
        }
    }
}
