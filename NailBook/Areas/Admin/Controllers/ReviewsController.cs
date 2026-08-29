using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NailBook.Data;
using NailBook.Models;

namespace NailBook.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = RoleNames.Admin)]
public class ReviewsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReviewsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var reviews = await _context.Reviews
            .Include(review => review.Customer)
            .Include(review => review.Appointment)
            .ThenInclude(appointment => appointment.Service)
            .OrderByDescending(review => review.CreatedAt)
            .ToListAsync();

        return View(reviews);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleVisibility(int id)
    {
        var review = await _context.Reviews.FindAsync(id);

        if (review is null)
        {
            return NotFound();
        }

        review.IsVisible = !review.IsVisible;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = review.IsVisible
            ? "Review is visible on the public page."
            : "Review is hidden from the public page.";

        return RedirectToAction(nameof(Index));
    }
}
