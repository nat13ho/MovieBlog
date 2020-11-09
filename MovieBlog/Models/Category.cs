using System.ComponentModel.DataAnnotations;

namespace MovieBlog.Models
{
    public class Category
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название категории!")]
        public string Name { get; set; }
    }
}