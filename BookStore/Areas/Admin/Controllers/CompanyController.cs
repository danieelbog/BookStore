using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Data.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Utility;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]

    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            var company = new Company();
            if (id == null)
                return View(company);
            else
            {
                company = _unitOfWork.Company.Get(id.GetValueOrDefault());
                if (company == null)
                    return NotFound();
                else
                    return View(company);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    _unitOfWork.Company.Add(company);
                }
                else
                {
                    _unitOfWork.Company.Update(company);
                }

                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(company);
            }
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var categories = _unitOfWork.Company.GetAll();
            return Json(new { data = categories });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var company = _unitOfWork.Company.Get(id);
            if (company == null)
                return Json(new { success = false, message = "Error while deleting" });
            else
            {
                _unitOfWork.Company.Remove(company);
                _unitOfWork.Save();
                return Json(new { success = true, message = "Delete Successful" });
            }
        }

        #endregion
    }
}