using FreeCourse.Web.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreeCourse.Web.Services.Interface
{
    public interface IOrderService
    {
        Task<OrderCreatedViewModel> CreateOrder(CheckOutInfoInput checkOutInfoInput);
        Task SuspendOrder(CheckOutInfoInput checkOutInfoInput);
        Task<List<OrderViewModel>> GetOrders();
    }
}
