using System;
using System.Collections.Generic;
using System.Text;

namespace Models.ViewModels
{
    public class CatergoryViewModel
    {
        public IEnumerable<Category> Categories { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
