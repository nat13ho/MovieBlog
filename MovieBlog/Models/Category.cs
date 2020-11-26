using System;
using System.ComponentModel.DataAnnotations;

namespace MovieBlog.Models
{
    public class Category
    {
        [Required]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Введите название категории!")]
        public string Name { get; set; }
    }
}