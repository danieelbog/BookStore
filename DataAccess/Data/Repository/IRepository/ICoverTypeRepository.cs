using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Data.Repository.IRepository
{
    public interface ICoverTypeRepository : IRepository<CoverType>
    {
        void Update(CoverType coverType);
    }
}
