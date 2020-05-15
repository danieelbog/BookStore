using BookStore.DataAccess.Data;
using DataAccess.Data.Repository.IRepository;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAccess.Data.Repository
{
    public class CoverTypeRepository : Repository<CoverType>, ICoverTypeRepository
    {
        private readonly ApplicationDbContext _context;
        public CoverTypeRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(CoverType coverType)
        {
            var coverTypeDB = _context.CoverTypes.FirstOrDefault(c => c.Id == coverType.Id);

            if (coverTypeDB != null)
            {
                coverTypeDB.Name = coverTypeDB.Name;
            }
        }
    }
}
