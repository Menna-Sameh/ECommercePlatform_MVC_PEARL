using DataAcessLayer.Context;
using DataAcessLayer.Models;
using DataAcessLayer.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;


namespace PresentationLayer.Controllers
{
    //[Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "ShoppingCart";
        private IUnitOfWork UnitOfWork;

        public OrderController(ApplicationDbContext context,IUnitOfWork unitOfWork)
        {
            _context = context;
            UnitOfWork = unitOfWork;
        }

        public IActionResult MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                TempData["Error"] = "User not authenticated!";
                return RedirectToAction("Login", "Account");
            }

            var orders = _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product) // التأكد من جلب المنتجات مع الطلبات
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }



        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            Console.WriteLine("PlaceOrder action started...");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                Console.WriteLine("User not authenticated!");
                TempData["Error"] = "User not authenticated!";
                return RedirectToAction("Login", "Account");
            }

            // جلب المنتجات من الـ Session
            var cartItemsJson = HttpContext.Session.GetString("CartSessionKey");
            if (string.IsNullOrEmpty(cartItemsJson))
            {
                Console.WriteLine("Cart is empty!");
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("Index", "Cart");
            }

            // تحويل البيانات من JSON إلى قائمة `CartItem`
            var cartItems = JsonSerializer.Deserialize<List<CartItem>>(cartItemsJson);

            if (cartItems == null || !cartItems.Any())
            {
                Console.WriteLine("Cart is empty after deserialization!");
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("Index", "Cart");
            }

            // جلب بيانات المنتجات من قاعدة البيانات
            var orderItems = new List<OrderItem>();
            foreach (var item in cartItems)
            {
                var product = await UnitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    orderItems.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        Price = product.Price
                    });
                }
            }

            if (!orderItems.Any())
            {
                Console.WriteLine("No valid products found in the cart!");
                TempData["Error"] = "No valid products found in the cart!";
                return RedirectToAction("Index", "Cart");
            }

            // إنشاء الطلب
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                OrderItems = orderItems
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // مسح السلة من الـ Session
            HttpContext.Session.Remove("CartSessionKey");

            Console.WriteLine("Order placed successfully!");
            TempData["Success"] = "Order placed successfully!";
            return RedirectToAction("MyOrders");
        }






        public IActionResult OrderDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = _context.Orders
                                .Include(o => o.OrderItems)
                                .ThenInclude(oi => oi.Product)
                                .FirstOrDefault(o => o.Id == id && o.UserId == userId); // التحقق من الـ UserId

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public IActionResult TotalOrder(decimal r)
        {
            var totalPrice = r;
            return View("payment1", totalPrice);
        }

        [HttpPost]
        public IActionResult CreateCheckoutSession(decimal totalAmount)
        {
            var domain = "https://localhost:7177"; // استبدليها بـ URL الفعلي عند النشر

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = (long)(totalAmount * 100), // تحويل إلى سنتات
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Order Payment"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = $"{domain}/Order/Success",
                CancelUrl = $"{domain}/Order/Failed"
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return Redirect(session.Url);
        }

        public IActionResult Success()
        {
            return View();
        }

        public IActionResult Failed()
        {
            return View();
        }
    }

}

