using Api_Dot_Net.Application.Payloads.RequestModels.UserRequests;
using Api_Dot_Net.Application.Payloads.ResponseModels.DataUsers;
using Api_Dot_Net.Application.Payloads.Responses;
using Api_Dot_Net.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api_Dot_Net.Application.InterfaceService
{
    public interface IAuthService
    {
        Task<ResponseObject<DataResponseUser>> Register(Request_Register request);
        Task<string> ConfirmRegisterAccount(string confirmCode);
        Task<ResponseObject<DataResponseLogin>> GetJwtTokenAsync(User user);
        Task<ResponseObject<DataResponseLogin>> Login(Request_Login request);
        Task<ResponseObject<DataResponseUser>> ChangePassword(long userId, Request_ChangePassword request);
        Task<string> FogotPassword(string email);
        Task<string> ConfirmCreateNewPassword(Request_CreateNewPassword request);
        Task<string> AddRolesToUser (long userId, List<string> roles);
        Task<string> DeleteRoles(long userId, List<string> roles);
    }
}
