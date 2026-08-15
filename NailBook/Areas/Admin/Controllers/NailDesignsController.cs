using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NailBook.Data;
using NailBook.Models;
using NailBook.ViewModels.NailDesigns;

namespace NailBook.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = RoleNames.Admin)]
public class NailDesignsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public NailDesignsController(
        ApplicationDbContext context,
        IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Index()
    {
        var nailDesigns = _context.NailDesigns
            .OrderByDescending(design => design.CreatedAt)
            .ToList();

        return View(nailDesigns);
    }
    public IActionResult Create()
    {
        return View(new CreateNailDesignViewModel());
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateNailDesignViewModel viewModel)
    {
        if (viewModel.ImageFile is null)
        {
            ModelState.AddModelError(nameof(viewModel.ImageFile),
                "Please choose an image.");
        }
        else
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var fileExtension = Path.GetExtension(viewModel.ImageFile.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError(nameof(viewModel.ImageFile),
                    "Please upload a JPG, PNG, or WEBP image.");
            }

            if (viewModel.ImageFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(nameof(viewModel.ImageFile),
                    "The image must be smaller than 5 MB.");
            }
        }

        if (!ModelState.IsValid || viewModel.ImageFile is null)
        {
            return View(viewModel);
        }

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(viewModel.ImageFile.FileName)}";
        var uploadsFolder = Path.Combine(
            _webHostEnvironment.WebRootPath,
            "uploads",
            "nail-designs");

        Directory.CreateDirectory(uploadsFolder);

        var filePath = Path.Combine(uploadsFolder, fileName);

        await using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await viewModel.ImageFile.CopyToAsync(fileStream);
        }

        var nailDesign = new NailDesign
        {
            Title = viewModel.Title,
            Description = viewModel.Description,
            Style = viewModel.Style,
            ImageUrl = $"/uploads/nail-designs/{fileName}"
        };

        _context.NailDesigns.Add(nailDesign);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
    
    public IActionResult Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var nailDesign = _context.NailDesigns.Find(id);

        if (nailDesign is null)
        {
            return NotFound();
        }

        var viewModel = new EditNailDesignViewModel
        {
            Id = nailDesign.Id,
            Title = nailDesign.Title,
            Description = nailDesign.Description,
            Style = nailDesign.Style,
            ImageUrl = nailDesign.ImageUrl
        };

        return View(viewModel);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditNailDesignViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return NotFound();
        }

        var nailDesign = _context.NailDesigns.Find(id);

        if (nailDesign is null)
        {
            return NotFound();
        }

        if (viewModel.ImageFile is not null)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var fileExtension = Path.GetExtension(viewModel.ImageFile.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError(nameof(viewModel.ImageFile),
                    "Please upload a JPG, PNG, or WEBP image.");
            }

            if (viewModel.ImageFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(nameof(viewModel.ImageFile),
                    "The image must be smaller than 5 MB.");
            }
        }

        if (!ModelState.IsValid)
        {
            viewModel.ImageUrl = nailDesign.ImageUrl;
            return View(viewModel);
        }

        nailDesign.Title = viewModel.Title;
        nailDesign.Description = viewModel.Description;
        nailDesign.Style = viewModel.Style;

        if (viewModel.ImageFile is not null)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(viewModel.ImageFile.FileName)}";
            var uploadsFolder = Path.Combine(
                _webHostEnvironment.WebRootPath,
                "uploads",
                "nail-designs");

            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);

            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await viewModel.ImageFile.CopyToAsync(fileStream);
            }

            nailDesign.ImageUrl = $"/uploads/nail-designs/{fileName}";
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
    
    public IActionResult Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var nailDesign = _context.NailDesigns.Find(id);

        if (nailDesign is null)
        {
            return NotFound();
        }

        return View(nailDesign);
    }
    
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var nailDesign = _context.NailDesigns.Find(id);

        if (nailDesign is null)
        {
            return NotFound();
        }

        _context.NailDesigns.Remove(nailDesign);
        await _context.SaveChangesAsync();

        var fileName = Path.GetFileName(nailDesign.ImageUrl);
        var imagePath = Path.Combine(
            _webHostEnvironment.WebRootPath,
            "uploads",
            "nail-designs",
            fileName);

        if (System.IO.File.Exists(imagePath))
        {
            System.IO.File.Delete(imagePath);
        }

        return RedirectToAction(nameof(Index));
    }
}