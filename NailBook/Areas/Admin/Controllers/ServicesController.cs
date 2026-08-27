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
    
    [HttpGet]
    public IActionResult Create()
    {
        return View(new Service());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Service service)
    {
        if (!ModelState.IsValid)
        {
            return View(service);
        }

        _context.Services.Add(service);
        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }
    
    [HttpGet]
    public IActionResult Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var service = _context.Services.Find(id);

        if (service is null)
        {
            return NotFound();
        }

        return View(service);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Service service)
    {
        if (id != service.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(service);
        }

        _context.Services.Update(service);
        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleActive(int id)
    {
        var service = _context.Services.Find(id);

        if (service is null)
        {
            return NotFound();
        }

        service.IsActive = !service.IsActive;
        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }
}