using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MovieBlog.Models;
using MovieBlog.ViewModels;

namespace MovieBlog.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationContext _database;


        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<User> userManager, ApplicationContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _database = context;
        }
        
        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            var categories = _database.Categories.ToList();
            return View(new RoleIndexViewModel() {Roles = roles, Categories = categories});
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(string roleName)
        {
            if (!string.IsNullOrEmpty(roleName))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(roleName));

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index)); 
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }
        
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var role = await _roleManager.FindByIdAsync(id);

                if (role != null)
                {
                    var result = await _roleManager.DeleteAsync(role);
                }
            }
            
            return RedirectToAction(nameof(Index));
        }
        
        [HttpGet]
        public IActionResult GetUserList()
        {
            var users = _userManager.Users.ToList();
            var categories = _database.Categories.ToList();

            return View(new UserListViewModel() {Users = users, Categories = categories});
        }
        
        [HttpGet]
        public async Task<IActionResult> Edit(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var allRoles = _roleManager.Roles.ToList();
                
                var model = new ChangeRoleViewModel
                {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    UserRoles = userRoles,
                    AllRoles = allRoles
                };
                
                return View(model);
            }
 
            return NotFound();
        }
        
        [HttpPost]
        public async Task<IActionResult> Edit(string userId, List<string> roles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var allRoles = _roleManager.Roles.ToList();
                var addedRoles = roles.Except(userRoles);
                var removedRoles = userRoles.Except(roles);
 
                await _userManager.AddToRolesAsync(user, addedRoles);
                await _userManager.RemoveFromRolesAsync(user, removedRoles);
 
                return RedirectToAction(nameof(GetUserList));
            }
 
            return NotFound();
        }
    }
}