using DataAcessLayer.Context;
using DataAcessLayer.Models;
using DataAcessLayer.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

public class AuthController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;


    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _context = context;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByNameAsync(model.UserName);
        if (user == null)
        {
            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            ModelState.AddModelError("", "Your account is locked. Please try again later.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);

            var roles = await _userManager.GetRolesAsync(user);
            string role = roles.FirstOrDefault() ?? "Customer";

            Console.WriteLine($"🔎 User {user.UserName} has roles: {string.Join(", ", roles)}");

            return role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Buyer" => RedirectToAction("Dashboard", "Buyer"),
                "Customer" => RedirectToAction("Dashboard", "Customer"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        ModelState.AddModelError("", "Username or password is incorrect.");
        return View(model);
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (await _userManager.FindByNameAsync(model.UserName) != null)
        {
            ModelState.AddModelError("", "Username is already taken.");
            return View(model);
        }

        if (await _userManager.FindByEmailAsync(model.Email) != null)
        {
            ModelState.AddModelError("", "Email is already registered.");
            return View(model);
        }

        string userRole = string.IsNullOrEmpty(model.Role) ? "Customer" : model.Role;

        var user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            FullName = model.FullName,
            Role = userRole
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View(model);
        }

        if (!await _roleManager.RoleExistsAsync(userRole))
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(userRole));
            if (!roleResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to create role.");
                return View(model);
            }
        }

        var roleAssignmentResult = await _userManager.AddToRoleAsync(user, userRole);
        if (!roleAssignmentResult.Succeeded)
        {
            ModelState.AddModelError("", "Failed to assign role.");
            return View(model);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);

        return userRole switch
        {
            "Admin" => RedirectToAction("Dashboard", "Admin"),
            "Buyer" => RedirectToAction("Dashboard", "Buyer"),
            "Customer" => RedirectToAction("Dashboard", "Customer"),
            _ => RedirectToAction("Index", "Home")
        };
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}
