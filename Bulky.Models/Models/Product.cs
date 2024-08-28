using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Bulky.Models.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string ISBN { get; set; }
        [Required]
        public string Author { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        [Display(Name = "List Price")]
        public Double ListPrice { get; set; }

        [Required]
        [Display(Name = "Price for 1-50")]
        public Double Price { get; set; }

        [Required]
        [Display(Name = "Price for 50+")]
        public Double Price50 { get; set; }

        [Required]
        [Display(Name = "Price for 100+")]
        public Double Price100 { get; set; }

        public int CategoryId { get; set; }
        [ForeignKey(nameof(CategoryId))]
        [ValidateNever]
        public Category Category { get; set; }

        public string ImageURL { get; set; }
    }
}
