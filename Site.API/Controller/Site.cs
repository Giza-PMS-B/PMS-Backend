using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> AddSite()
        {
            try
            {
                var site = await _siteService.CreateSiteAsync();
                return Ok(new { message = "Site created successfully.", siteId = site.Id, nameEn = site.NameEn });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to create site.", error = ex.Message });
            }
        }

    }
}
