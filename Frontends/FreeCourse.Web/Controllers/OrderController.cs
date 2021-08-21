using FreeCourse.Web.Models.Orders;
using FreeCourse.Web.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreeCourse.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IBasketService _basketService;
        private readonly IOrderService _orderService;

        public OrderController(IBasketService basketService, IOrderService orderService)
        {
            _basketService = basketService;
            _orderService = orderService;
        }

        public async Task<IActionResult> CheckOut()
        {
            var basket = await _basketService.Get();

            ViewBag.basket = basket;

            return View(new CheckOutInfoInput());
        }

        [HttpPost]
        public async Task<IActionResult> CheckOut(CheckOutInfoInput checkOutInfoInput)
        {
            //Synchronous Connection
            //var orderStatus = await _orderService.CreateOrder(checkOutInfoInput);

            //Asynchronous Connection
            var orderSuspend = await _orderService.SuspendOrder(checkOutInfoInput);

            if (!orderSuspend.IsSuccessfull)
            {
                var basket = await _basketService.Get();

                ViewBag.basket = basket;

                ViewBag.error = orderSuspend.Error;

                return View();
            }

            //Synchronous Connection
            //return RedirectToAction(nameof(SuccessfulCheckOut), new { orderId = orderStatus.OrderId });

            //Asynchronous Connection
            return RedirectToAction(nameof(SuccessfulCheckOut), new { orderId = new Random().Next(1, 1000) });
        }

        public IActionResult SuccessfulCheckOut(int orderId)
        {
            ViewBag.orderId = orderId;

            return View();
        }

        public async Task<IActionResult> CheckOutHistory()
        {
            return View(await _orderService.GetOrders());
        }
    }
}
