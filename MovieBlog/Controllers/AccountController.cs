using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBlog.Models;
using MovieBlog.ViewModels;

namespace MovieBlog.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationContext _database;
        private readonly IWebHostEnvironment _environment;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager,
            ApplicationContext context, IWebHostEnvironment environment, RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
            _signInManager = signInManager;
            _userManager = userManager;
            _environment = environment;
            _database = context;
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _database.Users
                .Include(u => u.Image)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            var categories = await _database.Categories.OrderBy(c => c.Name).ToListAsync();

            if (user != null)
            {
                return View(new ProfileViewModel() {User = user, Categories = categories});
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() => View();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var defaultImage = await _database.Images.FirstOrDefaultAsync(i => i.Name == "MovieBlogUserImg");

            if (ModelState.IsValid && defaultImage != null)
            {
                var user = new User {Email = model.Email, UserName = model.Username, Image = defaultImage};
                var userRole = await _roleManager.FindByNameAsync("User");
                var result = await _userManager.CreateAsync(user, model.Password);
                await _userManager.AddToRoleAsync(user, userRole.Name);

                if (result.Succeeded)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action(
                        "ConfirmEmail",
                        "Account",
                        new {userId = user.Id, code},
                        protocol: HttpContext.Request.Scheme);
                    EmailService emailService = new EmailService();

                    await emailService.SendEmailAsync(model.Email, "Confirm your account",
                        $"Подтвердите регистрацию, перейдя по ссылке: <a href='{callbackUrl}'>link</a>");

                    return View("RegisterCofirmation",
                        "Для завершения регистрации проверьте электронную почту и перейдите по ссылке, указанной в письме");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId != null && code != null)
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user != null)
                {
                    var result = await _userManager.ConfirmEmailAsync(user, code);

                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, false);
                        return RedirectToAction(nameof(Profile));
                    }
                }
                else
                {
                    return View("Error");
                }
            }
            else
            {
                return View("Error");
            }

            return View("Error");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl) => View(new LoginViewModel {ReturnUrl = returnUrl});

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result =
                    await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction(nameof(Profile));
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Неправильный логин и (или) пароль");
                }
            }
            else
            {
                return View(model);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var model = new ChangePasswordViewModel {Id = user.Id};

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);

                if (user != null)
                {
                    var result = await _userManager.ChangePasswordAsync(user, model.Password, model.NewPassword);

                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(Profile));
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Пользователь не найден");
                }
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                {
                    return View("ForgotPasswordConfirmation");
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callBackUrl = Url.Action(
                    "ResetPassword",
                    "Account",
                    new {userId = user.Id, code},
                    protocol: HttpContext.Request.Scheme
                );

                var emailService = new EmailService();
                await emailService.SendEmailAsync(model.Email, "Reset Password",
                    $"Для сброса пароля пройдите по ссылке: <a href='{callBackUrl}'>link</a>");

                return View("ForgotPasswordConfirmation");
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, false);
                        return RedirectToAction(nameof(Profile));
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    return View("ResetPasswordConfirmation");
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Favorites()
        {
            var user = await _database.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user != null)
            {
                var userFavPosts = _database.UserFavPosts
                    .Include(p => p.Post.Image)
                    .Include(p => p.Post.Category)
                    .Include(p => p.Post.Comments)
                    .Where(p => p.UserId == user.Id).Select(p => p.Post).ToList();

                var categories = await _database.Categories.OrderBy(c => c.Name).ToListAsync();

                return View(new FavoritesViewModel() {FavPosts = userFavPosts, Categories = categories});
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (id != null)
            {
                var user = await _database.Users
                    .Include(u => u.Image)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user != null)
                {
                    var model = new EditAccountViewModel()
                    {
                        Username = user.UserName,
                        UserImage = user.Image,
                        UserId = user.Id
                    };

                    return View(model);
                }
            }

            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditAccountViewModel model)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == model.UserId);
            var defaultUserImage = await _database.Images.FirstOrDefaultAsync(i => i.Name == "MovieBlogUserImg");

            if (user != null)
            {
                if (model.NewUserImage != null)
                {
                    var fileName = Path.GetFileNameWithoutExtension(model.NewUserImage.FileName);
                    var extension = Path.GetExtension(model.NewUserImage.FileName);
                    var imageModel = new Image()
                    {
                        Name = fileName,
                        Extension = extension
                    };

                    await using (var dataStream = new MemoryStream())
                    {
                        await model.NewUserImage.CopyToAsync(dataStream);
                        imageModel.Data = dataStream.ToArray();
                    }

                    if (user.Image != defaultUserImage)
                    {
                        _database.Images.Remove(user.Image);
                    }
                    
                    user.Image = imageModel;
                    await _database.Images.AddAsync(imageModel);
                    await _database.SaveChangesAsync();
                }

                user.UserName = model.Username;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction(nameof(Profile));
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                return NotFound();
            }
        }
    }
}