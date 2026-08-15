using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using NailBook.Models.Enums;

namespace NailBook.ViewModels.NailDesigns;

public class CreateNailDesignViewModel
{
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public NailDesignStyle Style { get; set; }

    [Required]
    public IFormFile? ImageFile { get; set; }
}