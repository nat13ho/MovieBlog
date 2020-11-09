using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using MovieBlog.Models;

namespace MovieBlog.ViewModels
{
    public class EditPostViewModel
    {
        [Required]
        public string Id { get; set; }
        
        [Required(ErrorMessage = "Заголовок не может быть пустым!")]
        public string Title { get; set; }
        
        [Required(ErrorMessage = "Контент не может быть пустым!")]
        public string Content { get; set; }
        
        public IFormFile Image { get; set; }

        public string Category { get; set; }
        public List<Category> AllCategories { get; set; }
    }
}