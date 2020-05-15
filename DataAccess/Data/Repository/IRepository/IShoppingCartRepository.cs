using DataAccess.Data.Repository.IRepository;
using Models;

namespace DataAccess.Data.Repository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        void Update(ShoppingCart shoppingCart);
    }
}