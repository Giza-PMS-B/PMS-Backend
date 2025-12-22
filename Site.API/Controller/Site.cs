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

        /// <summary>
        /// Health check endpoint to verify Site API is running
        /// </summary>
        /// <returns>Status message</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult CheckHealth()
        {
            return Ok("Site API is running");
        }

        /// <summary>
        /// Add a new parent site (Main path for parking area)
        /// </summary>
        /// <param name="createSiteDTO">Parent site details</param>
        /// <returns>Created site details</returns>
        [HttpPost("add/parent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Add a new leaf site (Available parking area slots as child below the parent site)
        /// </summary>
        /// <param name="createLeafSiteDTO">Leaf site details including polygons</param>
        /// <returns>Created leaf site details</returns>
        [HttpPost("add/leaf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Get all root-level sites (sites without parents)
        /// </summary>
        /// <returns>List of root sites</returns>
        [HttpGet("roots")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Get all leaf sites (final level parking areas)
        /// </summary>
        /// <returns>List of leaf sites</returns>
        [HttpGet("leaves")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Get all child sites of a specific parent site
        /// </summary>
        /// <param name="parentId">Parent site GUID</param>
        /// <returns>List of child sites</returns>
        [HttpGet("children/{parentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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