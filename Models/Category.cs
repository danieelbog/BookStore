using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models
{
   public class Category
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name="Category Name")]
        public string Name { get; set; }
    }
}
