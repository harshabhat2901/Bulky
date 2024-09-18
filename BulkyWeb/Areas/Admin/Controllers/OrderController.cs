using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Bulky.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _uow;
        [BindProperty]
        public OrderVM orderVM { get; set; }

        public OrderController(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public IActionResult Index()
        {

            return View();
        }

        public IActionResult Details(int orderId)
        {
            orderVM = new OrderVM
            {
                OrderHeader = _uow.OrderHeader.Get(u => u.Id == orderId, includeproperties: "ApplicationUser"),
                OrderDetail = _uow.OrderDetail.GetAll(u => u.OrderHeaderID == orderId, includeProperties: "Product")
            };
            return View(orderVM);
        }

        [HttpPost]
        [Authorize(Roles = SD.ROLE_ADMIN + "," + SD.ROLE_EMPLOYEE)]
        public IActionResult UpdateOrderDetails()
        {
            OrderHeader orderFromDB = _uow.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);
            orderFromDB.Name = orderVM.OrderHeader.Name;
            orderFromDB.PhoneNumber = orderVM.OrderHeader.PhoneNumber;
            orderFromDB.StreetAddress = orderVM.OrderHeader.StreetAddress;
            orderFromDB.City = orderVM.OrderHeader.City;
            orderFromDB.State = orderVM.OrderHeader.State;
            orderFromDB.PostalCode = orderVM.OrderHeader.PostalCode;

            if (!string.IsNullOrEmpty(orderVM.OrderHeader.Carrier))
                orderFromDB.Carrier = orderVM.OrderHeader.Carrier;

            if (!string.IsNullOrEmpty(orderVM.OrderHeader.TrackingNumber))
                orderFromDB.Carrier = orderVM.OrderHeader.TrackingNumber;

            _uow.OrderHeader.Update(orderFromDB);
            _uow.Save();
            TempData["Success"] = "Order Details updated successfully.";
            return RedirectToAction(nameof(Details), new { orderId = orderFromDB.Id });
        }


        [HttpPost]
        [Authorize(Roles = SD.ROLE_ADMIN + "," + SD.ROLE_EMPLOYEE)]
        public IActionResult StartProcessing()
        {
            _uow.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, SD.StatusInProcess);
            _uow.Save();
            TempData["Success"] = "Order Status updated Successfully";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.ROLE_ADMIN + "," + SD.ROLE_EMPLOYEE)]
        public IActionResult ShipOrder()
        {
            OrderHeader orderFromDB = _uow.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);
            orderFromDB.ShippingDate = DateTime.Today;
            orderFromDB.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            orderFromDB.Carrier = orderVM.OrderHeader.Carrier;
            orderFromDB.OrderStatus = SD.StatusShipped;
            if (orderFromDB.PaymentStatus == SD.PaymentStatusDelayedPayment)
                orderFromDB.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));

            _uow.OrderHeader.Update(orderFromDB);
            _uow.Save();
            TempData["Success"] = "Order shipped Successfully";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.ROLE_ADMIN + "," + SD.ROLE_EMPLOYEE)]
        public IActionResult CancelOrder()
        {
            OrderHeader orderFromDB = _uow.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);

            if(orderFromDB.PaymentStatus == SD.PaymentStatusApproved)
            {
                //This section is for approved (normal payment completion)
            }
            else 
            {
                // this section is for company payment which isnot payed yet
                _uow.OrderHeader.UpdateStatus(orderFromDB.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _uow.Save();
            TempData["Success"] = "Order cancelled Successfully";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
        }

        [ActionName("Details")]
        [HttpPost]       
        public IActionResult Details_Pay_Now()
        {
            orderVM.OrderHeader = _uow.OrderHeader.Get(u=> u.Id == orderVM.OrderHeader.Id);
            orderVM.OrderDetail = _uow.OrderDetail.GetAll(u => u.Id == orderVM.OrderHeader.Id);

            var domain = "https://localhost:7081/";
            var successURL = domain + $"admin/order/PaymentConfirmation?id={orderVM.OrderHeader.Id}";
            _uow.OrderHeader.UpdateStripePaymentID(orderVM.OrderHeader.Id, "SessionID123", null);
            _uow.Save();
            return Redirect(successURL);
        }

        public IActionResult PaymentConfirmation(int id)
        {
            //Once we get the session we need to understand whether the payment is made
            OrderHeader orderHeader = _uow.OrderHeader.Get(u => u.Id == id, includeproperties: "ApplicationUser");

            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                //We need to write logic to get the payment status. If the status is paid then run below line
                _uow.OrderHeader.UpdateStripePaymentID(id, "SessionID123", "PaymentIntentId123");
                _uow.OrderHeader.UpdateStatus(id, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                _uow.Save();
            }
         

            return View(id);
        }

        #region APICall
        [HttpGet]

        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;

            if (User.IsInRole(SD.ROLE_ADMIN) || User.IsInRole(SD.ROLE_EMPLOYEE))
                orderHeaders = _uow.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                orderHeaders = _uow.OrderHeader.GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser").ToList();
            }



            switch (status)
            {
                case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "pending":
                    orderHeaders = orderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaders });
        }

        #endregion
    }
}
