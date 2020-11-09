using Microsoft.AspNetCore.Http;
using MovieBlog.Models;

namespace MovieBlog.ViewModels
{
    public class EditAccountViewModel
    {
        public string Username { get; set; }
        public Image UserImage { get; set; }
        public IFormFile NewUserImage { get; set; }

        public string UserId { get; set; }
    }
}