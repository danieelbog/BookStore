using BookStore.DataAccess.Data;
using DataAccess.Data.Repository.IRepository;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAccess.Data.Repository
{
    public class OrderDetailsRepository : Repository<OrderDetails>, IOrderDetailsRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderDetailsRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(OrderDetails orderDetails)
        {
            var orderDetailsDb = _context.OrderDetails.FirstOrDefault(c => c.Id == orderDetails.Id);

            if (orderDetailsDb != null)
            {
                _context.Update(orderDetailsDb);
            }
        }
    }
}
