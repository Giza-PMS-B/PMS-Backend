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
        public SiteController(SiteService siteService)
        {
            _siteService = siteService;
        }
        [HttpGet]
        public string Get()
        {
            return "Site API is running";
        }

        [HttpPost]
        public async Task<IActionResult> AddSite(CreateSiteDTO createSiteDTO)
        {
            await _siteService.CreateSiteAsync(createSiteDTO);
            return Ok("Site created successfully.");
        }
    }
}
