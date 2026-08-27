using System.ComponentModel.DataAnnotations;

namespace NailBook.ViewModels.Reviews;

public class CreateReviewViewModel
{
    [Required]
    public int AppointmentId { get; set; }

    [Required]
    [Range(1, 5)]
    public int? Rating { get; set; }

    [StringLength(1000)]
    public string? Comment { get; set; }
}
