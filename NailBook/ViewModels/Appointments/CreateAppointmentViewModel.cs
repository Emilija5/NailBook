using System.ComponentModel.DataAnnotations;

namespace NailBook.ViewModels.Appointments;

public class CreateAppointmentViewModel
{
    [Required(ErrorMessage = "Please choose a service.")]
    public int? ServiceId { get; set; }

    [Required(ErrorMessage = "Please choose a date and time.")]
    [DataType(DataType.DateTime)]
    public DateTime? AppointmentDateTime { get; set; }

    [StringLength(500)]
    public string? CustomerNote { get; set; }
}