using System;

namespace Site.Application.DTO;

public class CreateSiteDTO
{
    public string Path { get; set; }
    public string NameEn { get; set; }
    public string NameAr { get; set; }
    public Guid? ParentId { get; set; }

}
