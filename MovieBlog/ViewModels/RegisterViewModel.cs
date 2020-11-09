using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MovieBlog.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите имя пользователя!")]
        public string Username { get; set; }
        
        [Required(ErrorMessage = "Введите Email!")]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Введите пароль!")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Required(ErrorMessage = "Введите повторно пароль!")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают!")]
        public string ConfirmPassword { get; set; }
    }
}