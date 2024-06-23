using Api_Dot_Net.Application.Handle.HandleEmail;
using Api_Dot_Net.Application.InterfaceService;
using Api_Dot_Net.Application.Payloads.Mappers;
using Api_Dot_Net.Application.Payloads.RequestModels.UserRequests;
using Api_Dot_Net.Application.Payloads.ResponseModels.DataUsers;
using Api_Dot_Net.Application.Payloads.Responses;
using Api_Dot_Net.Domain.ConstantsDomain;
using Api_Dot_Net.Domain.Entities;
using Api_Dot_Net.Domain.InterfaceRepositories;
using Api_Dot_Net.Domain.Validations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Api_Dot_Net.Application.ImplementService
{
    public class AuthService : IAuthService
    {
        private readonly IBaseRepository<User> _baseUserRepository;
        private readonly UserConverter _userConverter;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IBaseRepository<ConfirmEmail> _baseConfirmEmailRepository;
        private readonly IBaseRepository<Permission> _basePermissionRepository;
        private readonly IBaseRepository<Role> _baseRoleRepository;
        private readonly IBaseRepository<RefreshToken> _baseRefreshTokenRepository;
        private readonly IHttpContextAccessor _contextAccessor;
        public AuthService(IBaseRepository<User> baseUserRepository, UserConverter userConverter, IConfiguration configuration, 
            IUserRepository userRepository, IEmailService emailService, IBaseRepository<ConfirmEmail> baseConfirmEmailRepository, 
            IBaseRepository<Permission> basePermissionRepository, IBaseRepository<Role> baseRoleRepository,
            IBaseRepository<RefreshToken> baseRefreshTokenRepository, IHttpContextAccessor contextAccessor)
        {
            _baseUserRepository = baseUserRepository;
            _userConverter = userConverter;
            _configuration = configuration;
            _userRepository = userRepository;
            _emailService = emailService;
            _baseConfirmEmailRepository = baseConfirmEmailRepository;   
            _basePermissionRepository = basePermissionRepository;
            _baseRoleRepository = baseRoleRepository;
            _baseRefreshTokenRepository = baseRefreshTokenRepository;
            _contextAccessor = contextAccessor;
        }
        // nó báo là userRepo bị null
        public async Task<ResponseObject<DataResponseUser>> Register(Request_Register request)
        {
            try
            {
                if (!ValidateInput.IsValidEmail(request.Email))
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Định dạng email không hợp lệ",
                        Data = null
                    };
                }
                if (!ValidateInput.IsValidPhoneNumber(request.PhoneNumber))
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Định dạng số điện thoại không hợp lệ",
                        Data = null
                    };
                }
                var checkEmail = await _userRepository.GetUserByEmail(request.Email);
                if (checkEmail != null)
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Email đã tồn tại trong hệ thống! Vui lòng sử dụng email khác",
                        Data = null
                    };
                }
                if (await _userRepository.GetUserByPhoneNumber(request.PhoneNumber) != null)
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Số điện thoại đã tồn tại trong hệ thống! Vui lòng sử dụng số điện thoại khác",
                        Data = null
                    };
                }
                if (await _userRepository.GetUserByUsername(request.UserName) != null)
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "UserName đã tồn tại trong hệ thống! Vui lòng sử dụng UserName khác",
                        Data = null
                    };
                }
                var user = new User
                {
                    Avata = "https://static.vecteezy.com/system/resources/previews/009/734/564/original/default-avatar-profile-icon-of-social-media-user-vector.jpg",
                    IsActive = true,
                    CreateTime = DateTime.Now,
                    DateOfBirth = request.DateOfBirth,
                    Email = request.Email,
                    FullName = request.FullName,
                    Password = BCryptNet.HashPassword(request.Password),
                    PhoneNumber = request.PhoneNumber,
                    UserName = request.UserName,
                    UserStatus = Domain.ConstantsDomain.Enumerates.UserStatusEnum.UnActivated,
                };
                user = await _baseUserRepository.CreateAsync(user);
                await _userRepository.AddRolesToUserAsync(user, new List<string> { "User" });
                ConfirmEmail confirmEmail = new ConfirmEmail
                {
                    IsActive = true,
                    ConfirmCode = GenerateCodeActive(),
                    ExpiryTime = DateTime.Now.AddMinutes(1),
                    IsConfirmed = false,
                    Userid = user.Id,
                };
                confirmEmail = await _baseConfirmEmailRepository.CreateAsync(confirmEmail);
                var message = new EmailMessage(new string[] {request.Email}, "Nhận mã xác nhận tại đây: ", $"Mã xác nhận: {confirmEmail.ConfirmCode}");
                var responseMessage = _emailService.SendEmail(message);
                return new ResponseObject<DataResponseUser>
                {
                    Status = StatusCodes.Status201Created,
                    Message = "Bạn đã gửi yêu cầu đăng ký! Vui lòng nahạn mã xác nhận tại email để đăng ký tài khoản",
                    Data = _userConverter.EntityToDTO(user)
                };
            }
            catch (Exception ex)
            {
                return new ResponseObject<DataResponseUser>
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = "Error: " + ex.Message,
                    Data = null
                };
            }
        }
        private string GenerateCodeActive()
        {
            string str = "Ngockhanh_" + DateTime.Now.Ticks.ToString();
            return str;
        }

        public async Task<string> ConfirmRegisterAccount(string confirmCode)
        {
            try
            {
                var code = await _baseConfirmEmailRepository.GetAsync( x => x.ConfirmCode.Equals(confirmCode) );
                if(code == null )
                {
                    return "Mã xác nhận khong hợp lệ";
                }
                var user = await _baseUserRepository.GetAsync( x=>x.Id==code.Userid );
                if(code.ExpiryTime< DateTime.Now)
                {
                    return " Mã xác nhận đã hết hạn";
                }
                user.UserStatus = Domain.ConstantsDomain.Enumerates.UserStatusEnum.Activated;
                code.IsConfirmed = true;
                await _baseUserRepository.UpdateAsync( user );
                await _baseConfirmEmailRepository.UpdateAsync(code );
                return "Xác nhận đăng ký tài khoản thành công!";
            }catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<ResponseObject<DataResponseLogin>> GetJwtTokenAsync(User user)
        {
            var permissions = await _basePermissionRepository.GetAllAsync(x => x.UserId == user.Id);
            var roles = await _baseRoleRepository.GetAllAsync();
            var authClaims = new List<Claim>
            {
                new Claim("Id", user.Id.ToString()),
                new Claim("UserName", user.UserName.ToString()),
                new Claim("Email", user.Email.ToString()),
                new Claim("PhoneNumber", user.PhoneNumber.ToString()),

            };
            foreach( var permission in permissions )
            {
                foreach( var role in roles )
                {
                    if(role.Id == permission.RoleId)
                    {
                        authClaims.Add(new Claim("Permission", role.RoleName) );
                    }
                }
            }
            var userRoles = await _userRepository.GetRolesOfUserAsync(user);
            foreach(var item in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, item));
            }
            var jwtToken = GetToken(authClaims);
            var refreshtoken = GenereteRefreshToken();
            _ = int.TryParse(_configuration["JWT:RefreshTokenValidity"], out int refreshTokenValidity);

            RefreshToken rf = new RefreshToken
            {
                IsActive = true,
                ExpiryTime = DateTime.Now.AddHours(refreshTokenValidity),
                UserId = user.Id,
                Token = refreshtoken
            };
            rf = await _baseRefreshTokenRepository.CreateAsync(rf);
            return new ResponseObject<DataResponseLogin>
            {
                Status = StatusCodes.Status200OK,
                Message = "Tạo Token thành công",
                Data = new DataResponseLogin
                {
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    RefreshToken = refreshtoken,
                }
            };
        }

        public async Task<ResponseObject<DataResponseLogin>> Login(Request_Login request)
        {
            var user = await _baseUserRepository.GetAsync( x=> x.UserName.Equals(request.UserName) );
            if(user == null)
            {
                return new ResponseObject<DataResponseLogin>
                {
                    Status = StatusCodes.Status400BadRequest,
                    Message = "Sai tên tài khoản",
                    Data = null
                };
            }
            if(user.UserStatus.ToString().Equals(Enumerates.UserStatusEnum.UnActivated.ToString()) )
            {
                return new ResponseObject<DataResponseLogin>
                {
                    Status= StatusCodes.Status401Unauthorized,
                    Message = "Tài khoản chưa được xác thực",
                    Data = null
                };
            }
            bool checkPass = BCryptNet.Verify( request.Password, user.Password );
            if(!checkPass)
            {
                return new ResponseObject<DataResponseLogin>
                {
                    Status = StatusCodes.Status400BadRequest,
                    Message = " Mật khẩu không chính xác",
                    Data = null
                };
            }
            return new ResponseObject<DataResponseLogin>
            {
                Status = StatusCodes.Status200OK,
                Message = " Đăng nhập thành công",
                Data = new DataResponseLogin
                {
                    AccessToken = GetJwtTokenAsync(user).Result.Data.AccessToken,
                    RefreshToken = GetJwtTokenAsync(user).Result.Data.RefreshToken,
                }
            };
        }
        public async Task<ResponseObject<DataResponseUser>> ChangePassword(long userId, Request_ChangePassword request)
        {
            try
            {
                var user = await _baseUserRepository.GetByIdAsync(userId);
                bool checkPass = BCryptNet.Verify(request.OldPassword, user.Password);
                if(!checkPass)
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Mật khẩu không chính xác",
                        Data = null
                    };
                }
                if (!request.NewPassword.Equals(request.ConfirmPassword))
                {
                    return new ResponseObject<DataResponseUser>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Mật khẩu không trùng khớp",
                        Data = null
                    };
                }
                user.Password = BCryptNet.HashPassword(request.NewPassword);
                user.UpdateTime = DateTime.Now;
                await _baseUserRepository.UpdateAsync(user);
                return new ResponseObject<DataResponseUser>
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Đổi mật khẩu thành công",
                    Data = _userConverter.EntityToDTO(user)
                };
            }
            catch(Exception ex)
            {
                return new ResponseObject<DataResponseUser>
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = ex.Message,
                    Data = null
                };
            }
        }
        public async Task<string> FogotPassword(string email)
        {
            try
            {
                var user = await _userRepository.GetUserByEmail(email);
                if(user == null)
                {
                    return "Email không tồn tại trong hệ thống";
                }              
                ConfirmEmail confirmEmail = new ConfirmEmail
                {
                    IsActive = true,
                    ConfirmCode = GenerateCodeActive(),
                    ExpiryTime = DateTime.Now.AddMinutes(1),
                    Userid = user.Id,
                    IsConfirmed = false,
                };
                confirmEmail = await _baseConfirmEmailRepository.CreateAsync(confirmEmail);
                var message = new EmailMessage(new string[] { user.Email }, "Nhận mã xác nhận tại đây: ", $"Mã xác nhận là: {confirmEmail.ConfirmCode}");
                var send = _emailService.SendEmail(message);

                return "Gửi mã xác nhận về email thanh công, vui lòng kiểm tra!";
            }catch(Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> ConfirmCreateNewPassword(Request_CreateNewPassword request)
        {
            try
            {
                var confirmEmail = await _baseConfirmEmailRepository.GetAsync(x=>x.ConfirmCode.Equals(request.ConfirmCode));
                if(confirmEmail == null)
                {
                    return "Mã xác nhận không hợp lệ";
                }
                if(confirmEmail.ExpiryTime < DateTime.Now)
                {
                    return "Mã xác nhận đã hết hạn";
                }
                if(!request.NewPassword.Equals(request.ConfirmPassword))
                {
                    return "Mật khẩu không trùng khớp";
                }
                var user = await _baseUserRepository.GetAsync(x=> x.Id == confirmEmail.Userid);
                user.Password = BCryptNet.HashPassword(request.NewPassword);
                user.UpdateTime = DateTime.Now;
                await _baseUserRepository.UpdateAsync(user);
                return "Tạo mật khẩu mới thành công";
            }
            catch( Exception ex )
            {
                return ex.Message;
            }
        }
        public async Task<string> AddRolesToUser(long userId, List<string> roles)
        {
            var currentUser = _contextAccessor.HttpContext.User;
            try
            {
                if (!currentUser.Identity.IsAuthenticated)
                {
                    return "Người dùng chưa được xác thực";
                }
                if(!currentUser.IsInRole("Admin"))
                {
                    return "Bạn không có quyền thực hiện chức năng này";
                }
                var user = await _baseUserRepository.GetByIdAsync(userId);
                if(user == null)
                {
                    return "Không tìm thấy người dùng";
                }
                await _userRepository.AddRolesToUserAsync(user, roles);
                return "Thêm quyền cho người dùng thành công";
            }catch( Exception ex )
            {
                return ex.Message;
            }
        }

        public async Task<string> DeleteRoles(long userId, List<string> roles)
        {
            var currentUser = _contextAccessor.HttpContext.User;
            try
            {
                if (!currentUser.Identity.IsAuthenticated)
                {
                    return "Người dùng chưa được xác thực";
                }
                if (!currentUser.IsInRole("Admin"))
                {
                    return "Bạn không có quyền thực hiện chức năng này";
                }
                var user = await _baseUserRepository.GetByIdAsync(userId);
                if(user == null)
                {
                    return "Người dùng không tồn tại";
                }
                await _userRepository.DeleteRolesAsync(user, roles);
                return "Xóa quyền thành công";
               
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #region Privare Methods
        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
            _ = int.TryParse(_configuration["JWT:TokenValidityInHours"], out int tokenValidityInHours);
            var expirationUTC = DateTime.Now.AddHours(tokenValidityInHours);
            /*var localTimeZone = TimeZoneInfo.Local;*/
            /*var expirationTimeInLocalTimeZone = TimeZoneInfo.ConvertTimeToUtc(expirationUTC, localTimeZone);*/

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: expirationUTC,
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
            return token;
        }
        private string GenereteRefreshToken()
        {
            var randomNumber = new Byte[64];
            var range = RandomNumberGenerator.Create();
            range.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        #endregion
    }
}
