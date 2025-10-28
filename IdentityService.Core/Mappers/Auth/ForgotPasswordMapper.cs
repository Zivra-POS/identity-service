using IdentityService.Core.Entities;
using IdentityService.Shared.DTOs.Response;

namespace IdentityService.Core.Mappers;

public class ForgotPasswordMapper
{
    public static ForgotPasswordResponse ToForgotPasswordResponse(UserToken req)
    {
        return new ForgotPasswordResponse
        {
            Name = req.Name,
            Token = req.Value
        };
    }
}