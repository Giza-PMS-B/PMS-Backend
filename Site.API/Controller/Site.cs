using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Site.Application.DTO;
using Site.Application.Services;
using System.ComponentModel.DataAnnotations;

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
        public IActionResult CheckHealth()
        {
            return Ok("Site API is running");
        }

        [HttpPost("add/parent")]
        public async Task<IActionResult> AddParentSite([FromBody] CreateSiteDTO createSiteDTO)
        {
            try
            {
                var createdSite = await _siteService.CreateParentSiteAsync(createSiteDTO);
                return Ok(new { message = "Parent site created successfully.", data = createdSite });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "An error occurred while creating the parent site.", details = ex.Message });
            }
        }

        [HttpPost("add/leaf")]
        public async Task<IActionResult> AddLeafSite([FromBody] CreateLeafSiteDTO createLeafSiteDTO)
        {
            try
            {
                var createdSite = await _siteService.CreateLeafSiteAsync(createLeafSiteDTO);
                return Ok(new { message = "Leaf site created successfully.", data = createdSite });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "An error occurred while creating the leaf site.", details = ex.Message });
            }
        }

        [HttpGet("roots")]
        public async Task<IActionResult> GetRootSites()
        {
            try
            {
                var sites = await _siteService.GetRootSites();
                return Ok(sites);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "An error occurred while retrieving root sites.", details = ex.Message });
            }
        }

        [HttpGet("leaves")]
        public async Task<IActionResult> GetLeafSites()
        {
            try
            {
                var sites = await _siteService.GetLeafSites();
                return Ok(sites);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "An error occurred while retrieving leaf sites.", details = ex.Message });
            }
        }

        [HttpGet("children/{parentId}")]
        public async Task<IActionResult> GetChildSites([FromRoute] Guid parentId)
        {
            try
            {
                if (parentId == Guid.Empty)
                {
                    return BadRequest(new { error = "Invalid parent ID." });
                }

                var sites = await _siteService.GetAllChildSitesOf(parentId);
                return Ok(sites);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "An error occurred while retrieving child sites.", details = ex.Message });
            }
        }
    }
}