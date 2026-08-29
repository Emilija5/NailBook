using System.ComponentModel.DataAnnotations;

namespace NailBook.ViewModels.Profile;

public class EditProfileViewModel
{
    [Required(ErrorMessage = "Please enter your full name.")]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    [StringLength(30)]
    public string? PhoneNumber { get; set; }

    public string Email { get; set; } = string.Empty;
}
