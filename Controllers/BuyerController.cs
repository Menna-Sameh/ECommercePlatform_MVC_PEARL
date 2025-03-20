using DataAcessLayer.Models;
using DataAcessLayer.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PresentationLayer.Controllers
{
    [Authorize(Roles = "Buyer,Admin")]
    public class BuyerController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public BuyerController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Dashboard()
        {
            return View();
        }

        public async Task<IActionResult> SellerOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var orders = await _unitOfWork.GetRepository<Order>()
                .GetAllAsync(o => o.OrderItems.Any(oi => oi.Product.BuyerId == userId));

            return View(orders);
        }

    }
}
