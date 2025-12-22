using System;
using System.ComponentModel.DataAnnotations;
using System.Security;
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

    public async Task<List<Model.Entities.Site>> GetAllChildSitesOf(Guid parentId)
    {
        return _siteRepository.GetAll().Where(s => s.ParentId == parentId).ToList();
    }
    public async Task<List<Model.Entities.Site>> GetLeafSites()
    {
        return _siteRepository.GetAll().Where(s => s.IsLeaf == true).ToList();
    }
    public async Task<List<Model.Entities.Site>> GetRootSites()
    {
        return _siteRepository.GetAll().Where(s => s.ParentId == null).ToList();
    }

    public async Task<Model.Entities.Site> CreateParentSiteAsync(CreateSiteDTO dto)
    {
        _logger.LogInformation("Creating parent site {SiteName} at path {Path}", dto.NameEn, dto.Path);

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

        return parentSite;
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

    public async Task<Model.Entities.Site> CreateLeafSiteAsync(CreateLeafSiteDTO dto)
    {
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

        return leafSite;
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

}