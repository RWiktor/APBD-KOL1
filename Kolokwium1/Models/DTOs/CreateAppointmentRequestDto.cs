namespace Kolokwium1.Models.DTOs;

public class CreateAppointmentRequestDto
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string PWZ { get; set; } = string.Empty;
    public List<ServiceRequestDto> Services { get; set; } = [];
}

public class ServiceRequestDto
{
    public string ServiceName { get; set; } = string.Empty;
    public decimal ServiceFee { get; set; }
}