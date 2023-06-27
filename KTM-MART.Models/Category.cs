using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace KTM_MART.Models
{
    public class Category
    {
     

            [Key]
            public int Id { get; set; }
            [Required]
            [DisplayName("Category Name")]
            public string Name { get; set; }
            public DateTime CreatedDate { get; set; } = DateTime.Now;
        
    }
}
