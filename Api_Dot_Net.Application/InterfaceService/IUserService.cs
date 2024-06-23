using Api_Dot_Net.Application.Payloads.RequestModels.UserRequests;
using Api_Dot_Net.Application.Payloads.ResponseModels.DataUsers;
using Api_Dot_Net.Application.Payloads.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api_Dot_Net.Application.InterfaceService
{
    public interface IUserService
    {
        Task<ResponseObject<DataResponseUser>> UpdateUser( long userId, Request_UpdateUser request);
    }
}
