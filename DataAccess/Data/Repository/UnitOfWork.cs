using BookStore.DataAccess.Data;
using DataAccess.Data.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Data.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;       

        public ICategoryRepository Category { get; private set; }
        public ICoverTypeRepository CoverType { get; private set; }
        public ISP_Call SP_Call { get; private set; }
        public IProductRepository Product { get; private set; }
        public ICompanyRepository Company { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IOrderHeaderRepository OrderHeader { get; set; }
        public IOrderDetailsRepository OrderDetails { get; set; }
        public IShoppingCartRepository ShoppingCart { get; set; }


        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Category = new CategoryRepository(_context);
            CoverType = new CoverTypeRepository(_context);
            SP_Call = new Sp_Call(_context);
            Product = new ProductRepositroy(_context);
            Company = new CompanyRepository(_context);
            ApplicationUser = new ApplicationUserRepository(_context);
            OrderDetails = new OrderDetailsRepository(_context);
            OrderHeader = new OrderHeaderRepository(_context);
            ShoppingCart = new ShoppingCartRepository(_context);
        }
        public void Dispose()
        {
            _context.Dispose();
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
