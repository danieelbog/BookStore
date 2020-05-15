using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Data.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product product);
    }
}
