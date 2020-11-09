using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovieBlog.Models;

namespace MovieBlog
{
    public class SiteInitializer
    {
        public static async Task InitializeAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ApplicationContext database)
        {
            if (await roleManager.FindByNameAsync("Admin") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            
            if (await roleManager.FindByNameAsync("User") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }
            
            if (!await database.Categories.AnyAsync())
            {
                await database.Categories.AddRangeAsync(
                    new Category { Name = "Биографический"},
                    new Category { Name = "Боевик"},
                    new Category { Name = "Вестерн"},
                    new Category { Name = "Военный"},
                    new Category { Name = "Детектив"},
                    new Category { Name = "Документальный"},
                    new Category { Name = "Драма"},
                    new Category { Name = "Исторический"},
                    new Category { Name = "Кинокомикс"},
                    new Category { Name = "Комедия"},
                    new Category { Name = "Короткометражный"},
                    new Category { Name = "Криминал"},
                    new Category { Name = "Мультфильм"},
                    new Category { Name = "Научный"},
                    new Category { Name = "Семейный"},
                    new Category { Name = "Триллер"},
                    new Category { Name = "Ужасы"},
                    new Category { Name = "Фантастика"},
                    new Category { Name = "Фэнтези"}
                );
                
                await database.SaveChangesAsync();
            }

        }
    }
}