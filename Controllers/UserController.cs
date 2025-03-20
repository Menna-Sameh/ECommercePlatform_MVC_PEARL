using DataAcessLayer.Models;
using DataAcessLayer.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class UserController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public UserController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // ✅ عرض الملف الشخصي
    public async Task<IActionResult> Profile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Auth");
        }

        var user = await _unitOfWork.GetRepository<ApplicationUser>().GetByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        return View(user); // 🔹 يعرض صفحة الملف الشخصي
    }

    // ✅ عرض سجل الطلبات
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> OrderHistory()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Auth");
        }

        var orders = await _unitOfWork.GetRepository<Order>().GetAllAsync(o => o.UserId == userId);
        return View(orders); // 🔹 يعرض الطلبات الخاصة بالمستخدم
    }


}
