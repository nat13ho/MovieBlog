using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MovieBlog.Models;
using MovieBlog.ViewModels;

namespace MovieBlog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationContext _database;
        private readonly IWebHostEnvironment _environment;

        public HomeController(ApplicationContext context, IWebHostEnvironment environment)
        {
            _database = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index(string sortOrder, string searchString, string category, string showAll)
        {
            ViewData["DateSortParam"] = string.IsNullOrEmpty(sortOrder) ? "date_asc" : "";
            ViewData["TitleSortParam"] = sortOrder == "title" ? "title_desc" : "title";
            ViewData["CurrentFilter"] = searchString;
            ViewData["ShowParam"] = showAll == "true" ? "false" : "true";
            ViewBag.PostCount = _database.Posts.Count();
            
            var variable = Convert.ToBoolean(ViewData["ShowParam"]);

            var categories = _database.Categories.ToList();
            var allPosts = _database.Posts
                .Include(p => p.Image)
                .Include(p => p.Category)
                .Include(p => p.Comments)
                .ToList();
            
            switch (sortOrder)
            {
                case "date_asc":
                    allPosts = allPosts.OrderBy(p => p.Created).ToList();
                    break;
                case "title":
                    allPosts = allPosts.OrderBy(p => p.Title.Split(" ").First()).ToList();
                    break;
                case "title_desc":
                    allPosts = allPosts.OrderByDescending(p => p.Title.Split(" ").First()).ToList();
                    break;
                default:
                    allPosts = allPosts.OrderByDescending(p => p.Created).ToList();
                    break;
            }
            
            if (!string.IsNullOrEmpty(searchString))
            {
                allPosts = allPosts
                    .Where(p => p.Title.ToLower().Contains(searchString.ToLower()) ||
                                p.Content.ToLower().Contains(searchString.ToLower()))
                    .ToList();
            }

            if (!string.IsNullOrEmpty(category))
            {
                allPosts = allPosts.Where(p => p.Category.Name.ToLower() == category.ToLower()).ToList();
            }
            
            if (_database.Posts.Count() > 6 && string.IsNullOrEmpty(sortOrder))
            {
                if (Convert.ToBoolean(ViewData["ShowParam"]))
                {
                    allPosts = allPosts.Take(6).ToList();
                }
            }

            if (User.IsInRole("User"))
            {
                var user = await _database.Users
                    .Include(u => u.UserFavPosts)
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                var userPosts = user.UserFavPosts.Select(p => p.Post).ToList();

                return View(new HomeIndexViewModel()
                    {AllPosts = allPosts, UserPosts = userPosts, Categories = categories});
            }
            else
            {
                return View(new HomeIndexViewModel() {AllPosts = allPosts, Categories = categories});
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddImage() => View();

        [HttpPost]
        public async Task<IActionResult> AddImage(IFormFile image)
        {
            if (image != null)
            {
                var path = "/Files/" + image.FileName;

                await using (var fileStream = new FileStream(_environment.WebRootPath + path, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                await _database.Images.AddAsync(new Image {Name = image.FileName, Path = path});
                await _database.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View();
            }
        }
    }
}