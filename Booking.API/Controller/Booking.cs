using Booking.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Booking.Application.DTO;

namespace Booking.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly TicketService _ticketService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(TicketService ticketService, ILogger<BookingController> logger)
        {
            _ticketService = ticketService;
            _logger = logger;
        }

        [HttpGet("health")]
        public IActionResult CheckHealth()
        {
            _logger.LogInformation("Health check requested for Booking API");
            return Ok(new { status = "healthy", service = "Booking API", timestamp = DateTime.UtcNow });
        }

        [HttpGet]
        public string CheckHealthLegacy()
        {
            return "Booking API is running";
        }

        [HttpPost()]
        public async Task<IActionResult> CreateTicket(CreateTicketDTO createTicketDTO)
        {
            _logger.LogInformation("Creating ticket for site {SiteId}", createTicketDTO.SiteId);
            try
            {
                var ticket = await _ticketService.CreateTicketAsync(createTicketDTO);
                _logger.LogInformation("Successfully created ticket {TicketId}", ticket.Id);
                return Ok(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create ticket for site {SiteId}", createTicketDTO.SiteId);
                throw;
            }
        }
    }
}
