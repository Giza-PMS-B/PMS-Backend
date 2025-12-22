using System;
using System.ComponentModel.DataAnnotations;
using System.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.EventDriven.Abstraction;
using SharedKernel.Infrastructure.Persistent.Abstraction;
using SharedKernel.MessageBus.Abstraction;
using Site.Application.DTO;
using Site.Model.Entities;
using Site.Model.Shared.Events;

namespace Site.Application.Services;

public class SiteService
{
    private readonly IRepo<Model.Entities.Site> _siteRepository;
    private readonly IUOW _uow;
    private readonly IIntegrationEventProducer _eventProducer;
    private readonly ILogger<SiteService> _logger;

    public SiteService(IRepo<Model.Entities.Site> siteRepository, IUOW uow, IIntegrationEventProducer eventProducer, ILogger<SiteService> logger)
    {
        _siteRepository = siteRepository;
        _uow = uow;
        _eventProducer = eventProducer;
        _logger = logger;
    }

    public async Task<List<SiteResponseDTO>> GetAllChildSitesOf(Guid parentId)
    {
        var sites = _siteRepository.GetAll()
            .Include(s => s.Polygons)
                .ThenInclude(p => p.PolygonPoints)
            .Where(s => s.ParentId == parentId)
            .ToList();
        return sites.Select(s => MapToResponseDTO(s)).ToList();
    }
    public async Task<List<SiteResponseDTO>> GetLeafSites()
    {
        var sites = _siteRepository.GetAll()
            .Include(s => s.Polygons)
                .ThenInclude(p => p.PolygonPoints)
            .Where(s => s.IsLeaf == true)
            .ToList();
        return sites.Select(s => MapToResponseDTO(s)).ToList();
    }
    public async Task<List<SiteResponseDTO>> GetRootSites()
    {
        var sites = _siteRepository.GetAll()
            .Include(s => s.Children)
                .ThenInclude(c => c.Polygons)
                    .ThenInclude(p => p.PolygonPoints)
            .Include(s => s.Polygons)
                .ThenInclude(p => p.PolygonPoints)
            .Where(s => s.ParentId == null)
            .ToList();
        return sites.Select(s => MapToResponseDTO(s)).ToList();
    }

    public async Task<SiteResponseDTO> CreateParentSiteAsync(CreateSiteDTO dto)
    {

        ValidateSiteNameUniqueness(dto.NameEn, dto.NameAr);

        _logger.LogInformation("start Creating parent site {SiteName} at path {Path}", dto.NameEn, dto.Path);

        var parentSite = CreateParentSite(dto);
        await _siteRepository.AddAsync(parentSite);

        var siteCreatedEvent = new SiteCreatedEvent
        {
            SiteId = parentSite.Id,
            NameEn = parentSite.NameEn,
            NameAr = parentSite.NameAr,
            Path = parentSite.Path,
            IsLeaf = parentSite.IsLeaf,
        };

        _eventProducer.Enqueue(siteCreatedEvent);
        _logger.LogInformation("Enqueued SiteCreatedEvent for site {SiteId}", parentSite.Id);

        await _uow.SaveChangesAsync(1);

        _logger.LogInformation("Successfully created parent site {SiteId}", parentSite.Id);
        return MapToResponseDTO(parentSite);

    }
    private static Model.Entities.Site CreateParentSite(CreateSiteDTO dto)
    {
        return new Model.Entities.Site
        {
            Id = Guid.NewGuid(),
            Path = dto.Path,
            NameEn = dto.NameEn,
            NameAr = dto.NameAr,
            IsLeaf = false,
            ParentId = dto.ParentId,
        };
    }

    public async Task<SiteResponseDTO> CreateLeafSiteAsync(CreateLeafSiteDTO dto)
    {
        ValidateSiteNameUniqueness(dto.NameEn, dto.NameAr);
        ValidateIntegrationCodeUniqueness(dto.IntegrationCode);


        _logger.LogInformation("Creating leaf site {SiteName} with {PolygonCount} polygons", dto.NameEn, dto.Polygons?.Count ?? 0);


        var leafSite = CreateLeafSite(dto);
        await _siteRepository.AddAsync(leafSite);

        var siteCreatedEvent = new SiteCreatedEvent
        {
            SiteId = leafSite.Id,
            NameEn = leafSite.NameEn,
            NameAr = leafSite.NameAr,
            Path = leafSite.Path,
            IsLeaf = leafSite.IsLeaf,
            PricePerHour = leafSite.PricePerHour,
            IntegrationCode = leafSite.IntegrationCode,
            NumberOfSolts = leafSite.NumberOfSolts,
        };

        _eventProducer.Enqueue(siteCreatedEvent);
        _logger.LogInformation("Enqueued SiteCreatedEvent for leaf site {SiteId}", leafSite.Id);

        await _uow.SaveChangesAsync(1);

        _logger.LogInformation("Successfully created leaf site {SiteId}", leafSite.Id);

        return MapToResponseDTO(leafSite);
    }

    private Model.Entities.Site CreateLeafSite(CreateLeafSiteDTO dto)
    {
        ValidateLeafSite(dto);

        var leafSite = new Model.Entities.Site
        {
            Id = Guid.NewGuid(),
            Path = dto.Path,
            NameEn = dto.NameEn,
            NameAr = dto.NameAr,
            IsLeaf = true,
            PricePerHour = dto.PricePerHour,
            IntegrationCode = dto.IntegrationCode,
            NumberOfSolts = dto.NumberOfSolts,
            ParentId = dto.ParentId,
        };

        AddPolygons(dto, leafSite);
        return leafSite;
    }

    private void ValidateSiteNameUniqueness(string nameEn, string nameAr)
    {
        var existingByNameEn = _siteRepository.GetAll()
            .FirstOrDefault(s => s.NameEn.ToLower() == nameEn.ToLower());

        if (existingByNameEn != null)
            throw new ValidationException("The site name is already existed");

        var existingByNameAr = _siteRepository.GetAll()
            .FirstOrDefault(s => s.NameAr.ToLower() == nameAr.ToLower());

        if (existingByNameAr != null)
            throw new ValidationException("The site name is already existed");
    }

    private void ValidateIntegrationCodeUniqueness(string integrationCode)
    {
        var existing = _siteRepository.GetAll()
            .FirstOrDefault(s => s.IntegrationCode != null &&
                                 s.IntegrationCode.ToLower() == integrationCode.ToLower());

        if (existing != null)
            throw new ValidationException("Integration Code already exists");
    }

    private static void ValidateLeafSite(CreateLeafSiteDTO dto)
    {
        if (dto.Polygons == null || !dto.Polygons.Any())
            throw new ValidationException("Leaf site must have at least one polygon.");

        foreach (var polygon in dto.Polygons)
        {
            if (polygon.Points == null || polygon.Points.Count < 3)
                throw new ValidationException("Each polygon must have at least 3 points.");
        }
    }


    private static void AddPolygons(CreateLeafSiteDTO dto, Model.Entities.Site site)
    {
        foreach (var polygonDto in dto.Polygons)
        {
            var polygon = new Polygon
            {
                Id = Guid.NewGuid(),
                Name = polygonDto.Name,
                Site = site
            };

            foreach (var pointDto in polygonDto.Points)
            {
                polygon.PolygonPoints.Add(new PolygonPoint
                {
                    PolygonId = polygon.Id,
                    Latitude = pointDto.Latitude,
                    Longitude = pointDto.Longitude
                });
            }

            site.Polygons.Add(polygon);
        }
    }


    private static SiteResponseDTO MapToResponseDTO(Model.Entities.Site site)
    {
        var dto = new SiteResponseDTO
        {
            Id = site.Id,
            Path = site.Path,
            NameEn = site.NameEn,
            NameAr = site.NameAr,
            PricePerHour = site.PricePerHour,
            IntegrationCode = site.IntegrationCode,
            NumberOfSolts = site.NumberOfSolts,
            IsLeaf = site.IsLeaf,
            ParentId = site.ParentId,
            Polygons = site.Polygons?.Select(p => new PolygonResponseDTO
            {
                Id = p.Id,
                Name = p.Name,
                PolygonPoints = p.PolygonPoints?.Select(pp => new PolygonPointResponseDTO
                {
                    Latitude = pp.Latitude,
                    Longitude = pp.Longitude
                }).ToList() ?? new List<PolygonPointResponseDTO>()
            }).ToList() ?? new List<PolygonResponseDTO>()
        };

        return dto;
    }
}