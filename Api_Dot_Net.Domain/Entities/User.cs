using Api_Dot_Net.Domain.ConstantsDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api_Dot_Net.Domain.Entities
{
    public class User : BaseEntity
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime CreateTime { get; set; }
        /*[MaybeNull]*/
        public DateTime? UpdateTime { get; set; }
        public string Avata { get; set; }
        public string FullName { get; set; }
        public virtual ICollection<Permission>? Users { get; set; }
        public Enumerates.UserStatusEnum UserStatus { get; set; } = Enumerates.UserStatusEnum.UnActivated;
    }
}
