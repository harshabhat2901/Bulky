using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _uow = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> products = _uow.Product.GetAll().ToList();
           
            return View(products);
        }

        public IActionResult Upsert(int? id)
        {

            //ViewBag.CategoryList = categoryList;
            //ViewData["CategoryList"] = categoryList;
            ProductVM prdVM = new()
            {
                Product = new Product(),
                CategoryList = _uow.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
            };
            if (!(id == null || id == 0))
            {

                prdVM.Product = _uow.Product.Get(u => u.Id == id);
               
            }
            return View(prdVM);
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM prdVM, IFormFile? file )
        {
            //if (cat.Name == cat.DisplayOrder.ToString())
            //{
            //    ModelState.AddModelError("Name", "Name cannot be same as Display Order");
            //}

            if (ModelState.IsValid)
            {
                if(file !=null)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string folderpath = Path.Combine(_webHostEnvironment.WebRootPath, @"Images\Product");

                    if(!string.IsNullOrEmpty(prdVM.Product.ImageURL)) 
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, prdVM.Product.ImageURL.TrimStart('\\'));
                        if(System.IO.File.Exists(oldImagePath))
                            System.IO.File.Delete(oldImagePath);
                    }

                    using (var filestream = new FileStream(Path.Combine(folderpath, filename), FileMode.Create))
                    {
                        file.CopyTo(filestream);
                    }
                    prdVM.Product.ImageURL= Path.Combine(@"\Images\Product", filename);

                }
                if(prdVM.Product.Id == 0)
                 {
                    _uow.Product.Add(prdVM.Product);
                }
                else
                    _uow.Product.Update(prdVM.Product);
                _uow.Save();
                TempData["Success"] = "Product Created Successfully";
                return RedirectToAction("Index");
            }
            else
            {
                prdVM.CategoryList = _uow.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(prdVM);
            }
            
        }


        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            var prdct = _uow.Product.Get(u => u.Id == id);
            if (prdct == null)
                return NotFound();

            return View(prdct);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Product? prdct = _uow.Product.Get(u => u.Id == id);
            if (prdct == null)
                return NotFound();
            _uow.Product.Remove(prdct);
            _uow.Save();
            TempData["Success"] = "Product Deleted Successfully";
            return RedirectToAction("Index");

        }
    }
}
