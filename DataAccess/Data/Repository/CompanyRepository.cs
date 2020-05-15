using BookStore.DataAccess.Data;
using DataAccess.Data.Repository.IRepository;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAccess.Data.Repository
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private readonly ApplicationDbContext _context;
        public CompanyRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Company company)
        {
            var objectDb = _context.Companies.FirstOrDefault(c => c.Id == company.Id);

            if (objectDb != null)
            {
                objectDb.Name = company.Name;
                objectDb.StreetAddress = company.StreetAddress;
                objectDb.City = company.City;
                objectDb.State = company.State;
                objectDb.PostalCode = company.PostalCode;
                objectDb.PhoneNumber = company.PhoneNumber;
                objectDb.isAuthorizedCompany = company.isAuthorizedCompany;
            }
        }
    }
}
