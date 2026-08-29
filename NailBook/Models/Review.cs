using System.ComponentModel.DataAnnotations;

namespace NailBook.Models;

public class Review
{
    public int Id { get; set; }

    [Required]
    public string CustomerId { get; set; } = string.Empty;

    public ApplicationUser Customer { get; set; } = null!;

    [Required]
    public int AppointmentId { get; set; }

    public Appointment Appointment { get; set; } = null!;

    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(1000)]
    public string? Comment { get; set; }

    public bool IsVisible { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
