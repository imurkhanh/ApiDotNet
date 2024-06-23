using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api_Dot_Net.Application.Payloads.ResponseModels.DataUsers
{
    public class DataResponseUser : DataResponseBase
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public DateTime CreateTime { get; set; }
        /*[MaybeNull]*/
        public DateTime? UpdateTime { get; set; }
        public string Avata { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string UserStatus { get; set; }
    }
}
