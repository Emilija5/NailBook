using Microsoft.AspNetCore.Mvc;
using NailBook.Data;

namespace NailBook.Controllers;

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
            .Where(service => service.IsActive)
            .OrderBy(service => service.Name)
            .ToList();

        return View(services);
    }
}