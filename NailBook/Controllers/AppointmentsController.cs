using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NailBook.Data;
using NailBook.Models;
using NailBook.ViewModels.Appointments;
using Microsoft.AspNetCore.Identity;
using NailBook.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace NailBook.Controllers;

[Authorize(Roles = RoleNames.Customer)]
public class AppointmentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AppointmentsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Create()
    {
        var services = _context.Services
            .Where(service => service.IsActive)
            .OrderBy(service => service.Name)
            .ToList();

        ViewBag.Services = new SelectList(services, "Id", "Name");

        return View(new CreateAppointmentViewModel());
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAppointmentViewModel viewModel)
    {
        var customerId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(customerId))
        {
            return Challenge();
        }

        if (viewModel.ServiceId.HasValue)
        {
            var serviceExists = _context.Services.Any(service =>
                service.Id == viewModel.ServiceId.Value && service.IsActive);

            if (!serviceExists)
            {
                ModelState.AddModelError(nameof(viewModel.ServiceId),
                    "Please choose an active service.");
            }
        }

        if (viewModel.AppointmentDateTime.HasValue &&
            viewModel.AppointmentDateTime.Value <= DateTime.Now)
        {
            ModelState.AddModelError(nameof(viewModel.AppointmentDateTime),
                "Please choose a future date and time.");
        }

        if (!ModelState.IsValid)
        {
            var services = _context.Services
                .Where(service => service.IsActive)
                .OrderBy(service => service.Name)
                .ToList();

            ViewBag.Services = new SelectList(
                services,
                "Id",
                "Name",
                viewModel.ServiceId);

            return View(viewModel);
        }

        var appointment = new Appointment
        {
            CustomerId = customerId,
            ServiceId = viewModel.ServiceId!.Value,
            AppointmentDateTime = viewModel.AppointmentDateTime!.Value,
            CustomerNote = viewModel.CustomerNote,
            Status = AppointmentStatus.Pending
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Your appointment request was sent.";

        return RedirectToAction(nameof(Create));
    }
    
    public IActionResult MyAppointments()
    {
        var customerId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(customerId))
        {
            return Challenge();
        }

        var appointments = _context.Appointments
            .Include(appointment => appointment.Service)
            .Where(appointment => appointment.CustomerId == customerId)
            .OrderByDescending(appointment => appointment.AppointmentDateTime)
            .ToList();

        return View(appointments);
    }
}