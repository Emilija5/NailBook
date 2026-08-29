using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NailBook.Data;
using NailBook.Models;
using NailBook.ViewModels.Admin;

namespace NailBook.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = RoleNames.Admin)]
public class UsersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var customers = await _userManager.GetUsersInRoleAsync(RoleNames.Customer);
        var customerIds = customers.Select(customer => customer.Id).ToList();

        var appointmentCounts = await _context.Appointments
            .Where(appointment => customerIds.Contains(appointment.CustomerId))
            .GroupBy(appointment => appointment.CustomerId)
            .Select(group => new
            {
                CustomerId = group.Key,
                Count = group.Count()
            })
            .ToDictionaryAsync(item => item.CustomerId, item => item.Count);

        var viewModel = customers
            .OrderBy(customer => customer.FullName ?? customer.Email)
            .Select(customer => new CustomerListItemViewModel
            {
                FullName = customer.FullName ?? "Not provided",
                Email = customer.Email ?? "Not provided",
                AppointmentCount = appointmentCounts.GetValueOrDefault(customer.Id)
            })
            .ToList();

        return View(viewModel);
    }
}
