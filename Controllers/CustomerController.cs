using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAcessLayer.Models;
using System.Linq;
using DataAcessLayer.Context;

namespace PresentationLayer.Controllers
{
    [Authorize(Roles = "Customer,Admin")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Products()
        {
            var products = _context.Products
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    ImageUrl = p.ImageUrl,
                    Price = p.Price,
                    IsFavorite = false
                })
                .ToList();

            var categories = _context.Categories
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToList();

            var viewModel = new ProductsPageViewModel
            {
                Products = products,
                Categories = categories
            };

            return View(viewModel);
        }



        [HttpPost]
        public IActionResult ToggleFavorite(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                product.IsFavorite = !product.IsFavorite;
                _context.SaveChanges();
                return Ok(new { success = true, isFavorite = product.IsFavorite });
            }
            return NotFound(new { success = false, message = "Product not found" });
        }

        public IActionResult Categories()
        {
            var categories = _context.Categories
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Products = c.Products.Select(p => new ProductViewModel
                    {
                        Id = p.Id,
                        Name = p.Name,
                        ImageUrl = p.ImageUrl,
                        Price = p.Price,
                        IsFavorite = false
                    }).ToList()
                }).ToList();

            return PartialView("_CategoriesSidebar", categories);
        }

        public IActionResult ProductsByCategory(int categoryId)
{
    var products = _context.Products
        .Where(p => p.CategoryId == categoryId)
        .Select(p => new ProductViewModel
        {
            Id = p.Id,
            Name = p.Name,
            ImageUrl = p.ImageUrl,
            Price = p.Price,
            IsFavorite = false
        })
        .ToList();

    var categories = _context.Categories
        .Select(c => new CategoryViewModel
        {
            Id = c.Id,
            Name = c.Name
        })
        .ToList();

    var viewModel = new ProductsPageViewModel
    {
        Products = products,
        Categories = categories
    };

    return View("Products", viewModel); 
}


       
        [HttpPost]
        public IActionResult AddToCart(int productId)
        {
            var product = _context.Products.Find(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            List<int> cart = HttpContext.Session.GetObject<List<int>>("Cart") ?? new List<int>();

          
            cart.Add(productId);

            HttpContext.Session.SetObject("Cart", cart);

            return Json(new { success = true, message = "Product added to cart!" });
        }



        public IActionResult ProductDetails(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            var productViewModel = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl
            };

            return View(productViewModel); 
        }


    }
}
