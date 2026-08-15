using System.ComponentModel.DataAnnotations;

namespace NailBook.Models;

public class Service
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Range(0.01, 1000)]
    public decimal Price { get; set; }
    
    [Range(1, 480)]
    public int DurationMinutes { get; set; }

    public bool IsActive { get; set; } = true;
}