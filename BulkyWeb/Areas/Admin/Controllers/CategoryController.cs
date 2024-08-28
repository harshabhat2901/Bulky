using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _uow;

        public CategoryController(IUnitOfWork db)
        {
            _uow = db;
        }
        public IActionResult Index()
        {
            List<Category> categories = _uow.Category.GetAll().ToList();
            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category cat)
        {
            if (cat.Name == cat.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Name cannot be same as Display Order");
            }

            if (ModelState.IsValid)
            {
                _uow.Category.Add(cat);
                _uow.Save();
                TempData["Success"] = "Category Created Successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            var category = _uow.Category.Get(u => u.Id == id);
            if (category == null)
                return NotFound();

            return View(category);
        }
        [HttpPost]
        public IActionResult Edit(Category cat)
        {


            if (ModelState.IsValid)
            {
                _uow.Category.Update(cat);
                _uow.Save();
                TempData["Success"] = "Category Updated Successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            var category = _uow.Category.Get(u => u.Id == id);
            if (category == null)
                return NotFound();

            return View(category);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Category? category = _uow.Category.Get(u => u.Id == id);
            if (category == null)
                return NotFound();
            _uow.Category.Remove(category);
            _uow.Save();
            TempData["Success"] = "Category Deleted Successfully";
            return RedirectToAction("Index");

        }
    }
}
