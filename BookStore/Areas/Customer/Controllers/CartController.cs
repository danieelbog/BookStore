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
using Stripe;
using Utility;

namespace BookStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        [BindProperty]
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
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product")
            };
            ShoppingCartViewModel.OrderHeader.OrderTotal = 0;
            ShoppingCartViewModel.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser
                                                        .GetFirstOrDefault(u => u.Id == claim.Value,
                                                        includeProperties: "Company");

            foreach (var list in ShoppingCartViewModel.ListCart)
            {
                list.Price = StaticDetails.GetPriceBasedOnQuantity(list.Count, list.Product.Price,
                                                    list.Product.Price50, list.Product.Price100);
                ShoppingCartViewModel.OrderHeader.OrderTotal += (list.Price * list.Count);
                list.Product.Descreption = StaticDetails.ConvertToRawHtml(list.Product.Descreption);
                if (list.Product.Descreption.Length > 100)
                {
                    list.Product.Descreption = list.Product.Descreption.Substring(0, 99) + "...";
                }
            }


            return View(ShoppingCartViewModel);
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

                HttpContext.Session.SetInt32(StaticDetails.ssShoppingCart, count - 1);
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

                HttpContext.Session.SetInt32(StaticDetails.ssShoppingCart, count - 1);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartViewModel = new ShoppingCartViewModel()
            {
                OrderHeader = new OrderHeader(),
                ListCart = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == claim.Value, includeProperties: "Product")
            };           

            ShoppingCartViewModel.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(c => c.Id == claim.Value, includeProperties: "Company");

            foreach (var list in ShoppingCartViewModel.ListCart)
            {
                list.Price = StaticDetails.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                ShoppingCartViewModel.OrderHeader.OrderTotal += (list.Price * list.Count);
            }

            ShoppingCartViewModel.OrderHeader.Name = ShoppingCartViewModel.OrderHeader.ApplicationUser.Name;
            ShoppingCartViewModel.OrderHeader.PhoneNumber = ShoppingCartViewModel.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartViewModel.OrderHeader.StreetAddress = ShoppingCartViewModel.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartViewModel.OrderHeader.City = ShoppingCartViewModel.OrderHeader.ApplicationUser.City;
            ShoppingCartViewModel.OrderHeader.State = ShoppingCartViewModel.OrderHeader.ApplicationUser.State;
            ShoppingCartViewModel.OrderHeader.PostalCode = ShoppingCartViewModel.OrderHeader.ApplicationUser.PostalCode;

            return View(ShoppingCartViewModel);

        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPost(string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartViewModel.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(c => c.Id == claim.Value, includeProperties:"Company");

            ShoppingCartViewModel.ListCart = _unitOfWork.ShoppingCart
                                        .GetAll(c => c.ApplicationUserId == claim.Value,
                                        includeProperties: "Product");

            ShoppingCartViewModel.OrderHeader.PaymentStatus = StaticDetails.PaymentStatusPending;
            ShoppingCartViewModel.OrderHeader.OrderStatus = StaticDetails.PaymentStatusPending;
            ShoppingCartViewModel.OrderHeader.ApplicationUserId = claim.Value;
            ShoppingCartViewModel.OrderHeader.OrderDate = DateTime.Now;

            _unitOfWork.OrderHeader.Add(ShoppingCartViewModel.OrderHeader);
            _unitOfWork.Save();

            List<OrderDetails> orderDetailsList = new List<OrderDetails>();
            foreach (var item in ShoppingCartViewModel.ListCart)
            {
                item.Price = StaticDetails.GetPriceBasedOnQuantity(item.Count, item.Product.Price,
                    item.Product.Price50, item.Product.Price100);

                OrderDetails orderDetails = new OrderDetails()
                {
                    ProductId = item.ProductId,
                    OrderHeaderId = ShoppingCartViewModel.OrderHeader.Id,
                    Price = item.Price,
                    Count = item.Count
                };
                ShoppingCartViewModel.OrderHeader.OrderTotal += orderDetails.Count * orderDetails.Price;
                _unitOfWork.OrderDetails.Add(orderDetails);
            }

            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartViewModel.ListCart);
            _unitOfWork.Save();
            HttpContext.Session.SetInt32(StaticDetails.ssShoppingCart, 0);

            if (stripeToken == null)
            {
                //order will be created for delayed payment for authroized company
                ShoppingCartViewModel.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
                ShoppingCartViewModel.OrderHeader.PaymentStatus = StaticDetails.PaymentStatusDelayedPayment;
                ShoppingCartViewModel.OrderHeader.OrderStatus = StaticDetails.PaymentStatusApproved;
            }
            else
            {
                //process the payment
                var options = new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(ShoppingCartViewModel.OrderHeader.OrderTotal * 100),
                    Currency = "usd",
                    Description = "Order ID : " + ShoppingCartViewModel.OrderHeader.Id,
                    Source = stripeToken
                };

                var service = new ChargeService();
                Charge charge = service.Create(options);

                if (charge.BalanceTransactionId == null)
                {
                    ShoppingCartViewModel.OrderHeader.PaymentStatus = StaticDetails.PaymentStatusRejected;
                }
                else
                {
                    ShoppingCartViewModel.OrderHeader.TransactionId = charge.BalanceTransactionId;
                }
                if (charge.Status.ToLower() == "succeeded")
                {
                    ShoppingCartViewModel.OrderHeader.PaymentStatus = StaticDetails.PaymentStatusApproved;
                    ShoppingCartViewModel.OrderHeader.OrderStatus = StaticDetails.PaymentStatusApproved;
                    ShoppingCartViewModel.OrderHeader.PaymentDate = DateTime.Now;
                }
            }

            _unitOfWork.Save();

            return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartViewModel.OrderHeader.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }

    }

}