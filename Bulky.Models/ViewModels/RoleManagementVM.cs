﻿using Bulky.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models.ViewModels
{
    public class RoleManagementVM
    {
       
        public ApplicationUser AppUser { get; set; }
        public IEnumerable<SelectListItem> CompanyList { get; set; }

        public IEnumerable<SelectListItem> RoleList { get; set; }

    }
}
