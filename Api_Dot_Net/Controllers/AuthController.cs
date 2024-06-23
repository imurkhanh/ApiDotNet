﻿using Api_Dot_Net.Application.InterfaceService;
using Api_Dot_Net.Application.Payloads.RequestModels.UserRequests;
using Api_Dot_Net.Application.Payloads.ResponseModels.DataUsers;
using Api_Dot_Net.Application.Payloads.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Api_Dot_Net.Controllers
{
    [Route(Constant.DefaultValue.DEFAULT_CONTROLLER_ROUTE)]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] Request_Register request)
        {
            return Ok(await _authService.Register(request));
        }
        [HttpPost]
        public async Task<IActionResult> ConfirmRegisterAccount(string confirmCode)
        {
            return Ok(await _authService.ConfirmRegisterAccount(confirmCode));
        }
        [HttpPost]
        public async Task<IActionResult> Login(Request_Login request)
        {
            return Ok(await _authService.Login(request));
        }
        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // Xác thực bằng JWT, dựa vào thông tin mô tả có trong token và trong trường hợp này là Id
        public async Task<IActionResult> ChangePassword([FromBody]Request_ChangePassword request)
        {
            long id = long.Parse(HttpContext.User.FindFirst("Id").Value);
            return Ok(await _authService.ChangePassword(id, request));
        }
        [HttpPost]
        public async Task<IActionResult> FogotPassword(string email)
        {
            return Ok(await _authService.FogotPassword(email));
        }
        [HttpPut]
        public async Task<IActionResult> ConfirmCreateNewPassword([FromBody]Request_CreateNewPassword request)
        {
            return Ok(await _authService.ConfirmCreateNewPassword(request));
        }
        [HttpPost("{userId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AddRolesToUser([FromRoute]long userId,[FromBody] List<string> roles)
        {
            return Ok(await _authService.AddRolesToUser(userId, roles));
        }
        [HttpDelete("{userId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteRoles([FromRoute] long userId, [FromBody] List<string> roles)
        {
            return Ok(await _authService.DeleteRoles(userId, roles));
        }
        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateUser([FromForm]Request_UpdateUser request)
        {
            long id = long.Parse(HttpContext.User.FindFirst("Id").Value);
            return Ok(await _userService.UpdateUser(id, request));
        }
        [HttpGet]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Upload/Files", fileName);
            var provider = new FileExtensionContentTypeProvider();
            if(!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";

            }
            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(bytes, contentType, Path.GetFileName(fileName));
        }
    }
}
