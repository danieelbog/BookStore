using DataAccess.Data.Repository.IRepository;
using Models;

namespace DataAccess.Data.Repository
{
    public interface IOrderDetailsRepository : IRepository<OrderDetails>
    {
        void Update(OrderDetails orderDetails);
    }
}