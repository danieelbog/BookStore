using BookStore.DataAccess.Data;
using DataAccess.Data.Repository.IRepository;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAccess.Data.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly ApplicationDbContext _context;
        public ShoppingCartRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(ShoppingCart shoppingCart)
        {
            var shoppingCartDb = _context.ShoppingCarts.FirstOrDefault(c => c.Id == shoppingCart.Id);

            if (shoppingCartDb != null)
            {
                _context.Update(shoppingCartDb);
            }
        }
    }
}
