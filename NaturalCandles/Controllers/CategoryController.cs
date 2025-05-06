using Microsoft.AspNetCore.Mvc;
using NaturalCandles.DataAccess.Data;
using NaturalCandles.Models;

namespace NaturalCandles.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CategoryController(ApplicationDbContext db)
        {
            _db= db;
        }
        public IActionResult Index()
        {
            List<Category> objCategoryList = _db.Categories.ToList();
            return View(objCategoryList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
		public IActionResult Create(Category obj)
		{
            if (obj.Name.ToLower() == obj.CategoryType.ToString())
            {
                ModelState.AddModelError("name", "The Product cannot exactly match the Name");
            }

            if (ModelState.IsValid)
            {
                _db.Categories.Add(obj);
                _db.SaveChanges();
				TempData["success"] = "Category created succesfully";
				return RedirectToAction("Index");
			}
            return View();
		}
		public IActionResult Edit(int? id)
		{
            if (id == null || id == 0)
            {
                return NotFound();
            }
			Category? categoryFromdb = _db.Categories.Find(id);
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
				_db.Categories.Update(obj);
				_db.SaveChanges();
				TempData["success"] = "Category updated succesfully";
				return RedirectToAction("Index");
			}
			return View();
		}
		public IActionResult Delete(int? id)
		{
			if (id == null || id == 0)
			{
				return NotFound();
			}
			Category? categoryFromdb = _db.Categories.Find(id);
			if (categoryFromdb == null)
			{
				return NotFound();
			}
			return View(categoryFromdb);
		}
		[HttpPost, ActionName("Delete")]
		public IActionResult DeletePOST(int? id)
		{
			Category? obj = _db.Categories.Find(id);
			if (obj == null)
			{
				return NotFound();
			}
			_db.Categories.Remove(obj);
			_db.SaveChanges();
			TempData["success"] = "Category deleted succesfully";
			return RedirectToAction("Index");
		}
	}
}
