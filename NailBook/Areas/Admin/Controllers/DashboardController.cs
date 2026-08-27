using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NailBook.Data;
using NailBook.Models;
using NailBook.Models.Enums;

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
        var pendingAppointmentsCount = _context.Appointments.Count(
            appointment => appointment.Status == AppointmentStatus.Pending);

        ViewBag.PendingAppointmentsCount = pendingAppointmentsCount;

        return View();
    }
}