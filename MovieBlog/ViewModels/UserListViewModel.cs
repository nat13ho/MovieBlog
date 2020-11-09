using System.Collections.Generic;
using MovieBlog.Models;

namespace MovieBlog.ViewModels
{
    public class UserListViewModel
    {
        public List<User> Users { get; set; }
        public List<Category> Categories { get; set; }
    }
}