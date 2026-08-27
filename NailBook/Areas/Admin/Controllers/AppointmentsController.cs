using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NailBook.Data;
using NailBook.Models;
using NailBook.Models.Enums;

namespace NailBook.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = RoleNames.Admin)]
public class AppointmentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AppointmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index(AppointmentStatus? status)
    {
        IQueryable<Appointment> appointments = _context.Appointments
            .Include(appointment => appointment.Customer)
            .Include(appointment => appointment.Service);

        if (status.HasValue)
        {
            appointments = appointments.Where(
                appointment => appointment.Status == status.Value);
        }

        var pendingCount = _context.Appointments.Count(
            appointment => appointment.Status == AppointmentStatus.Pending);

        var confirmedCount = _context.Appointments.Count(
            appointment => appointment.Status == AppointmentStatus.Confirmed);

        var cancelledCount = _context.Appointments.Count(
            appointment => appointment.Status == AppointmentStatus.Cancelled);

        ViewBag.Statuses = new List<SelectListItem>
        {
            new()
            {
                Value = AppointmentStatus.Pending.ToString(),
                Text = $"Pending ({pendingCount})",
                Selected = status == AppointmentStatus.Pending
            },
            new()
            {
                Value = AppointmentStatus.Confirmed.ToString(),
                Text = $"Confirmed ({confirmedCount})",
                Selected = status == AppointmentStatus.Confirmed
            },
            new()
            {
                Value = AppointmentStatus.Cancelled.ToString(),
                Text = $"Cancelled ({cancelledCount})",
                Selected = status == AppointmentStatus.Cancelled
            }
        };

        return View(appointments
            .OrderBy(appointment => appointment.AppointmentDateTime)
            .ToList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int id, AppointmentStatus? status)
    {
        var appointment = _context.Appointments.Find(id);

        if (appointment is null)
        {
            return NotFound();
        }

        if (appointment.Status == AppointmentStatus.Pending)
        {
            appointment.Status = AppointmentStatus.Confirmed;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index), new { status });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, AppointmentStatus? status)
    {
        var appointment = _context.Appointments.Find(id);

        if (appointment is null)
        {
            return NotFound();
        }

        if (appointment.Status != AppointmentStatus.Cancelled)
        {
            appointment.Status = AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index), new { status });
    }
}
