using DataAcessLayer.Models;
using DataAcessLayer.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace PresentationLayer.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            return View(categories);
        }

       
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid!");
                return View(category);
            }

            try
            {
                await _unitOfWork.Categories.AddAsync(category);
                int result = await _unitOfWork.SaveAsync(); 

                if (result > 0)
                {
                    TempData["Success"] = "Category added successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Save operation failed!");
                    return View(category);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error adding category: " + ex.Message);
                return View(category);
            }
        }


        public async Task<IActionResult> Edit(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid!");
                return View(category);
            }

            try
            {
                _unitOfWork.Categories.Update(category);
                int result = await _unitOfWork.SaveAsync(); 

                if (result > 0)
                {
                    TempData["Success"] = "Category updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Update operation failed!");
                    return View(category);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error updating category: " + ex.Message);
                return View(category);
            }
        }


        public async Task<IActionResult> Delete(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null) return NotFound();

                _unitOfWork.Categories.Delete(category);
                await _unitOfWork.SaveAsync();
                TempData["Success"] = "Category deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting category: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
