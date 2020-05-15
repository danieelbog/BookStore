using DataAccess.Data.Repository.IRepository;
using Models;

namespace DataAccess.Data.Repository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader orderHeader);
    }
}