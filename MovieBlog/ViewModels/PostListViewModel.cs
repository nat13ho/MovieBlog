using System.Collections.Generic;
using MovieBlog.Models;

namespace MovieBlog.ViewModels
{
    public class PostListViewModel
    {
        public List<Post> Posts { get; set; }
        public List<Category> Categories { get; set; }
        
    }
}