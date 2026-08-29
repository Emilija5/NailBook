using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NailBook.Data;
using NailBook.Models;
using NailBook.Models.Enums;

namespace NailBook.Controllers;

public class GalleryController : Controller
{
    private readonly ApplicationDbContext _context;

    public GalleryController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index(NailDesignStyle? style)
    {
        IQueryable<NailDesign> nailDesigns = _context.NailDesigns;

        if (style.HasValue)
        {
            nailDesigns = nailDesigns.Where(design => design.Style == style.Value);
        }

        ViewBag.Styles = Enum.GetValues<NailDesignStyle>()
            .Select(styleOption => new SelectListItem
            {
                Value = styleOption.ToString(),
                Text = styleOption.ToString(),
                Selected = style == styleOption
            });

        return View(nailDesigns
            .OrderByDescending(design => design.CreatedAt)
            .ToList());
    }
}
