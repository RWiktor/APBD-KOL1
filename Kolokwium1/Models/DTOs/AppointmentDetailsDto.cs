namespace Kolokwium1.Models.DTOs;

public class AppointmentDetailsDto
{
    public DateTime Date { get; set; }
    public PatientDto Patient { get; set; } = null!;
    public DoctorDto Doctor { get; set; } = null!;
    public List<AppointmentServiceDto> AppointmentServices { get; set; } = [];
}

public class PatientDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

public class DoctorDto
{
    public int DoctorId { get; set; }
    public string PWZ { get; set; } = string.Empty;
}

public class AppointmentServiceDto
{
    public string Name { get; set; } = string.Empty;
    public decimal ServiceFee { get; set; }
}