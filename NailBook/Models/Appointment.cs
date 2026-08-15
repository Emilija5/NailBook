using System.ComponentModel.DataAnnotations;
using NailBook.Models.Enums;

namespace NailBook.Models;

public class Appointment
{
    public int Id { get; set; }

    [Required]
    public string CustomerId { get; set; } = string.Empty;

    public ApplicationUser Customer { get; set; } = null!;

    [Required]
    public int ServiceId { get; set; }

    public Service Service { get; set; } = null!;

    [Required]
    public DateTime AppointmentDateTime { get; set; }

    [StringLength(500)]
    public string? CustomerNote { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}