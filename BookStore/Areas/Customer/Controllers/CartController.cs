using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using DataAccess.Data.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Models;
using Models.ViewModels;
using Utility;

namespace BookStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        public ShoppingCartViewModel ShoppingCartViewModel { get; set; }

        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _userManager = userManager;
        }


        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartViewModel = new ShoppingCartViewModel()
            {
                OrderHeader = new OrderHeader(),
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties:"Product")
            };

            ShoppingCartViewModel.OrderHeader.OrderTotal = 0;
            ShoppingCartViewModel.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser
                .GetFirstOrDefault(u => u.Id == claim.Value, includeProperties: "Company");

            foreach (var list in ShoppingCartViewModel.ListCart)
            {
                list.Price = StaticDetails.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                ShoppingCartViewModel.OrderHeader.OrderTotal += (list.Price * list.Count);
                list.Product.Descreption = StaticDetails.ConvertToRawHtml(list.Product.Descreption);
                if(list.Product.Descreption.Length > 100)
                {
                    list.Product.Descreption = list.Product.Descreption.Substring(0, 99) + "...";
                }
            }

            return View();
        }

        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            if (user == null)
                ModelState.AddModelError(string.Empty, "Verification Email is EMpty");


            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = code },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            ModelState.AddModelError(string.Empty, "Verification Email is sent. Please chekc your email.");
            return RedirectToAction(nameof(Index));

        }


        public IActionResult Plus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId, includeProperties: "Product");

            cart.Count += 1;
            cart.Price = StaticDetails.GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId, includeProperties: "Product");

            if (cart.Count == 1)
            {
                var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count();

                _unitOfWork.ShoppingCart.Remove(cart);
                _unitOfWork.Save();

                HttpContext.Session.SetInt32(StaticDetails.ssShopingCart, count - 1);
            }
            else
            {
                cart.Count -= 1;
                cart.Price = StaticDetails.GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                _unitOfWork.Save();
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId, includeProperties: "Product");

                var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count();

                _unitOfWork.ShoppingCart.Remove(cart);
                _unitOfWork.Save();

                HttpContext.Session.SetInt32(StaticDetails.ssShopingCart, count - 1);

            return RedirectToAction(nameof(Index));
        }
    }
}