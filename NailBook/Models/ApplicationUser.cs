using Microsoft.AspNetCore.Identity;

namespace NailBook.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }

    public ICollection<Review> Reviews { get; set; } = [];
}
