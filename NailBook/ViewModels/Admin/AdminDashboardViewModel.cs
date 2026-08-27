using NailBook.Models;

namespace NailBook.ViewModels.Admin;

public class AdminDashboardViewModel
{
    public int PendingAppointmentsCount { get; set; }

    public List<Appointment> UpcomingAppointments { get; set; } = [];
}