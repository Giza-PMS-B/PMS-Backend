using System;
using Booking.Application.DTO;
using Booking.Model.Entities;
using SharedKernel.Infrastructure.Persistent.Abstraction;

namespace Booking.Application.Services;

public class SiteService
{
    private readonly IRepo<Site> _siteRepository;
    private readonly IUOW _uow;

    public SiteService(IRepo<Site> siteRepository, IUOW uow)
    {
        _siteRepository = siteRepository;
        _uow = uow;
    }
    public async Task<Site> CreateSiteAsync(CreateSiteDTO createSiteDTO)
    {
        throw new NotImplementedException();
    }
}
