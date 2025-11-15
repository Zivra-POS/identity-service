using IdentityService.Shared.DTOs.Response.Branch;

namespace IdentityService.Core.Mappers.Branch;

public static class BranchMapper
{
    public static BranchResponse ToResponse(Entities.Branch b)
    {
        return new BranchResponse
        {
            Id = b.Id,
            HashedId = b.HashedId,
            StoreId = b.StoreId,
            Name = b.Name,
            Code = b.Code,
            Address = b.Address,
            Province = b.Province,
            City = b.City,
            District = b.District,
            Rt = b.Rt,
            Rw = b.Rw,
            Phone = b.Phone,
            IsActive = b.IsActive,
            CreDate = b.CreDate,
            CreBy = b.CreBy,
            CreIpAddress = b.CreIpAddress,
            ModDate = b.ModDate
        };
    }
}

