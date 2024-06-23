using Api_Dot_Net.Application.Payloads.ResponseModels.DataUsers;
using Api_Dot_Net.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api_Dot_Net.Application.Payloads.Mappers
{
    public class UserConverter
    {
        public DataResponseUser EntityToDTO(User user)
        {
            return new DataResponseUser
            {
                Avata = user.Avata,
                CreateTime = user.CreateTime,
                DateOfBirth = user.DateOfBirth,
                Email = user.Email,
                FullName = user.FullName,
                Id = user.Id,
                PhoneNumber = user.PhoneNumber,
                UpdateTime = user.UpdateTime,
                UserStatus = user.UserStatus.ToString(),
            };
        }
    }
}
