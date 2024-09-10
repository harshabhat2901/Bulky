using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Bulky.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.ROLE_ADMIN)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public CompanyController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _uow = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Company> companies = _uow.Company.GetAll().ToList();
           
            return View(companies);
        }

        public IActionResult Upsert(int? id)
        {
            if (!(id == null || id == 0))
            {

                return View(_uow.Company.Get(u => u.Id == id));
            }

            else
                return View(new Company());
        }
        [HttpPost]
        public IActionResult Upsert(Company cmpny )
        {
          
            if (ModelState.IsValid)
            {

                if (cmpny.Id == 0)
                {
                    _uow.Company.Add(cmpny);
                    TempData["Success"] = "Company Created Successfully";
                }
                else
                {
                    _uow.Company.Update(cmpny);
                    TempData["Success"] = "Company edited Successfully";
                }
                _uow.Save();
                
                return RedirectToAction("Index");
            }
            else
            {
                return View(cmpny);
            }
            
        }



        #region APICall
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> companies = _uow.Company.GetAll().ToList();
            return Json(new {data = companies });
        }
        #endregion
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            Company companyToBeDeleted = _uow.Company.Get(u=> u.Id == id);
            if(companyToBeDeleted == null)
            {
                return Json(new {success = false, message = "Error While deleting"});

            }  
            _uow.Company.Remove(companyToBeDeleted);
            _uow.Save();
            return Json(new { success = true, message = "Deleted Successfully" });
        }
       
    }
}
