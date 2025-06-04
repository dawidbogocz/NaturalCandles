using Microsoft.AspNetCore.Mvc;
using NaturalCandles.DataAccess.Data;
using NaturalCandles.DataAccess.Repository.IRepository;
using NaturalCandles.Models;

namespace NaturalCandles.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Category> objCategoryList = _unitOfWork.Category.GetAll().ToList();
            return View(objCategoryList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category obj)
        {

            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category created succesfully";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Category? categoryFromdb = _unitOfWork.Category.Get(u => u.CategoryName == id);
            //Category? categoryFromdb1 = _db.Categories.FirstOrDefault(u=>u.CategoryId==id);
            //Category? categoryFromdb2 = _db.Categories.Where(u => u.CategoryId == id).FirstOrDefault();
            if (categoryFromdb == null)
            {
                return NotFound();
            }
            return View(categoryFromdb);
        }
        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category updated succesfully";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Category? categoryFromdb = _unitOfWork.Category.Get(u => u.CategoryName == id);
            if (categoryFromdb == null)
            {
                return NotFound();
            }
            return View(categoryFromdb);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(string? id)
        {
            Category? obj = _unitOfWork.Category.Get(u => u.CategoryName == id);
            if (obj == null)
            {
                return NotFound();
            }
            _unitOfWork.Category.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Category deleted succesfully";
            return RedirectToAction("Index");
        }
    }
}
