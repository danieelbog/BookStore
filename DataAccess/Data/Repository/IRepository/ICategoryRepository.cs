using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Data.Repository.IRepository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        void Update(Category product);
    }
}
