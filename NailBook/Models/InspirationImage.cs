using System.ComponentModel.DataAnnotations;

namespace NailBook.Models;

public class InspirationImage
{
    public int Id { get; set; }

    [Required]
    public int AppointmentId { get; set; }

    public Appointment Appointment { get; set; } = null!;

    [Required]
    [StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
