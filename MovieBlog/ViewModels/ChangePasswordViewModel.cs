using System.ComponentModel.DataAnnotations;

namespace MovieBlog.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают!")]
        public string ConfirmPassword { get; set; }
    }
}