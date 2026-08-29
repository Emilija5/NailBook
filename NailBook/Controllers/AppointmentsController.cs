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
    private readonly IWebHostEnvironment _webHostEnvironment;

    public AppointmentsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _userManager = userManager;
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Create()
    {
        LoadBookingFormData();

        return View(new CreateAppointmentViewModel());
    }

    [HttpGet]
    public async Task<IActionResult> AvailableTimes(
        int? serviceId,
        DateOnly? appointmentDate)
    {
        if (!serviceId.HasValue ||
            !appointmentDate.HasValue ||
            appointmentDate.Value < DateOnly.FromDateTime(DateTime.Today) ||
            appointmentDate.Value.DayOfWeek == DayOfWeek.Sunday)
        {
            return Json(Array.Empty<string>());
        }

        var service = await _context.Services.SingleOrDefaultAsync(service =>
            service.Id == serviceId.Value && service.IsActive);

        if (service is null)
        {
            return Json(Array.Empty<string>());
        }

        var startOfDay = appointmentDate.Value.ToDateTime(TimeOnly.MinValue);
        var nextDay = startOfDay.AddDays(1);

        var confirmedAppointments = await _context.Appointments
            .Include(appointment => appointment.Service)
            .Where(appointment =>
                (appointment.Status == AppointmentStatus.Pending ||
                 appointment.Status == AppointmentStatus.Confirmed) &&
                appointment.AppointmentDateTime >= startOfDay &&
                appointment.AppointmentDateTime < nextDay)
            .ToListAsync();

        var availableTimes = GetAppointmentTimes()
            .Where(time =>
            {
                var requestedStart = appointmentDate.Value.ToDateTime(time);
                var requestedEnd = requestedStart.AddMinutes(service.DurationMinutes);

                if (requestedStart <= DateTime.Now ||
                    requestedEnd > appointmentDate.Value.ToDateTime(
                        SalonHours.ClosingTime))
                {
                    return false;
                }

                return !confirmedAppointments.Any(appointment =>
                    requestedStart < appointment.AppointmentDateTime.AddMinutes(
                        appointment.Service.DurationMinutes) &&
                    requestedEnd > appointment.AppointmentDateTime);
            })
            .Select(time => time.ToString("HH:mm"))
            .ToList();

        return Json(availableTimes);
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
        
        if (viewModel.AppointmentDate.HasValue &&
            viewModel.AppointmentDate.Value.DayOfWeek == DayOfWeek.Sunday)
        {
            ModelState.AddModelError(nameof(viewModel.AppointmentDate),
                "The salon is closed on Sundays.");
        }

        if (viewModel.InspirationImageFile is not null)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var fileExtension = Path.GetExtension(viewModel.InspirationImageFile.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError(nameof(viewModel.InspirationImageFile),
                    "Please upload a JPG, PNG, or WEBP image.");
            }

            if (viewModel.InspirationImageFile.Length == 0)
            {
                ModelState.AddModelError(nameof(viewModel.InspirationImageFile),
                    "Please choose an image file.");
            }
            else if (viewModel.InspirationImageFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(nameof(viewModel.InspirationImageFile),
                    "The image must be smaller than 5 MB.");
            }
        }

        if (selectedService is not null && requestedStart.HasValue)
        {
            var requestedEnd = requestedStart.Value.AddMinutes(
                selectedService.DurationMinutes);

            if (viewModel.AppointmentTime!.Value < SalonHours.OpeningTime ||
                viewModel.AppointmentTime.Value >= SalonHours.ClosingTime)
            {
                ModelState.AddModelError(nameof(viewModel.AppointmentTime),
                    "Appointments are available from 08:00 to 18:00.");
            }
            else if (requestedEnd > requestedStart.Value.Date.Add(
                         SalonHours.ClosingTime.ToTimeSpan()))
            {
                ModelState.AddModelError(nameof(viewModel.AppointmentTime),
                    "This service must finish by 18:00.");
            }
            else
            {
                var confirmedAppointments = _context.Appointments
                    .Include(appointment => appointment.Service)
                    .Where(appointment =>
                        appointment.Status == AppointmentStatus.Pending ||
                        appointment.Status == AppointmentStatus.Confirmed)
                    .ToList();

                var hasConflict = confirmedAppointments.Any(appointment =>
                    requestedStart.Value < appointment.AppointmentDateTime.AddMinutes(
                        appointment.Service.DurationMinutes) &&
                    requestedEnd > appointment.AppointmentDateTime);

                if (hasConflict)
                {
                    ModelState.AddModelError(nameof(viewModel.AppointmentTime),
                        "This time overlaps with an existing appointment.");
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

        if (viewModel.InspirationImageFile is not null)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(viewModel.InspirationImageFile.FileName)}";
            var uploadsFolder = Path.Combine(
                _webHostEnvironment.WebRootPath,
                "uploads",
                "inspiration-images");

            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);

            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await viewModel.InspirationImageFile.CopyToAsync(fileStream);
            }

            var inspirationImage = new InspirationImage
            {
                AppointmentId = appointment.Id,
                ImageUrl = $"/uploads/inspiration-images/{fileName}"
            };

            _context.InspirationImages.Add(inspirationImage);
            await _context.SaveChangesAsync();
        }

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

        ViewBag.Services = services.Select(service => new SelectListItem
        {
            Value = service.Id.ToString(),
            Text = $"{service.Name} - €{service.Price:0.00} - {service.DurationMinutes} min",
            Selected = selectedServiceId == service.Id
        });

        var appointmentTimes = GetAppointmentTimes()
            .Select(time => new SelectListItem
            {
                Value = time.ToString("HH:mm"),
                Text = time.ToString("HH:mm"),
                Selected = selectedTime.HasValue &&
                           time == selectedTime.Value
            })
            .ToList();

        ViewBag.AppointmentTimes = appointmentTimes;
    }

    private static IEnumerable<TimeOnly> GetAppointmentTimes()
    {
        var time = SalonHours.OpeningTime;

        while (time < SalonHours.ClosingTime)
        {
            yield return time;
            time = time.AddMinutes(30);
        }
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
            .Include(appointment => appointment.Review)
            .Include(appointment => appointment.InspirationImage)
            .Where(appointment => appointment.CustomerId == customerId)
            .OrderByDescending(appointment => appointment.AppointmentDateTime)
            .ToList();

        return View(appointments);
    }
}
