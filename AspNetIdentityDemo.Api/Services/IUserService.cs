using AspNetIdentityDemo.Api.Models;
using AspNetIdentityDemo.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;

namespace AspNetIdentityDemo.Api.Services
{
    public interface IUserService
    {

        Task<UserManagerResponse> RegisterUserAsync(RegisterViewModel model);



        Task<UserManagerResponse> ConfirmEmailAsync(string userId, string token);

        Task<UserManagerResponse> ForgetPasswordAsync(string email);


        Task<UserManagerResponse> ResetPasswordAsync(ResetPasswordViewModel model);

        Task<AuthenticationResult> LoginWithFacebookAsync(string accessToken);
       
        public class UserService : IUserService
        {

            private UserManager<IdentityUser> _userManger;
            private IConfiguration _configuration;
            private IMailService _mailService;
            private ApplicationDbContext _applicationDbContext;
            private readonly ApplicationSettings _appSettings;
            private readonly IFacebookAuthService _facebookAuthService;
            private readonly RoleManager<IdentityRole> _roleManager;
            private readonly JwtSettings _jwtSettings;
            public UserService(JwtSettings jwtSettings, RoleManager<IdentityRole> roleManager, IFacebookAuthService facebookAuthService, UserManager<IdentityUser> userManager, IConfiguration configuration, IMailService mailService, ApplicationDbContext applicationDbContext)
            {
                _userManger = userManager;
                _configuration = configuration;
                _jwtSettings = jwtSettings;
                _mailService = mailService;
                _applicationDbContext = applicationDbContext;
                _facebookAuthService = facebookAuthService;
                _roleManager = roleManager;
            }

            public async Task<UserManagerResponse> RegisterUserAsync(RegisterViewModel model)
            {
                if (model == null)
                    throw new NullReferenceException("Reigster Model is null");

                string allowedNums = "0123456789";
                Random random = new Random();
                char[] chars = new char[6];
                for (int i = 0; i < chars.Length; i++)
                {
                    chars[i] = allowedNums[(int)(chars.Length * random.NextDouble())];
                }
                string OTP = new string(chars);


                var identityUser = new IdentityUser
                {
                    Email = model.Email,
                    UserName = model.Email,
                };

                var result = await _userManger.CreateAsync(identityUser, model.Password);
                await _userManger.AddToRoleAsync(identityUser, "user");

                if (result.Succeeded)
                {
                    await _applicationDbContext.User.AddAsync(new User()
                    {
                        UserID = identityUser.Id,
                        FirstName = model.FirstName,
                        LastName = model.LastName,

                        Email = model.Email,
                        VerificationCode = OTP


                    });
                    await _applicationDbContext.SaveChangesAsync();

                    var confirmEmailToken = await _userManger.GenerateEmailConfirmationTokenAsync(identityUser);

                    var encodedEmailToken = Encoding.UTF8.GetBytes(confirmEmailToken);
                    var validEmailToken = WebEncoders.Base64UrlEncode(encodedEmailToken);

                    string url = $"{_configuration["AppUrl"]}/api/auth/confirmemail?userid={identityUser.Id}&token={validEmailToken}";

                    await _mailService.SendEmailAsync(identityUser.Email, "Confirm your email", $"<h1>Welcome to Auth Demo</h1>" +
                        $"<p>VerificationCode is'{OTP}'</p>");


                    return new UserManagerResponse
                    {
                        Message = "User created successfully!",
                        IsSuccess = true,
                    };
                }

                return new UserManagerResponse
                {
                    Message = "User did not create",
                    IsSuccess = false,
                    Errors = result.Errors.Select(e => e.Description)
                };

            }


            public async Task<UserManagerResponse> ConfirmEmailAsync(string userId, string token)
            {
                var user = await _userManger.FindByIdAsync(userId);
                if (user == null)
                    return new UserManagerResponse
                    {
                        IsSuccess = false,
                        Message = "User not found"
                    };

                var decodedToken = WebEncoders.Base64UrlDecode(token);
                string normalToken = Encoding.UTF8.GetString(decodedToken);

                var result = await _userManger.ConfirmEmailAsync(user, normalToken);

                if (result.Succeeded)
                    return new UserManagerResponse
                    {
                        Message = "Email confirmed successfully!",
                        IsSuccess = true,
                    };

                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "Email did not confirm",
                    Errors = result.Errors.Select(e => e.Description)
                };
            }
            public async Task<AuthenticationResult> LoginWithFacebookAsync(string accessToken)
            {
                var validatedTokenResult = await _facebookAuthService.ValidateAccessTokenAsync(accessToken);

                if (!validatedTokenResult.Data.IsValid)
                {
                    return new AuthenticationResult
                    {
                        Errors = new[] { "Invalid Facebook token" }
                    };
                }

                var userInfo = await _facebookAuthService.GetUserInfoAsync(accessToken);

                var user = await _userManger.FindByEmailAsync(userInfo.Email);

                if (user == null)
                {
                    var identityUser = new IdentityUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        Email = userInfo.Email,
                        UserName = userInfo.Email
                    };

                    var createdResult = await _userManger.CreateAsync(identityUser);
                    if (!createdResult.Succeeded)
                    {
                        return new AuthenticationResult
                        {
                            Errors = new[] { "Something went wrong" }
                        };
                    }

                    return await GenerateAuthenticationResultForUserAsync(identityUser);
                }

                return await GenerateAuthenticationResultForUserAsync(user);
            }

            public async Task<UserManagerResponse> ForgetPasswordAsync(string email)
            {
                var user = await _userManger.FindByEmailAsync(email);
                if (user == null)
                    return new UserManagerResponse
                    {
                        IsSuccess = false,
                        Message = "No user associated with email",
                    };

                var token = await _userManger.GeneratePasswordResetTokenAsync(user);
                var encodedToken = Encoding.UTF8.GetBytes(token);
                var validToken = WebEncoders.Base64UrlEncode(encodedToken);

                string url = $"{_configuration["AppUrl"]}/ResetPassword?email={email}&token={validToken}";

                await _mailService.SendEmailAsync(email, "Reset Password", "<h1>Follow the instructions to reset your password</h1>" +
                    $"<p>To reset your password <a href='{url}'>Click here</a></p>");

                return new UserManagerResponse
                {
                    IsSuccess = true,
                    Message = "Reset password URL has been sent to the email successfully!"
                };
            }
            private async Task<AuthenticationResult> GenerateAuthenticationResultForUserAsync(IdentityUser user)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

                var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("id", user.Id)
            };

                var userClaims = await _userManger.GetClaimsAsync(user);
                claims.AddRange(userClaims);

                var userRoles = await _userManger.GetRolesAsync(user);
                foreach (var userRole in userRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, userRole));
                    var role = await _roleManager.FindByNameAsync(userRole);
                    if (role == null) continue;
                    var roleClaims = await _roleManager.GetClaimsAsync(role);

                    foreach (var roleClaim in roleClaims)
                    {
                        if (claims.Contains(roleClaim))
                            continue;

                        claims.Add(roleClaim);
                    }
                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.Add(_jwtSettings.TokenLifetime),
                    SigningCredentials =
                        new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);

                var refreshToken = new RefreshToken
                {
                    JwtId = token.Id,
                    UserId = user.Id,
                    CreationDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddMonths(6)
                };

                await _applicationDbContext.RefreshTokens.AddAsync(refreshToken);
                await _applicationDbContext.SaveChangesAsync();

                return new AuthenticationResult
                {
                    Success = true,
                    Token = tokenHandler.WriteToken(token),
                    RefreshToken = refreshToken.Token
                };
            }


            public async Task<UserManagerResponse> ResetPasswordAsync(ResetPasswordViewModel model)
            {
                var user = await _userManger.FindByEmailAsync(model.Email);
                if (user == null)
                    return new UserManagerResponse
                    {
                        IsSuccess = false,
                        Message = "No user associated with email",
                    };

                if (model.NewPassword != model.ConfirmPassword)
                    return new UserManagerResponse
                    {
                        IsSuccess = false,
                        Message = "Password doesn't match its confirmation",
                    };

                var decodedToken = WebEncoders.Base64UrlDecode(model.Token);
                string normalToken = Encoding.UTF8.GetString(decodedToken);

                var result = await _userManger.ResetPasswordAsync(user, normalToken, model.NewPassword);

                if (result.Succeeded)
                    return new UserManagerResponse
                    {
                        Message = "Password has been reset successfully!",
                        IsSuccess = true,
                    };

                return new UserManagerResponse
                {
                    Message = "Something went wrong",
                    IsSuccess = false,
                    Errors = result.Errors.Select(e => e.Description),
                };
            }
        }
    }
}
