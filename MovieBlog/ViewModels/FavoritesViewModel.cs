using System.Collections.Generic;
using MovieBlog.Models;

namespace MovieBlog.ViewModels
{
    public class FavoritesViewModel
    {
        public List<Post> FavPosts { get; set; }
        public List<Category> Categories { get; set; }
    }
}