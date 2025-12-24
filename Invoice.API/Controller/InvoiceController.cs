using Invoice.Application.DTO;
using Invoice.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Invoice.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly InvoiceService _invoiceService;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(InvoiceService invoiceService, ILogger<InvoiceController> logger)
        {
            _invoiceService = invoiceService;
            _logger = logger;
        }

        [HttpGet("health")]
        public IActionResult CheckHealth()
        {
            _logger.LogInformation("Health check requested for Invoice API");
            return Ok(new { status = "healthy", service = "Invoice API", timestamp = DateTime.UtcNow });
        }

        [HttpGet]
        public string CheckHealthLegacy()
        {
            return "Invoice API is running";
        }
        [HttpPost]
        public async Task<Model.Entities.Invoice> CreateInvoice(CreateInvoiceDTO createInvoiceDTO)
        {
            return await _invoiceService.CreateInvoiceAsync(createInvoiceDTO);
        }

    }
}
