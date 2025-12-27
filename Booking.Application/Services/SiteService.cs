using System;
using Booking.Application.DTO;
using Booking.Model.Entities;
using SharedKernel.Infrastructure.Persistent.Abstraction;

namespace Booking.Application.Services;

public class SiteService
{
    private readonly IRepo<Model.Entities.Site> _siteRepository;
    private readonly IUOW _uow;

    public SiteService(IRepo<Model.Entities.Site> siteRepository, IUOW uow)
    {
        _siteRepository = siteRepository;
        _uow = uow;
    }
    public async Task<Model.Entities.Site> CreateSiteAsync(CreateSiteDTO createSiteDTO)
    {
        throw new NotImplementedException();
    }
    public async Task<List<SiteResponseDTO>> GetLeafSites()
    {
        var sites = _siteRepository.GetAll()
            .Where(s => s.IsLeaf == true)
            .ToList();
        return sites.Select(s => MapToResponseDTO(s)).ToList();
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
        };

        return dto;
    }

}
