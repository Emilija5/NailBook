namespace NailBook.ViewModels.Admin;

public class CustomerListItemViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int AppointmentCount { get; set; }
}
