using BookStore.DataAccess.Data;
using DataAccess.Data.Repository.IRepository;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAccess.Data.Repository
{
    public class ProductRepositroy : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepositroy(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Product product)
        {
            var productDB = _context.Products.FirstOrDefault(c => c.Id == product.Id);

            if (productDB != null)
            {
                if(product.ImageUrl != null)
                {
                    productDB.ImageUrl = product.ImageUrl;
                }

                productDB.ISBN = product.ISBN;
                productDB.Price = product.Price;
                productDB.Price50 = product.Price50;
                productDB.Price100 = product.Price100;
                productDB.ListPrice = product.ListPrice;
                productDB.Title = product.Title;
                productDB.Descreption = product.Descreption;
                productDB.CategoryId = product.CategoryId;
                productDB.Author = product.Author;
                productDB.CoverTypeId = product.CoverTypeId;
            }
        }
    }
}
