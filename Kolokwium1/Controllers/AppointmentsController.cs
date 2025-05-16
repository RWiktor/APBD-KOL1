using Kolokwium1.Exceptions;
using Kolokwium1.Models.DTOs;
using Kolokwium1.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kolokwium1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IDbService _dbService;
        
        public AppointmentsController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointment(int id)
        {
            try
            {
                var appointment = await _dbService.GetAppointmentByIdAsync(id);
                return Ok(appointment);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddAppointment(CreateAppointmentRequestDto createAppointmentRequest)
        {
            if (!createAppointmentRequest.Services.Any())
            {
                return BadRequest("at least one service is required");
            }

            try
            {
                await _dbService.AddNewAppointmentAsync(createAppointmentRequest);
            }
            catch (ConflictException e)
            {
                return Conflict(e.Message);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            
            return CreatedAtAction(nameof(GetAppointment), new { id = createAppointmentRequest.AppointmentId }, createAppointmentRequest);
        }    
    }
}