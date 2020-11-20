using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBlog.Models;

namespace MovieBlog.Controllers
{
    public class CommentController : Controller
    {
       private readonly ApplicationContext _database;
        private readonly UserManager<User> _userManager;

        public CommentController(ApplicationContext database, UserManager<User> userManager)
        {
            _database = database;
            _userManager = userManager;
        }
        
        [HttpPost]
        public async Task<IActionResult> Add(Comment comment, string postId)
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

            return RedirectToAction("Show", "Post", new {id = postId});
        }
        
        [HttpPost]
        public async Task<IActionResult> Remove(int? id, string postId)
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

            return RedirectToAction("Show", "Post", new {id = postId});
        }
    }
}