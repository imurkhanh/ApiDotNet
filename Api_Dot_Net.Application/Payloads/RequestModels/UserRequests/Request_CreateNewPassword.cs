﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api_Dot_Net.Application.Payloads.RequestModels.UserRequests
{
    public class Request_CreateNewPassword
    {
        public string ConfirmCode {  get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
