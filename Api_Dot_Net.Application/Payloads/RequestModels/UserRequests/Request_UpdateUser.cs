using Api_Dot_Net.Domain.ConstantsDomain;
using Api_Dot_Net.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api_Dot_Net.Application.Payloads.RequestModels.UserRequests
{
    public class Request_UpdateUser
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public IFormFile Avata { get; set; }
        public string FullName { get; set; }
       
    }
}
