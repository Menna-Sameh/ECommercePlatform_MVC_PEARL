using DataAcessLayer.Models;
using DataAcessLayer.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

public class ProductController : Controller
{
    
   // [Authorize(Roles = "Admin")]
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProductController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    [Authorize(Roles = "Admin, Buyer")]
    public async Task<IActionResult> Index()
    {
        var products = await _unitOfWork.Products.GetAllAsync(p => p.Category);
        return View(products);
    }

    [Authorize(Roles = "Admin, Buyer")]
    public async Task<IActionResult> Create()
    {
        ViewBag.CategoryId = new SelectList(await _unitOfWork.Categories.GetAllAsync(), "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin, Buyer")]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.CategoryId = new SelectList(await _unitOfWork.Categories.GetAllAsync(), "Id", "Name");
            return View(product);
        }

        var user = await _userManager.GetUserAsync(User);
        product.BuyerId = user.Id;  

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveAsync();
        return RedirectToAction(nameof(MyProducts));
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, p => p.Category);
        if (product == null) return NotFound();
        return View(product);
    }

    //[Authorize(Roles = "Admin, Buyer")]
    //public async Task<IActionResult> Edit(int id)
    //{
    //    var product = await _unitOfWork.Products.GetByIdAsync(id, p => p.Category);
    //    if (product == null) return NotFound();

    //    var user = await _userManager.GetUserAsync(User);
    //    if (User.IsInRole("Buyer") && product.BuyerId != user.Id)
    //    {
    //        return Forbid();
    //    }

    //    ViewBag.CategoryId = new SelectList(await _unitOfWork.Categories.GetAllAsync(), "Id", "Name", product.CategoryId);
    //    return View(product);
    //}

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //[Authorize(Roles = "Admin, Buyer")]
    //public async Task<IActionResult> Edit(int id, Product product)
    //{
    //    if (id != product.Id) return NotFound();

    //    if (!ModelState.IsValid)
    //    {
    //        ViewBag.CategoryId = new SelectList(await _unitOfWork.Categories.GetAllAsync(), "Id", "Name", product.CategoryId);
    //        return View(product);
    //    }

    //    var user = await _userManager.GetUserAsync(User);
    //    var existingProduct = await _unitOfWork.Products.GetByIdAsync(id);
    //    if (User.IsInRole("Buyer") && existingProduct.BuyerId != user.Id)
    //    {
    //        return Forbid();
    //    }

    //    _unitOfWork.Products.Update(product);
    //    await _unitOfWork.SaveAsync();
    //    return RedirectToAction(nameof(Index));
    //}

    //[Authorize(Roles = "Admin")]
    //public async Task<IActionResult> Delete(int id)
    //{
    //    var product = await _unitOfWork.Products.GetByIdAsync(id, p => p.Category);
    //    if (product == null) return NotFound();
    //    return View(product);
    //}

    //[HttpPost, ActionName("Delete")]
    //[ValidateAntiForgeryToken]
    //[Authorize(Roles = "Admin")]
    //public async Task<IActionResult> DeleteConfirmed(int id)
    //{
    //    var product = await _unitOfWork.Products.GetByIdAsync(id);
    //    if (product == null) return NotFound();

    //    _unitOfWork.Products.Delete(product);
    //    await _unitOfWork.SaveAsync();
    //    return RedirectToAction(nameof(Index));
    //}


    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> MyProducts()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        var products = (await _unitOfWork.Products.GetAllAsync(p => p.Category))
            .Where(p => p.BuyerId == user.Id)
            .ToList();

        return View(products);
    }


    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AllProductsForAdmin()
    {
        var products = await _unitOfWork.Products.GetAllAsync(p => p.Category);
        return View("MyProducts", products);  // إعادة استخدام نفس الفيو
    }


}
