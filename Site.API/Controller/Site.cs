using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Site.Application.DTO;
using Site.Application.Services;

namespace Site.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class SiteController : ControllerBase
    {
        private readonly SiteService _siteService;
        private readonly ILogger<SiteController> _logger;

        public SiteController(SiteService siteService, ILogger<SiteController> logger)
        {
            _siteService = siteService;
            _logger = logger;
        }

        [HttpGet("health")]
        public IActionResult CheckHealth()
        {
            _logger.LogInformation("Health check requested for Site API");
            return Ok(new { status = "healthy", service = "Site API", timestamp = DateTime.UtcNow });
        }

        [HttpGet]
        public string CheckHealthLegacy()
        {
            return "Site API is running";
        }

        [HttpPost("add/parent")]
        public async Task<IActionResult> AddParentSite(CreateSiteDTO createSiteDTO)
        {
            _logger.LogInformation("Creating parent site {SiteName}", createSiteDTO.NameEn);
            try
            {
                var site = await _siteService.CreateParentSiteAsync(createSiteDTO);
                _logger.LogInformation("Successfully created parent site {SiteId}", site.Id);
                return Ok("Site created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create parent site {SiteName}", createSiteDTO.NameEn);
                throw;
            }
        }

        [HttpPost("add/leaf")]
        public async Task<IActionResult> AddLeafSite(CreateLeafSiteDTO createLeafSiteDTO)
        {
            _logger.LogInformation("Creating leaf site {SiteName}", createLeafSiteDTO.NameEn);
            try
            {
                var site = await _siteService.CreateLeafSiteAsync(createLeafSiteDTO);
                _logger.LogInformation("Successfully created leaf site {SiteId}", site.Id);
                return Ok("Site created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create leaf site {SiteName}", createLeafSiteDTO.NameEn);
                throw;
            }
        }

        [HttpGet("roots")]
        public async Task<List<Model.Entities.Site>> GetRootSites()
        {
            _logger.LogInformation("Retrieving root sites");
            try
            {
                var sites = await _siteService.GetRootSites();
                _logger.LogInformation("Retrieved {Count} root sites", sites.Count);
                return sites;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve root sites");
                throw;
            }
        }
    }
}
