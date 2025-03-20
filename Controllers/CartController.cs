using DataAcessLayer.Context;
using DataAcessLayer.Models;
using DataAcessLayer.Models.ViewModels;
using DataAcessLayer.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PresentationLayer.Controllers
{
    public class CartController : Controller
    {
        private const string CartSessionKey = "ShoppingCart";
        private readonly IGenericRepository<Product> _productRepository;
        private readonly ApplicationDbContext _context;

        public CartController(IGenericRepository<Product> productRepository, ApplicationDbContext context)
        {
            _productRepository = productRepository;
            _context = context;
        }


        public IActionResult Index()
        {
            var cart = GetCart();
            ViewBag.TotalPrice = cart.Sum(item => item.Price * item.Quantity);
            return View(cart);
        }

        public IActionResult CartIndex()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = _context.CartItems
                .Where(ci => ci.UserId == userId)
                .Include(ci => ci.Product).ToList();
            //foreach (var Item in cartItems) {
            //    CartItemOrderViewModel cartvm = new CartItemOrderViewModel
            //    {
            //        Id = Item.Id,
            //        ImageUrl = Item.ImageUrl,
            //        Name = Item.Name,
            //        OrderID=Item.

            //    };
            return View(cartItems);
        }
        public async Task<IActionResult> AddToCart(int id)
        {
            try
            {
                var cart = GetCart();
                var product = await _productRepository.GetByIdAsync(id);

                if (product != null)
                {
                    var existingItem = cart.FirstOrDefault(p => p.Id == id);
                    if (existingItem != null)
                    {
                        existingItem.Quantity++;
                    }
                    else
                    {
                        cart.Add(new CartItem
                        {
                            Id = product.Id,
                            Name = product.Name,
                            Price = product.Price,
                            ImageUrl = product.ImageUrl,
                            Quantity = 1
                        });
                    }
                    SaveCart(cart);
                }

                return RedirectToAction("Index", "Product");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();
            var itemToRemove = cart.FirstOrDefault(p => p.Id == id);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        private List<CartItem> GetCart()
        {
            var cart = HttpContext.Session.GetString(CartSessionKey);
            return cart != null ? JsonConvert.DeserializeObject<List<CartItem>>(cart) : new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonConvert.SerializeObject(cart));
        }

        public IActionResult GetCartItems()
        {
            var cart = GetCart();
            ViewBag.TotalPrice = cart.Sum(item => item.Price * item.Quantity);
            return PartialView("_CartSidebar", cart);
        }

        public IActionResult GetCartCount()
        {
            var cart = GetCart();
            return Json(cart.Sum(item => item.Quantity));
        }

        [Authorize]
        public IActionResult Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = _context.CartItems.Where(c => c.UserId == userId).ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("MyOrders");
            }

            var order = new Order
            {
                UserId = userId,
                OrderItems = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cartItems); // حذف المنتجات من الكارت
            _context.SaveChanges();

            TempData["Success"] = "Order placed successfully!";
            return RedirectToAction("MyOrders");
        }


    }
}
