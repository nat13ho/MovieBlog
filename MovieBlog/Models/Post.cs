using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace MovieBlog.Models
{
    public class Post
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required(ErrorMessage = "Укажите заголовок!")]
        public string Title { get; set; }
        
        [Required(ErrorMessage = "Новость не может быть пустой!")]
        public string Content { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime Created { get; set; } = DateTime.Now;

        public string CategoryId { get; set; }
        public Category Category { get; set; }
        
        public string ImageId { get; set; }
        public Image Image { get; set; }

        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<UserFavPost> UserFavPosts { get; set; } = new List<UserFavPost>();
    }
}