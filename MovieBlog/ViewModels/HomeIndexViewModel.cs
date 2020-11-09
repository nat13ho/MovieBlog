using System.Collections.Generic;
using MovieBlog.Models;

namespace MovieBlog.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<Post> AllPosts { get; set; }
        public List<Post> UserPosts { get; set; }
        
        public List<Category> Categories { get; set; }
    }
}