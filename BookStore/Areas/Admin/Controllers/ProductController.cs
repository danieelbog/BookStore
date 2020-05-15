using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Data.Repository.IRepository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models;
using Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Utility;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]

    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            var viewModel = new ProductViewModel()
            {
                Product = new Product(),
                CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                CoverTypeList = _unitOfWork.CoverType.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };

            if (id == null)
                return View(viewModel);
            else
            {
                viewModel.Product = _unitOfWork.Product.Get(id.GetValueOrDefault());
                if (viewModel.Product == null)
                    return NotFound();
                else
                    return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                string webRootPath = _hostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"images\products");
                    var extension = Path.GetExtension(files[0].FileName);

                    if (viewModel.Product.ImageUrl != null)
                    {
                        var imagePath = Path.Combine(webRootPath, viewModel.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                    using (var filesStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(filesStreams);
                    }
                    viewModel.Product.ImageUrl = @"\images\products\" + fileName + extension;
                }
                else
                {
                    //update when no change image occured
                    if (viewModel.Product.Id != 0)
                    {
                        var productDB = _unitOfWork.Product.Get(viewModel.Product.Id);
                        viewModel.Product.ImageUrl = productDB.ImageUrl;
                    }
                }

                if (viewModel.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(viewModel.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(viewModel.Product);
                }

                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            else
            {

                viewModel.CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });

                viewModel.CoverTypeList = _unitOfWork.CoverType.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });

                if(viewModel.Product.Id != 0)
                {
                    viewModel.Product = _unitOfWork.Product.Get(viewModel.Product.Id);
                }

                return View(viewModel.Product);
            }
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var categories = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
            return Json(new { data = categories });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var product = _unitOfWork.Product.Get(id);
            if (product == null)
                return Json(new { success = false, message = "Error while deleting" });
            else
            {
                string webRootPath = _hostEnvironment.WebRootPath;
                var imagePath = Path.Combine(webRootPath, product.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                _unitOfWork.Product.Remove(product);
                _unitOfWork.Save();
                return Json(new { success = true, message = "Delete Successful" });
            }
        }

        #endregion
    }
}