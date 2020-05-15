using BookStore.DataAccess.Data;
using DataAccess.Data.Repository.IRepository;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAccess.Data.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderHeaderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(OrderHeader orderHeader)
        {
            var orderHeaderDb = _context.OrderHeaders.FirstOrDefault(c => c.Id == orderHeader.Id);

            if (orderHeaderDb != null)
            {
                _context.Update(orderHeaderDb);
            }
        }
    }
}
