using System;
using System.ComponentModel.DataAnnotations;
using System.Security;
using SharedKernel.Infrastructure.Persistent.Abstraction;
using Site.Application.DTO;
using Site.Model.Entities;

namespace Site.Application.Services;

public class SiteService
{
    private readonly IRepo<Model.Entities.Site> _siteRepository;
    private readonly IUOW _uow;

    public SiteService(IRepo<Model.Entities.Site> siteRepository, IUOW uow)
    {
        _siteRepository = siteRepository;
        _uow = uow;
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

    public async Task<Model.Entities.Site> CreateSiteAsync(CreateSiteDTO dto)
    {
        ValidateSite(dto);

        var site = CreateBasicSite(dto);

        if (dto.IsLeaf)
        {
            AddPolygons(dto, site);
        }
        else
        {
            // Parent sites must not have leaf-only values
            site.PricePerHour = null;
            site.NumberOfSolts = null;
        }

        await _siteRepository.AddAsync(site);
        await _uow.SaveChangesAsync();
        return site;
    }

    // ----------------- Helpers -----------------

    private static void ValidateSite(CreateSiteDTO dto)
    {
        if (dto.IsLeaf)
        {
            if (dto.PricePerHour is null)
                throw new ValidationException("PricePerHour is required for leaf sites.");

            if (dto.NumberOfSolts is null)
                throw new ValidationException("NumberOfSolts is required for leaf sites.");

            if (dto.Polygons == null || !dto.Polygons.Any())
                throw new ValidationException("Leaf site must have at least one polygon.");

            foreach (var polygon in dto.Polygons)
            {
                if (polygon.Points == null || polygon.Points.Count < 3)
                    throw new ValidationException("Each polygon must have at least 3 points.");
            }
        }
        else
        {
            if (dto.Polygons != null && dto.Polygons.Any())
                throw new ValidationException("Parent site cannot have polygons.");
        }
    }

    private static Model.Entities.Site CreateBasicSite(CreateSiteDTO dto)
    {
        return new Model.Entities.Site
        {
            Id = Guid.NewGuid(),
            Path = dto.Path,
            NameEn = dto.NameEn,
            NameAr = dto.NameAr,
            IntegrationCode = dto.IntegrationCode,
            IsLeaf = dto.IsLeaf,
            ParentId = dto.ParentId,

            // Set only for leaf (may be null otherwise)
            PricePerHour = dto.IsLeaf ? dto.PricePerHour : null,
            NumberOfSolts = dto.IsLeaf ? dto.NumberOfSolts : null
        };
    }

    private static void AddPolygons(CreateSiteDTO dto, Model.Entities.Site site)
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
