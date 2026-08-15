using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NailBook.Data;
using NailBook.Models;

namespace NailBook.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = RoleNames.Admin)]
public class ServicesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ServicesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var services = _context.Services
            .OrderBy(service => service.Name)
            .ToList();

        return View(services);
    }
}