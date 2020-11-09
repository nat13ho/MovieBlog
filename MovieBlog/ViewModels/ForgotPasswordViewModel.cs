using System.ComponentModel.DataAnnotations;

namespace MovieBlog.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}