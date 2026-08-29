using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NailBook.Models;
using NailBook.ViewModels.Profile;

namespace NailBook.Controllers;

[Authorize(Roles = RoleNames.Customer)]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        var viewModel = new EditProfileViewModel
        {
            FullName = user.FullName ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email ?? string.Empty
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(EditProfileViewModel viewModel)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge();
        }

        viewModel.Email = user.Email ?? string.Empty;

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        user.FullName = viewModel.FullName;
        user.PhoneNumber = viewModel.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(viewModel);
        }

        TempData["SuccessMessage"] = "Your profile was updated.";

        return RedirectToAction(nameof(Index));
    }
}
