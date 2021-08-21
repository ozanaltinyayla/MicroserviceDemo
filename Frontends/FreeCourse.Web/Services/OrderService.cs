using FreeCourse.Shared.Dtos;
using FreeCourse.Shared.Services;
using FreeCourse.Web.Models.FakePaymens;
using FreeCourse.Web.Models.Orders;
using FreeCourse.Web.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace FreeCourse.Web.Services
{
    public class OrderService : IOrderService
    {
        private readonly IPaymentService _paymentService;
        private readonly HttpClient _httpClient;
        private readonly IBasketService _basketService;
        private readonly ISharedIdentityService _sharedIdentityService;

        public OrderService(IPaymentService paymentService, HttpClient httpClient, IBasketService basketService, ISharedIdentityService sharedIdentityService)
        {
            _paymentService = paymentService;
            _httpClient = httpClient;
            _basketService = basketService;
            _sharedIdentityService = sharedIdentityService;
        }

        public async Task<OrderCreatedViewModel> CreateOrder(CheckOutInfoInput checkOutInfoInput)
        {
            var basket = await _basketService.Get();

            var paymentInfoInput = new PaymentInfoInput()
            {
                CardName = checkOutInfoInput.CardName,
                CardNumber = checkOutInfoInput.CardNumber,
                CVV = checkOutInfoInput.CVV,
                Expiration = checkOutInfoInput.Expiration,
                TotalPrice = basket.TotalPrice
            };

            var responsePayment = await _paymentService.ReceivePayment(paymentInfoInput);

            if (!responsePayment)
            {
                return new OrderCreatedViewModel()
                {
                    Error = "Payment Failed",
                    IsSuccessfull = false
                };
            }

            var orderCreateInput = new OrderCreateInput()
            {
                BuyerId = _sharedIdentityService.GetUserId,
                Address = new AddressCreateInput { Province = checkOutInfoInput.Province, District = checkOutInfoInput.District, Line = checkOutInfoInput.Line, Street = checkOutInfoInput.Street, ZipCode = checkOutInfoInput.ZipCode }
            };

            basket.BasketItems.ForEach(x =>
            {
                var orderItem = new OrderItemCreateInput { ProductId = x.CourseId, Price = x.GetCurrentPrice, PictureUrl = "", ProductName = x.CourseName };

                orderCreateInput.OrderItems.Add(orderItem);
            });

            var response = await _httpClient.PostAsJsonAsync<OrderCreateInput>("orders", orderCreateInput);

            if (!response.IsSuccessStatusCode)
            {
                return new OrderCreatedViewModel()
                {
                    Error = "Order could not be created",
                    IsSuccessfull = false
                };
            }

            var orderCreatedViewModel = await response.Content.ReadFromJsonAsync<Response<OrderCreatedViewModel>>();

            orderCreatedViewModel.Data.IsSuccessfull = true;

            await _basketService.Delete();

            return orderCreatedViewModel.Data;
        }

        public async Task<List<OrderViewModel>> GetOrders()
        {
            var response = await _httpClient.GetFromJsonAsync<Response<List<OrderViewModel>>>("orders");

            return response.Data;
        }

        public async Task<OrderSuspendViewModel> SuspendOrder(CheckOutInfoInput checkOutInfoInput)
        {
            var basket = await _basketService.Get();

            var orderCreateInput = new OrderCreateInput()
            {
                BuyerId = _sharedIdentityService.GetUserId,
                Address = new AddressCreateInput { Province = checkOutInfoInput.Province, District = checkOutInfoInput.District, Line = checkOutInfoInput.Line, Street = checkOutInfoInput.Street, ZipCode = checkOutInfoInput.ZipCode }
            };

            basket.BasketItems.ForEach(x =>
            {
                var orderItem = new OrderItemCreateInput { ProductId = x.CourseId, Price = x.GetCurrentPrice, PictureUrl = "", ProductName = x.CourseName };

                orderCreateInput.OrderItems.Add(orderItem);
            });

            var paymentInfoInput = new PaymentInfoInput()
            {
                CardName = checkOutInfoInput.CardName,
                CardNumber = checkOutInfoInput.CardNumber,
                CVV = checkOutInfoInput.CVV,
                Expiration = checkOutInfoInput.Expiration,
                TotalPrice = basket.TotalPrice,
                Order = orderCreateInput
            };

            var responsePayment = await _paymentService.ReceivePayment(paymentInfoInput);

            if (!responsePayment)
            {
                return new OrderSuspendViewModel()
                {
                    Error = "Payment Failed",
                    IsSuccessfull = false
                };
            }

            await _basketService.Delete();

            return new OrderSuspendViewModel() { IsSuccessfull = true };
        }
    }
}
