using Microsoft.AspNetCore.Mvc;
using NailBook.Data;

namespace NailBook.Controllers;

public class GalleryController : Controller
{
    private readonly ApplicationDbContext _context;

    public GalleryController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var nailDesigns = _context.NailDesigns
            .OrderByDescending(design => design.CreatedAt)
            .ToList();

        return View(nailDesigns);
    }
}