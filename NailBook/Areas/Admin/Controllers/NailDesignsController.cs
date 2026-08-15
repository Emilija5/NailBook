using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NailBook.Data;
using NailBook.Models;

namespace NailBook.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = RoleNames.Admin)]
public class NailDesignsController : Controller
{
    private readonly ApplicationDbContext _context;

    public NailDesignsController(ApplicationDbContext context)
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