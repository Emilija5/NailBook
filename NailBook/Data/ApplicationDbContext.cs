using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NailBook.Models;

namespace NailBook.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Service> Services { get; set; }
    public DbSet<NailDesign> NailDesigns { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Review>()
            .HasOne(review => review.Appointment)
            .WithOne(appointment => appointment.Review)
            .HasForeignKey<Review>(review => review.AppointmentId);

        builder.Entity<Review>()
            .HasOne(review => review.Customer)
            .WithMany(customer => customer.Reviews)
            .HasForeignKey(review => review.CustomerId);
    }
}
