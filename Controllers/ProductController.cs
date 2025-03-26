using BusinessLogicLayer.Services;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileservice _fileService;

    public ProductController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IFileservice fileService)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _fileService = fileService;
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

    [Authorize(Roles = "Admin, Buyer")]
    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.CategoryId = new SelectList(await _unitOfWork.Categories.GetAllAsync(), "Id", "Name");
            return View(product);
        }

        var user = await _userManager.GetUserAsync(User);
        product.BuyerId = user.Id;
        var uniqueFileName = _fileService.UploadFile(product.formfile, "Images");
        product.ImageUrl = uniqueFileName;
        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveAsync();
        return RedirectToAction(nameof(MyProducts));
    }

    [Authorize(Roles = "Admin, Buyer")]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null) return NotFound();
        ViewBag.CategoryId = new SelectList(await _unitOfWork.Categories.GetAllAsync(), "Id", "Name", product.CategoryId);
        return View(product);
    }

    [HttpPost]
    [Authorize(Roles = "Admin, Buyer")]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.Id) return BadRequest(); 

        if (!ModelState.IsValid)
        {
            ViewBag.CategoryId = new SelectList(await _unitOfWork.Categories.GetAllAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        var existingProduct = await _unitOfWork.Products.GetByIdAsync(id);
        if (existingProduct == null) return NotFound();

        existingProduct.Name = product.Name;
        existingProduct.Price = product.Price;
        existingProduct.CategoryId = product.CategoryId;
        existingProduct.Description = product.Description;

        if (product.formfile != null)
        {
            var uniqueFileName = _fileService.UploadFile(product.formfile, "Images");
            existingProduct.ImageUrl = uniqueFileName;
        }

        await _unitOfWork.Products.UpdateAsync(existingProduct);
        await _unitOfWork.SaveAsync();

        return RedirectToAction(nameof(MyProducts));
    }


    [Authorize(Roles = "Admin, Buyer")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null) return NotFound();
        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin, Buyer")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null) return NotFound();

        await _unitOfWork.Products.DeleteAsync(product); 
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
        return View("MyProducts", products);
    }
}
