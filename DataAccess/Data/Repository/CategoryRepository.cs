using BookStore.DataAccess.Data;
using DataAccess.Data.Repository.IRepository;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAccess.Data.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Category category)
        {
            var categoryDB = _context.Categories.FirstOrDefault(c => c.Id == category.Id);

            if (categoryDB != null)
            {
                categoryDB.Name = category.Name;
            }
        }
    }
}
