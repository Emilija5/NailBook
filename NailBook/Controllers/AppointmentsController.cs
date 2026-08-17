using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NailBook.Data;
using NailBook.Models;
using NailBook.Models.Enums;
using NailBook.ViewModels.Appointments;

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
        LoadBookingFormData();

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

        DateTime? requestedStart = null;

        if (viewModel.AppointmentDate.HasValue &&
            viewModel.AppointmentTime.HasValue)
        {
            requestedStart = viewModel.AppointmentDate.Value
                .ToDateTime(viewModel.AppointmentTime.Value);

            if (requestedStart.Value <= DateTime.Now)
            {
                ModelState.AddModelError(nameof(viewModel.AppointmentDate),
                    "Please choose a future date and time.");
            }
        }

        if (selectedService is not null && requestedStart.HasValue)
        {
            var requestedEnd = requestedStart.Value.AddMinutes(
                selectedService.DurationMinutes);

            var requestedEndTime = TimeOnly.FromDateTime(requestedEnd);

            if (viewModel.AppointmentTime!.Value < SalonHours.OpeningTime ||
                viewModel.AppointmentTime.Value >= SalonHours.ClosingTime)
            {
                ModelState.AddModelError(nameof(viewModel.AppointmentTime),
                    "Appointments are available from 08:00 to 18:00.");
            }
            else if (requestedEndTime > SalonHours.ClosingTime)
            {
                ModelState.AddModelError(nameof(viewModel.AppointmentTime),
                    "This service must finish by 18:00.");
            }
            else
            {
                var confirmedAppointments = _context.Appointments
                    .Include(appointment => appointment.Service)
                    .Where(appointment =>
                        appointment.Status == AppointmentStatus.Confirmed)
                    .ToList();

                var hasConflict = confirmedAppointments.Any(appointment =>
                    requestedStart.Value < appointment.AppointmentDateTime.AddMinutes(
                        appointment.Service.DurationMinutes) &&
                    requestedEnd > appointment.AppointmentDateTime);

                if (hasConflict)
                {
                    ModelState.AddModelError(nameof(viewModel.AppointmentTime),
                        "This time overlaps with a confirmed appointment.");
                }
            }
        }

        if (!ModelState.IsValid)
        {
            LoadBookingFormData(
                viewModel.ServiceId,
                viewModel.AppointmentTime);

            return View(viewModel);
        }

        var appointment = new Appointment
        {
            CustomerId = customerId,
            ServiceId = viewModel.ServiceId!.Value,
            AppointmentDateTime = requestedStart!.Value,
            CustomerNote = viewModel.CustomerNote,
            Status = AppointmentStatus.Pending
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Your appointment request was sent.";

        return RedirectToAction(nameof(Create));
    }

    private void LoadBookingFormData(
        int? selectedServiceId = null,
        TimeOnly? selectedTime = null)
    {
        var services = _context.Services
            .Where(service => service.IsActive)
            .OrderBy(service => service.Name)
            .ToList();

        ViewBag.Services = new SelectList(
            services,
            "Id",
            "Name",
            selectedServiceId);

        var appointmentTimes = new List<SelectListItem>();
        var time = SalonHours.OpeningTime;

        while (time < SalonHours.ClosingTime)
        {
            appointmentTimes.Add(new SelectListItem
            {
                Value = time.ToString("HH:mm"),
                Text = time.ToString("HH:mm"),
                Selected = selectedTime.HasValue &&
                           time == selectedTime.Value
            });

            time = time.AddMinutes(30);
        }

        ViewBag.AppointmentTimes = appointmentTimes;
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var customerId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(customerId))
        {
            return Challenge();
        }

        var appointment = _context.Appointments.FirstOrDefault(appointment =>
            appointment.Id == id &&
            appointment.CustomerId == customerId);

        if (appointment is null)
        {
            return NotFound();
        }

        if (appointment.Status == AppointmentStatus.Pending)
        {
            appointment.Status = AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your appointment request was cancelled.";
        }

        return RedirectToAction(nameof(MyAppointments));
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