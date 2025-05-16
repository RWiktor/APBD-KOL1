using System.Data.Common;
using System.Data.SqlClient;
using Kolokwium1.Exceptions;
using Kolokwium1.Models.DTOs;

namespace Kolokwium1.Services;

public class DbService : IDbService
{
    private readonly string _connectionString;
    
    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? string.Empty;
    }
    
    public async Task<AppointmentDetailsDto> GetAppointmentByIdAsync(int appointmentId)
    {
        var query = @"
            SELECT a.date, p.first_name, p.last_name, p.date_of_birth, d.doctor_id, d.PWZ, s.name, as_s.service_fee
            FROM Appointment a
            JOIN Patient p ON a.patient_id = p.patient_id
            JOIN Doctor d ON a.doctor_id = d.doctor_id
            JOIN Appointment_Service as_s ON a.appoitment_id = as_s.appoitment_id
            JOIN Service s ON as_s.service_id = s.service_id
            WHERE a.appoitment_id = @appointmentId;";
        
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        command.CommandText = query;
        await connection.OpenAsync();
        
        command.Parameters.AddWithValue("@appointmentId", appointmentId);
        var reader = await command.ExecuteReaderAsync();
        
        AppointmentDetailsDto? appointment = null;
        
        while (await reader.ReadAsync())
        {
            if (appointment is null)
            {
                appointment = new AppointmentDetailsDto
                {
                    Date = reader.GetDateTime(0),
                    Patient = new PatientDto
                    {
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        DateOfBirth = reader.GetDateTime(3)
                    },
                    Doctor = new DoctorDto
                    {
                        DoctorId = reader.GetInt32(4),
                        PWZ = reader.GetString(5)
                    },
                    AppointmentServices = new List<AppointmentServiceDto>()
                };
            }
            
            appointment.AppointmentServices.Add(new AppointmentServiceDto
            {
                Name = reader.GetString(6),
                ServiceFee = reader.GetDecimal(7)
            });
        }       
        
        if (appointment is null)
        {
            throw new NotFoundException("no appointment found for this ID");
        }
        
        return appointment;
    }

    Task IDbService.AddNewAppointmentAsync(CreateAppointmentRequestDto appointmentRequest)
    {
        return AddNewAppointmentAsync(appointmentRequest);
    }

    public async Task AddNewAppointmentAsync(CreateAppointmentRequestDto appointmentRequest)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM Appointment WHERE appoitment_id = @AppointmentId;";
            command.Parameters.AddWithValue("@AppointmentId", appointmentRequest.AppointmentId);
                
            var appointmentIdRes = await command.ExecuteScalarAsync();
            if(appointmentIdRes is not null)
                throw new ConflictException($"Appointment with ID - {appointmentRequest.AppointmentId} already exists");
            
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM Patient WHERE patient_id = @PatientId;";
            command.Parameters.AddWithValue("@PatientId", appointmentRequest.PatientId);
                
            var patientIdRes = await command.ExecuteScalarAsync();
            if(patientIdRes is null)
                throw new NotFoundException($"Patient with ID - {appointmentRequest.PatientId} not found");
            
            command.Parameters.Clear();
            command.CommandText = "SELECT doctor_id FROM Doctor WHERE PWZ = @PWZ;";
            command.Parameters.AddWithValue("@PWZ", appointmentRequest.PWZ);
                
            var doctorId = await command.ExecuteScalarAsync();
            if(doctorId is null)
                throw new NotFoundException($"Doctor with PWZ - {appointmentRequest.PWZ} not found");
            
            command.Parameters.Clear();
            command.CommandText = 
                @"INSERT INTO Appointment
                VALUES(@AppointmentId, @PatientId, @DoctorId, @Date);";
        
            command.Parameters.AddWithValue("@AppointmentId", appointmentRequest.AppointmentId);
            command.Parameters.AddWithValue("@PatientId", appointmentRequest.PatientId);
            command.Parameters.AddWithValue("@DoctorId", doctorId);
            command.Parameters.AddWithValue("@Date", DateTime.Now);

            await command.ExecuteNonQueryAsync();
            
            foreach (var service in appointmentRequest.Services)
            {
                command.Parameters.Clear();
                command.CommandText = "SELECT service_id FROM Service WHERE name = @ServiceName;";
                command.Parameters.AddWithValue("@ServiceName", service.ServiceName);
                
                var serviceId = await command.ExecuteScalarAsync();
                if(serviceId is null)
                    throw new NotFoundException($"Service - {service.ServiceName} - not found.");
                
                command.Parameters.Clear();
                command.CommandText = 
                    @"INSERT INTO Appointment_Service
                        VALUES(@AppointmentId, @ServiceId, @ServiceFee);";
        
                command.Parameters.AddWithValue("@AppointmentId", appointmentRequest.AppointmentId);
                command.Parameters.AddWithValue("@ServiceId", serviceId);
                command.Parameters.AddWithValue("@ServiceFee", service.ServiceFee);
                
                await command.ExecuteNonQueryAsync();
            }
            
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    Task<AppointmentDetailsDto> IDbService.GetAppointmentByIdAsync(int appointmentId)
    {
        return GetAppointmentByIdAsync(appointmentId);
    }
}