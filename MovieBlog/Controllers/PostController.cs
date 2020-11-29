using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageMagick;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBlog.Models;
using MovieBlog.ViewModels;
using TinyPng;

namespace MovieBlog.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly ApplicationContext _database;
        private readonly UserManager<User> _userManager;

        public PostController(ApplicationContext context, UserManager<User> userManager)
        {
            _database = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View(new CreatePostViewModel() {AllCategories = await _database.Categories.OrderBy(c => c.Name).ToListAsync()});
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CreatePostViewModel model, string category)
        {
            var defaultPostImage = await _database.Images.FirstOrDefaultAsync(i => i.Name == "MovieBlogPostImg");
            
            if (ModelState.IsValid)
            {
                var postCategory = await _database.Categories.FirstOrDefaultAsync(c => c.Name == category);

                if (postCategory != null)
                {
                    Post post;
                    
                    if (model.Image != null)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(model.Image.FileName);
                        var extension = Path.GetExtension(model.Image.FileName);
                        var imageModel = new Image()
                        {
                            Name = fileName,
                            Extension = extension,
                            Data = await Image.GetCompressedData(model.Image)
                        };
                        
                        await _database.Images.AddAsync(imageModel);
                        await _database.SaveChangesAsync();
                            
                        post = new Post()
                            {Title = model.Title, Content = model.Content, Category = postCategory, Image = imageModel};
                    }
                    else
                    {
                        post = new Post()
                            {Title = model.Title, Content = model.Content, Category = postCategory, Image = defaultPostImage};
                    }

                    await _database.Posts.AddAsync(post);
                    await _database.SaveChangesAsync();

                    return RedirectToAction(nameof(GetPostList));
                }
            }

            return View(new CreatePostViewModel() {AllCategories = await _database.Categories.OrderBy(c => c.Name).ToListAsync()});
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var post = await _database.Posts
                    .Include(p => p.Category)
                    .Include(p => p.Image)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (post != null)
                {
                    var model = new EditPostViewModel
                    {
                        Id = id,
                        Title = post.Title,
                        Content = post.Content,
                        Category = post.Category.Name,
                        AllCategories = await _database.Categories.OrderBy(c => c.Name).ToListAsync()
                    };

                    return View(model);
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(EditPostViewModel model, string category)
        {
            if (ModelState.IsValid)
            {
                var post = await _database.Posts
                    .Include(p => p.Category)
                    .Include(p => p.Image)
                    .FirstOrDefaultAsync(p => p.Id == model.Id);
                var postCategory = await _database.Categories.FirstOrDefaultAsync(c => c.Name == category);
                var defaultPostImage = await _database.Images.FirstOrDefaultAsync(i => i.Name == "MovieBlogPostImg");

                if (post != null && postCategory != null)
                {
                    if (model.Image != null)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(model.Image.FileName);
                        var extension = Path.GetExtension(model.Image.FileName);
                        var imageModel = new Image()
                        {
                            Name = fileName,
                            Extension = extension,
                            Data = await Image.GetCompressedData(model.Image)
                        };
                        
                        if (post.Image != defaultPostImage)
                        {
                            _database.Images.Remove(post.Image);
                        }
                        
                        post.Image = imageModel;
                        await _database.Images.AddAsync(imageModel);
                        await _database.SaveChangesAsync();
                    }

                    post.Title = model.Title;
                    post.Content = model.Content;
                    post.Category = postCategory;
                    await _database.SaveChangesAsync();

                    return RedirectToAction("Show", new {id = model.Id});
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Новость или Категория не найдена");
                }
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var post = await _database.Posts.FirstOrDefaultAsync(p => p.Id == id);

                if (post != null)
                {
                    var postImage = await _database.Images.FirstOrDefaultAsync(i => i.Id == post.ImageId);
                    var defaultPostImage = await _database.Images.FirstOrDefaultAsync(i => i.Name == "MovieBlogPostImg");

                    if (postImage != null)
                    {
                        if (postImage.Id != defaultPostImage.Id)
                        {
                            _database.Images.Remove(postImage);
                        }
                        
                        _database.Comments.RemoveRange(_database.Comments.Where(c => c.PostId == id));
                        _database.Posts.Remove(post);
                        await _database.SaveChangesAsync();
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
            }

            return RedirectToAction("GetPostList", "Post");
        }
        
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Show(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var post = await _database.Posts
                    .Include(p => p.Image)
                    .Include(p => p.Category)
                    .Include(p => p.Comments)
                    .FirstOrDefaultAsync(p => p.Id == id);

                var postComments = await _database.Comments
                    .Include(c => c.Post)
                    .Include(c => c.User)
                    .Include(c => c.User.Image)
                    .Where(c => c.PostId == id)
                    .OrderBy(c => c.CreatedAt)
                    .ToListAsync();
                var categories = await _database.Categories.OrderBy(c => c.Name).ToListAsync();
                User user = default;

                if (User.Identity.IsAuthenticated)
                {
                    user = await _database.Users
                        .Include(u => u.Image)
                        .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                }

                if (post != null && user != null)
                {
                    return View(new ShowPostViewModel()
                    {
                        Post = post, Comment = new Comment(), PostComments = postComments, Categories = categories,
                        User = user
                    });
                }
                else if (post != null)
                {
                    return View(new ShowPostViewModel()
                        {Post = post, Comment = new Comment(), PostComments = postComments, Categories = categories});
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> AddToFavorite(string id, string returnUrl)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var post = await _database.Posts.FirstOrDefaultAsync(p => p.Id == id);
                var user = await _database.Users
                    .Include(u => u.UserFavPosts)
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (post != null && user != null)
                {
                    var userPost = new UserFavPost() {User = user, Post = post};
                    await _database.UserFavPosts.AddAsync(userPost);
                    await _database.SaveChangesAsync();

                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromFavorites(string id, string returnUrl)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var post = await _database.Posts.FirstOrDefaultAsync(p => p.Id == id);
                var user = await _database.Users
                    .Include(u => u.UserFavPosts)
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (post != null && user != null)
                {
                    var userPost = await _database.UserFavPosts
                        .FirstOrDefaultAsync(p => p.UserId == user.Id && p.PostId == post.Id);

                    if (userPost != null)
                    {
                        user.UserFavPosts.Remove(userPost);
                        await _userManager.UpdateAsync(user);
                            
                        if (!string.IsNullOrEmpty(returnUrl))
                        {
                            return Redirect(returnUrl);
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
            }
            else
            {
                return View("Error");
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> AddCategory() => View(await _database.Categories.OrderBy(c => c.Name).ToListAsync());

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddCategory(string category)
        {
            if (!string.IsNullOrEmpty(category))
            {
                var allCategories = _database.Categories.Select(s => s.Name).ToList();

                if (!allCategories.Contains(category))
                {
                    var newCategory = new Category() {Name = category};
                    await _database.Categories.AddAsync(newCategory);
                    await _database.SaveChangesAsync();
                }
            }
            
            return View(await _database.Categories.OrderBy(c => c.Name).ToListAsync());
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> RemoveCategory(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var oldCategory = await _database.Categories.FirstOrDefaultAsync(c => c.Id == id);
                
                if (oldCategory != null)
                {
                    _database.Categories.Remove(oldCategory);
                    await _database.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(AddCategory));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetPostList()
        {
            var posts = _database.Posts
                .Include(p => p.Image)
                .Include(p => p.Category)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.Created)
                .ToList();
            
            var categories = await _database.Categories.OrderBy(c => c.Name).ToListAsync();

            return View(new PostListViewModel() {Posts = posts, Categories = categories});
        }
    }
}