using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Data.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models;
using Utility;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            var category = new Category();
            if (id == null)
                return View(category);
            else
            {
                category = _unitOfWork.Category.Get(id.GetValueOrDefault());
                if (category == null)
                    return NotFound();
                else
                    return View(category);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.Id == 0)
                {
                    _unitOfWork.Category.Add(category);
                }
                else
                {
                    _unitOfWork.Category.Update(category);
                }

                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(category);
            }
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var categories = _unitOfWork.Category.GetAll();
            return Json(new { data = categories });
        }
        
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var category = _unitOfWork.Category.Get(id);
            if (category == null)
                return Json(new { success = false, message = "Error while deleting" });
            else
            {
                _unitOfWork.Category.Remove(category);
                _unitOfWork.Save();
                return Json(new { success = true, message = "Delete Successful" });
            }
        }

        #endregion
    }
}