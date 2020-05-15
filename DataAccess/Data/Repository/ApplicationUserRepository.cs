using BookStore.DataAccess.Data;
using DataAccess.Data.Repository.IRepository;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAccess.Data.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private readonly ApplicationDbContext _context;
        public ApplicationUserRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
