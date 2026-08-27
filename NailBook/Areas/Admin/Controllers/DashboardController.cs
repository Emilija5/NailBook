using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NailBook.Data;
using NailBook.Models;
using NailBook.Models.Enums;
using NailBook.ViewModels.Admin;

namespace NailBook.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = RoleNames.Admin)]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var dashboard = new AdminDashboardViewModel
        {
            PendingAppointmentsCount = _context.Appointments.Count(
                appointment => appointment.Status == AppointmentStatus.Pending),
            CompletedAppointmentsCount = _context.Appointments.Count(
                appointment => appointment.Status == AppointmentStatus.Completed),
            ActiveServicesCount = _context.Services.Count(
                service => service.IsActive),
            UpcomingAppointments = _context.Appointments
                .Include(appointment => appointment.Customer)
                .Include(appointment => appointment.Service)
                .Where(appointment =>
                    appointment.Status != AppointmentStatus.Cancelled &&
                    appointment.Status != AppointmentStatus.Completed &&
                    appointment.AppointmentDateTime >= DateTime.Now)
                .OrderBy(appointment => appointment.AppointmentDateTime)
                .Take(5)
                .ToList()
        };

        return View(dashboard);
    }
}
