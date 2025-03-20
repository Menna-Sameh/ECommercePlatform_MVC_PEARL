using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using DataAcessLayer.Models;
using DataAcessLayer.Context;

public class CategoriesSidebarViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public CategoriesSidebarViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public IViewComponentResult Invoke()
    {
        var categories = _context.Categories
            .Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();

        return View(categories ?? new List<CategoryViewModel>());
    }
}
