using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using KTM_MART.Models.ViewModels;
using NHibernate.Mapping;


namespace KTM_MART.Models
{
	public  class Product
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

		[Key]
        public int Id { get; set; }
        [DisplayName("Product Name")]
        [Required]
        public string ProductName { get; set; }
        [Required]
        public string ProductCode { get; set; }
        [Required]
        public string RewardPoints { get; set; }
        [DisplayName("Product Description")]
        [DataType(DataType.Text)]
        public string ProductDescription { get; set; }
        [ValidateNever]   
        public string ImageUrl { get; set; }
        [DisplayName("Product Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public float Price { get; set; }
        public  float ListPrice { get; set; }
        public float Price50 { get; set; }
        public float Price100 { get; set; }
        [Required]
        [DisplayName("Category")]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }

	}
}
