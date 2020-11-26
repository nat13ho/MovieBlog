using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovieBlog.Models;

namespace MovieBlog
{
    public class SiteInitializer
    {
        public static async Task InitializeAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, 
            ApplicationContext database, IWebHostEnvironment environment)
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

            if (!await database.Images.AnyAsync())
            {
                var defaultUserImage = new Image()
                {
                    Name = "MovieBlogUserImg",
                    Extension = ".png",
                };

                await using (var fileStream = File.OpenRead($"{environment.WebRootPath}/img/MovieBlogUserImg.png"))
                {
                    var data = new byte[fileStream.Length];
                    fileStream.Read(data, 0, data.Length);
                    defaultUserImage.Data = data;
                }
                
                var defaultPostImage = new Image()
                {
                    Name = "MovieBlogPostImg",
                    Extension = ".jpg",
                };

                await using (var fileStream = File.OpenRead($"{environment.WebRootPath}/img/MovieBlogPostImg.jpg"))
                {
                    var data = new byte[fileStream.Length];
                    fileStream.Read(data, 0, data.Length);
                    defaultPostImage.Data = data;
                }

                await database.Images.AddRangeAsync(defaultUserImage, defaultPostImage);
                await database.SaveChangesAsync();
            }

            if (!await database.Users.AnyAsync())
            {
                var adminImage = await database.Images.FirstOrDefaultAsync(i => i.Name == "MovieBlogUserImg");

                if (adminImage != null)
                {
                    var admin = new User {Email = "nat13ho@gmail.com", UserName = "nat13ho", Image = adminImage};
                    var adminRole = await roleManager.FindByNameAsync("Admin");
                    var userRole = await roleManager.FindByNameAsync("User");
                    var result = await userManager.CreateAsync(admin, "password");
                    await userManager.AddToRoleAsync(admin, userRole.Name);
                    await userManager.AddToRoleAsync(admin, adminRole.Name);

                    if (result.Succeeded)
                    {
                        var code = await userManager.GenerateEmailConfirmationTokenAsync(admin);
                        await userManager.ConfirmEmailAsync(admin, code);
                    }
                }
            }
        }
    }
}