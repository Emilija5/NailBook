using NailBook.Models;

namespace NailBook.ViewModels.Admin;

public class AdminDashboardViewModel
{
    public int PendingAppointmentsCount { get; set; }
    public int CompletedAppointmentsCount { get; set; }
    public int ActiveServicesCount { get; set; }
    public List<Appointment> UpcomingAppointments { get; set; } = [];
}
