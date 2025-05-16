using Kolokwium1.Models.DTOs;

namespace Kolokwium1.Services;

public interface IDbService
{
    Task<AppointmentDetailsDto> GetAppointmentByIdAsync(int appointmentId);
    Task AddNewAppointmentAsync(CreateAppointmentRequestDto appointmentRequest);
}