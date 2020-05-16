using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataAccess.Data.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using Stripe;
using Utility;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderDetailsViewModel OrderDetailsViewModel { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int id)
        {
            OrderDetailsViewModel = new OrderDetailsViewModel()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id, includeProperties: "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetails.GetAll(o => o.OrderHeaderId == id, includeProperties: "Product")
            };

            return View(OrderDetailsViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Details")]
        public IActionResult Details(string stripeToken)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderDetailsViewModel.OrderHeader.Id,
                                                includeProperties: "ApplicationUser");
            if (stripeToken != null)
            {
                //process the payment
                var options = new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 100),
                    Currency = "usd",
                    Description = "Order ID : " + orderHeader.Id,
                    Source = stripeToken
                };

                var service = new ChargeService();
                Charge charge = service.Create(options);

                if (charge.Id == null)
                {
                    orderHeader.PaymentStatus = StaticDetails.PaymentStatusRejected;
                }
                else
                {
                    orderHeader.TransactionId = charge.Id;
                }
                if (charge.Status.ToLower() == "succeeded")
                {
                    orderHeader.PaymentStatus = StaticDetails.PaymentStatusApproved;

                    orderHeader.PaymentDate = DateTime.Now;
                }

                _unitOfWork.Save();

            }
            return RedirectToAction("Details", "Order", new { id = orderHeader.Id });
        }

        [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
        public IActionResult StartProcessing(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            orderHeader.OrderStatus = StaticDetails.StatusInProcess;
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
        public IActionResult ShipOrder()
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderDetailsViewModel.OrderHeader.Id);
            orderHeader.TrackingNumber = OrderDetailsViewModel.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderDetailsViewModel.OrderHeader.Carrier;
            orderHeader.OrderStatus = StaticDetails.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
        public IActionResult CancelOrder(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);

            //STRIPE CODE
            if (orderHeader.PaymentStatus == StaticDetails.StatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 100),
                    Reason = RefundReasons.RequestedByCustomer,
                    Charge = orderHeader.TransactionId

                };
                var service = new RefundService();
                Refund refund = service.Create(options);

                orderHeader.OrderStatus = StaticDetails.StatusRefunded;
                orderHeader.PaymentStatus = StaticDetails.StatusRefunded;
            }
            else
            {
                orderHeader.OrderStatus = StaticDetails.StatusCancelled;
                orderHeader.PaymentStatus = StaticDetails.StatusCancelled;
            }

            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        #region
        [HttpGet]
        public IActionResult GetOrderList(string status)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            IEnumerable<OrderHeader> orderHeaderList;

            if (User.IsInRole(StaticDetails.Role_Admin) || User.IsInRole(StaticDetails.Role_Employee))
            {
                orderHeaderList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                orderHeaderList = _unitOfWork.OrderHeader.GetAll(
                                        u => u.ApplicationUserId == claim.Value,
                                        includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    orderHeaderList = orderHeaderList.Where(o => o.PaymentStatus == StaticDetails.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orderHeaderList = orderHeaderList.Where(o => o.OrderStatus == StaticDetails.StatusApproved ||
                                                            o.OrderStatus == StaticDetails.StatusInProcess ||
                                                            o.OrderStatus == StaticDetails.StatusPending);
                    break;
                case "completed":
                    orderHeaderList = orderHeaderList.Where(o => o.OrderStatus == StaticDetails.StatusShipped);
                    break;
                case "rejected":
                    orderHeaderList = orderHeaderList.Where(o => o.OrderStatus == StaticDetails.StatusCancelled ||
                                                            o.OrderStatus == StaticDetails.StatusRefunded ||
                                                            o.OrderStatus == StaticDetails.PaymentStatusRejected);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaderList });
        }

        #endregion

    }
}