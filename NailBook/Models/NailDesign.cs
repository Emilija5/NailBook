using System.ComponentModel.DataAnnotations;
using NailBook.Models.Enums;

namespace NailBook.Models;

public class NailDesign
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(300)]
    public string ImageUrl { get; set; } = string.Empty;

    public NailDesignStyle Style { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}