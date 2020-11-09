using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Operations;
using MovieBlog.Models;

namespace MovieBlog.ViewModels
{
    public class RoleIndexViewModel
    {
        public List<IdentityRole> Roles { get; set; }
        public List<Category> Categories { get; set; }
    }
}