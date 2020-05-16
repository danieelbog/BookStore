using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BookStore.Models.ViewModels;
using DataAccess.Data.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using Utility;

namespace BookStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {         
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");

            // GET THE USER ID, LOL
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                //dont forget to go to login cs and add unitofwork
                //and session to the post method
                //add also to the logout, so it will be cleared when logout happens
                var count = _unitOfWork.ShoppingCart
                    .GetAll(c => c.ApplicationUserId == claim.Value)
                    .ToList().Count();

                HttpContext.Session.SetInt32(StaticDetails.ssShoppingCart, count);
            }



            return View(productList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            var product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id, includeProperties: "Category,CoverType");
            var shoppingCart = new ShoppingCart()
            {
                Product = product,
                ProductId = product.Id
            };



            return View(shoppingCart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            shoppingCart.Id = 0;

            if (ModelState.IsValid)
            {

                // GET THE USER ID, LOL
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                shoppingCart.ApplicationUserId = claim.Value;

                var cartFromDd = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.ApplicationUserId == shoppingCart.ApplicationUserId && u.ProductId == shoppingCart.ProductId
                ,includeProperties:"Product");

                if (cartFromDd == null)
                    _unitOfWork.ShoppingCart.Add(shoppingCart);
                else
                    cartFromDd.Count += shoppingCart.Count;

                _unitOfWork.Save();

                //USE SESSION TO SHOW THE NUMBER OF THE ORDERS IN A CART
                //U CAN USE THIS SESSION EVERYWHERE WITHOUT MAKING IN ALL THE TIME WITH THE GET OBJECT
                //U CAN USE THE ASP CORE SET.INT32 INSTEAD OF SETOBJECT (CUSTOM IMPLEMENTATION).
                var count = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == shoppingCart.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetObject(StaticDetails.ssShoppingCart, count);


                return RedirectToAction(nameof(Index));
            }
            else
            {
                var product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == shoppingCart.Id, includeProperties: "Category,CoverType");

                var carObj = new ShoppingCart()
                {
                    Product = product,
                    ProductId = product.Id
                };

                return View(carObj);

            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
