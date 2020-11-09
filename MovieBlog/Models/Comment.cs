using System;
using System.ComponentModel.DataAnnotations;

namespace MovieBlog.Models
{
    public class Comment
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Комментарий не может быть пустым!")]
        public string Content { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string PostId { get; set; }
        public Post Post { get; set; }
        
        public string UserId { get; set; }
        public User User { get; set; }
    }
}