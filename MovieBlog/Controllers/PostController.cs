using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBlog.Models;
using MovieBlog.ViewModels;

namespace MovieBlog.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly ApplicationContext _database;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _environment;

        public PostController(ApplicationContext context, UserManager<User> userManager,
            IWebHostEnvironment environment)
        {
            _environment = environment;
            _database = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreatePostViewModel() {AllCategories = _database.Categories.ToList()});
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CreatePostViewModel model, string category)
        {
            var defaultPostImage = await _database.Images.FirstOrDefaultAsync(i => i.Name == "movieblog-postImage.jpg");
            
            if (ModelState.IsValid)
            {
                var postCategory = await _database.Categories.FirstOrDefaultAsync(c => c.Name == category);

                if (postCategory != null)
                {
                    Post post = default;
                    
                    if (model.Image != null)
                    {
                        var listOfImages = _database.Images.Select(i => i.Name).ToList();

                        if (listOfImages.Contains(model.Image.FileName))
                        {
                            var newImage = await _database.Images.FirstOrDefaultAsync(i => i.Name == model.Image.FileName);

                            if (newImage != null)
                            {
                                post = new Post()
                                    {Title = model.Title, Content = model.Content, Category = postCategory, Image = newImage};
                            }
                        }
                        else
                        {
                            var path = "/Files/" + model.Image.FileName;

                            await using (var fileStream = new FileStream(_environment.WebRootPath + path, FileMode.Create))
                            {
                                await model.Image.CopyToAsync(fileStream);
                            }

                            await _database.Images.AddAsync(new Image {Name = model.Image.FileName, Path = path});
                            await _database.SaveChangesAsync();

                            var image = await _database.Images.FirstOrDefaultAsync(i => i.Path == path);
                            post = new Post()
                                {Title = model.Title, Content = model.Content, Category = postCategory, Image = image};
                        }
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

            return View(new CreatePostViewModel() {AllCategories = _database.Categories.ToList()});
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (id != null)
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
                        AllCategories = _database.Categories.ToList()
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

                if (post != null && postCategory != null)
                {
                    if (model.Image != null)
                    {
                        var path = "/Files/" + model.Image.FileName;

                        await using (var fileStream = new FileStream(_environment.WebRootPath + path, FileMode.Create))
                        {
                            await model.Image.CopyToAsync(fileStream);
                        }

                        await _database.Images.AddAsync(new Image {Name = model.Image.FileName, Path = path});
                        await _database.SaveChangesAsync();
                        var image = await _database.Images.FirstOrDefaultAsync(i => i.Path == path);
                        post.Image = image;
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
            if (id != null)
            {
                var post = await _database.Posts.FirstOrDefaultAsync(p => p.Id == id);

                if (post != null)
                {
                    var postImage = await _database.Images.FirstOrDefaultAsync(i => i.Id == post.ImageId);
                    var defaultPostImage = await _database.Images.FirstOrDefaultAsync(i => i.Name == "movieblog-postImage.jpg");

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

        [HttpPost]
        public async Task<IActionResult> RemoveComment(int? id, string postId)
        {
            if (id != null && postId != null)
            {
                var comment = await _database.Comments.FirstOrDefaultAsync(c => c.Id == id);
                var post = await _database.Posts.FirstOrDefaultAsync(p => p.Id == postId);

                if (comment != null && post != null)
                {
                    post.Comments.Remove(comment);
                    _database.Comments.Remove(comment);
                    await _database.SaveChangesAsync();
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            return RedirectToAction("Show", new {id = postId});
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(Comment comment, string postId)
        {
            if (ModelState.IsValid)
            {
                var post = await _database.Posts.FirstOrDefaultAsync(p => p.Id == postId);
                var user = await _userManager.FindByNameAsync(User.Identity.Name);

                if (post != null && user != null)
                {
                    comment.Post = post;
                    comment.User = user;
                    post.Comments.Add(comment);
                    await _database.Comments.AddAsync(comment);
                    await _database.SaveChangesAsync();
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            return RedirectToAction("Show", new {id = postId});
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Show(string id)
        {
            if (id != null)
            {
                var post = await _database.Posts
                    .Include(p => p.Image)
                    .Include(p => p.Category)
                    .Include(p => p.Comments)
                    .FirstOrDefaultAsync(p => p.Id == id);

                var postComments = _database.Comments
                    .Include(c => c.Post)
                    .Include(c => c.User)
                    .Include(c => c.User.Image)
                    .Where(c => c.PostId == id)
                    .ToList();
                var categories = _database.Categories.ToList();
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
        public async Task<IActionResult> AddToFavorite(string id)
        {
            if (id != null)
            {
                var post = await _database.Posts.FirstOrDefaultAsync(p => p.Id == id);
                var user = await _database.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (post != null && user != null)
                {
                    var userPost = new UserFavPost() {User = user, Post = post};
                    await _database.UserFavPosts.AddAsync(userPost);
                    await _database.SaveChangesAsync();

                    if (!user.UserFavPosts.Contains(userPost))
                    {
                        user.UserFavPosts.Add(userPost);
                        await _userManager.UpdateAsync(user);
                    }
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromFavorites(string id)
        {
            if (id != null)
            {
                var post = await _database.Posts.FirstOrDefaultAsync(p => p.Id == id);
                var user = await _database.Users
                    .Include(u => u.UserFavPosts)
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (post != null && user != null)
                {
                    var userPost = await _database.UserFavPosts
                        .FirstOrDefaultAsync(p => p.UserId == user.Id);

                    if (userPost != null)
                    {
                        if (user.UserFavPosts.Contains(userPost))
                        {
                            user.UserFavPosts.Remove(userPost);
                            await _userManager.UpdateAsync(user);
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
        public IActionResult AddCategory() => View(_database.Categories.ToList());

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
            
            return View(_database.Categories.ToList());
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> RemoveCategory(int? id)
        {
            if (id != null)
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
        public IActionResult GetPostList()
        {
            var posts = _database.Posts
                .Include(p => p.Image)
                .Include(p => p.Category)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.Created)
                .ToList();
            
            var categories = _database.Categories.ToList();

            return View(new PostListViewModel() {Posts = posts, Categories = categories});
        }
    }
}