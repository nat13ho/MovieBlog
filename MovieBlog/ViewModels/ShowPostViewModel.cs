using System.Collections.Generic;
using MovieBlog.Models;

namespace MovieBlog.ViewModels
{
    public class ShowPostViewModel
    {
        public Post Post { get; set; }
        public Comment Comment { get; set; }

        public List<Comment> PostComments { get; set; }
        
        public List<Category> Categories { get; set; }

        public User User { get; set; }
    }
}