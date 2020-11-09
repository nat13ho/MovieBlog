using System.Collections.Generic;
using MovieBlog.Models;

namespace MovieBlog.ViewModels
{
    public class ProfileViewModel
    {
        public User User { get; set; }
        public List<Category> Categories { get; set; }
    }
}