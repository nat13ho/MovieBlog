using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace MovieBlog.Models
{
    public class User : IdentityUser
    {
        public int ImageId { get; set; }
        public Image Image { get; set; }
        
        public List<UserFavPost> UserFavPosts { get; set; } = new List<UserFavPost>();
    }
}