using NailBook.Models;

namespace NailBook.ViewModels.Admin;

public class AdminDashboardViewModel
{
    public int TotalAppointmentsCount { get; set; }
    public int PendingAppointmentsCount { get; set; }
    public int ConfirmedAppointmentsCount { get; set; }
    public int CompletedAppointmentsCount { get; set; }
    public int ActiveServicesCount { get; set; }
    public List<Appointment> UpcomingAppointments { get; set; } = [];
}
