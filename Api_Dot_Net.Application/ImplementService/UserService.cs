using Api_Dot_Net.Application.Handle.HandleFile;
using Api_Dot_Net.Application.InterfaceService;
using Api_Dot_Net.Application.Payloads.Mappers;
using Api_Dot_Net.Application.Payloads.RequestModels.UserRequests;
using Api_Dot_Net.Application.Payloads.ResponseModels.DataUsers;
using Api_Dot_Net.Application.Payloads.Responses;
using Api_Dot_Net.Domain.Entities;
using Api_Dot_Net.Domain.InterfaceRepositories;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api_Dot_Net.Application.ImplementService
{
    public class UserService : IUserService
    {
        private readonly IBaseRepository<User> _repository;
        private readonly UserConverter _converter;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserService(IBaseRepository<User> repository, UserConverter converter, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _converter = converter;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseObject<DataResponseUser>> UpdateUser(long userId, Request_UpdateUser request)
        {
            var currentUser = _httpContextAccessor.HttpContext.User;
            try
            {
                if(!currentUser.Identity.IsAuthenticated) // tài khoản chưa xác thực, không biết thằng này là thằng nào
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status401Unauthorized,
                        Message = "Người dùng chưa được xác thực",
                        Data = null
                    };
                }
                
                var user = currentUser.FindFirst("Id").Value;
                var userItem = await _repository.GetByIdAsync(userId);
                if (long.Parse(user) != userId && long.Parse(user) != userItem.Id)
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Message = "Bạn khong có quyền thực hiện chức năng này",
                        Data= null
                    };
                }
                userItem.Avata = await HandleUploadFile.WriteFile(request.Avata);
                userItem.PhoneNumber = request.PhoneNumber;
                userItem.DateOfBirth = request.DateOfBirth;
                userItem.Email = request.Email;
                userItem.UpdateTime= DateTime.Now;
                userItem.FullName = request.FullName;
                await _repository.UpdateAsync(userItem);
                return new ResponseObject<DataResponseUser>
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Cập nhập thong tin người dùng thành công",
                    Data = _converter.EntityToDTO(userItem)
                };

            }catch (Exception ex)
            {
                throw;
            }
        }
    }
}
