using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NailBook.Data;
using NailBook.Models;
using NailBook.Models.Enums;
using NailBook.ViewModels.Reviews;

namespace NailBook.Controllers;

[Authorize(Roles = RoleNames.Customer)]
public class ReviewsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReviewsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Create(int id)
    {
        var customerId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(customerId))
        {
            return Challenge();
        }

        var appointmentExists = await _context.Appointments.AnyAsync(appointment =>
            appointment.Id == id &&
            appointment.CustomerId == customerId &&
            appointment.Status == AppointmentStatus.Completed &&
            appointment.Review == null);

        if (!appointmentExists)
        {
            return NotFound();
        }

        return View(new CreateReviewViewModel { AppointmentId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateReviewViewModel viewModel)
    {
        var customerId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(customerId))
        {
            return Challenge();
        }

        var appointment = await _context.Appointments
            .Include(appointment => appointment.Review)
            .SingleOrDefaultAsync(appointment =>
                appointment.Id == viewModel.AppointmentId &&
                appointment.CustomerId == customerId &&
                appointment.Status == AppointmentStatus.Completed);

        if (appointment is null || appointment.Review is not null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var review = new Review
        {
            CustomerId = customerId,
            AppointmentId = appointment.Id,
            Rating = viewModel.Rating!.Value,
            Comment = viewModel.Comment
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Thank you for your review.";

        return RedirectToAction("MyAppointments", "Appointments");
    }
}
