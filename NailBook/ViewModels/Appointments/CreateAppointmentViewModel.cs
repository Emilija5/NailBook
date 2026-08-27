using System.ComponentModel.DataAnnotations;

namespace NailBook.ViewModels.Appointments;

public class CreateAppointmentViewModel
{
    [Required(ErrorMessage = "Please choose a service.")]
    public int? ServiceId { get; set; }

    [Required(ErrorMessage = "Please choose a date.")]
    [DataType(DataType.Date)]
    public DateOnly? AppointmentDate { get; set; }

    [Required(ErrorMessage = "Please choose an appointment time.")]
    [DataType(DataType.Time)]
    public TimeOnly? AppointmentTime { get; set; }

    [StringLength(500)]
    public string? CustomerNote { get; set; }

    public IFormFile? InspirationImageFile { get; set; }
}
