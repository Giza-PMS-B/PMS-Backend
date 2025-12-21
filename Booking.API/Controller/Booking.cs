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
        public BookingController(TicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        public string CheckHealth()
        {
            return "Booking API is running";
        }

        [HttpPost()]
        public async Task<IActionResult> CreateTicket(CreateTicketDTO createTicketDTO)
        {
            var ticket=await _ticketService.CreateTicketAsync(createTicketDTO);
            return Ok(ticket);
        }
    }
}
