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

        Service? selectedService = null;

        if (viewModel.ServiceId.HasValue)
        {
            selectedService = _context.Services.FirstOrDefault(service =>
                service.Id == viewModel.ServiceId.Value && service.IsActive);

            if (selectedService is null)
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
        
        if (selectedService is not null &&
            viewModel.AppointmentDateTime.HasValue)
        {
            var requestedStart = viewModel.AppointmentDateTime.Value;
            var requestedEnd = requestedStart.AddMinutes(
                selectedService.DurationMinutes);

            var confirmedAppointments = _context.Appointments
                .Include(appointment => appointment.Service)
                .Where(appointment => appointment.Status == AppointmentStatus.Confirmed)
                .ToList();

            var hasConflict = confirmedAppointments.Any(appointment =>
                requestedStart < appointment.AppointmentDateTime.AddMinutes(
                    appointment.Service.DurationMinutes) &&
                requestedEnd > appointment.AppointmentDateTime);

            if (hasConflict)
            {
                ModelState.AddModelError(nameof(viewModel.AppointmentDateTime),
                    "This time overlaps with a confirmed appointment.");
            }
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