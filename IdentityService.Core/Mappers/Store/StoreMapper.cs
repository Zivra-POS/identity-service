using IdentityService.Shared.DTOs.Response.Store;

namespace IdentityService.Core.Mappers.Store;

public static class StoreMapper
{
    public static StoreResponse ToResponse(Entities.Store s)
    {
        return new StoreResponse
        {
            HashedId = s.HashedId,
            Id = s.Id,
            Name = s.Name,
            Code = s.Code,
            Address = s.Address,
            Province = s.Province,
            City = s.City,
            District = s.District,
            Rt = s.Rt,
            Rw = s.Rw,
            Phone = s.Phone,
            IsActive = s.IsActive,
            CostingMethod = s.CostingMethod,
            CreDate = s.CreDate,
            CreBy = s.CreBy,
            CreIpAddress = s.CreIpAddress,
            ModDate = s.ModDate,
            ModBy = s.ModBy,
            ModIpAddress = s.ModIpAddress,
            
        };
    }
}
