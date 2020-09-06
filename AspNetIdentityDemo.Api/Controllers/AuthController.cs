using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AspNetIdentityDemo.Api.Models;
using AspNetIdentityDemo.Api.Services;
using AspNetIdentityDemo.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace AspNetIdentityDemo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private IUserService _userService;
        private IMailService _mailService;
        private IConfiguration _configuration;
        private UserManager<IdentityUser> _userManger;
        private readonly ApplicationSettings _appSettings;
        public AuthController(IOptions<ApplicationSettings> appSettings, UserManager<IdentityUser> userManager, IUserService userService, IMailService mailService, IConfiguration configuration)
        {
            _userService = userService;
            _mailService = mailService;
            _configuration = configuration;
            _userManger = userManager;
            _appSettings = appSettings.Value;
        }

            // /api/auth/register
            [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync([FromBody]RegisterViewModel model)
        {
            if(ModelState.IsValid)
            {
                var result = await _userService.RegisterUserAsync(model);

                if (result.IsSuccess)
                    return Ok(result); // Status Code: 200 

                return BadRequest(result);
            }

            return BadRequest("Some properties are not valid"); // Status code: 400
        }

        // /api/auth/login
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManger.FindByEmailAsync(model.Email);
               
                if (user != null && await _userManger.CheckPasswordAsync(user, model.Password))
                {
                    var role = await _userManger.GetRolesAsync(user);
                    IdentityOptions _options = new IdentityOptions();
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                        new Claim("UserID",user.Id.ToString()),
                        new Claim(_options.ClaimsIdentity.RoleClaimType,role.FirstOrDefault())
                        }),
                        Expires = DateTime.UtcNow.AddDays(1),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JWT_Secret)), SecurityAlgorithms.HmacSha256Signature)
                    };
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                    var token = tokenHandler.WriteToken(securityToken);
                    var response = new
                    {
                        id = user.Id.ToString(),
                        role = new { role },

                        auth_token = new { token }

                    };

                    var json = JsonConvert.SerializeObject(response);
                    return new OkObjectResult(response);
                }
            }
            return BadRequest(new { message = "Email or password is incorrect." });

        }
        [HttpPost("LoginWithFaceBook")]
        public async Task<IActionResult> FacebookAuth([FromBody] UserFacebookAuthRequest request)
        {
            var authResponse = await _userService.LoginWithFacebookAsync(request.AccessToken);

            if (!authResponse.Success)
            {
                return BadRequest(new AuthFailedResponse
                {
                    Errors = authResponse.Errors
                });
            }

            return Ok(new AuthSuccessResponse
            {
                Token = authResponse.Token,
                RefreshToken = authResponse.RefreshToken
            });
        }
    

    // /api/auth/confirmemail?userid&token
    [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                return NotFound();

            var result = await _userService.ConfirmEmailAsync(userId, token);

            if(result.IsSuccess)
            {
                return Redirect($"{_configuration["AppUrl"]}/ConfirmEmail.html");
            }

            return BadRequest(result);
        }

        // api/auth/forgetpassword
        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword([FromBody] LoginViewModel forget)
        {
            if (string.IsNullOrEmpty(forget.Email))
                return NotFound();

            var result = await _userService.ForgetPasswordAsync(forget.Email);

            if (result.IsSuccess)
                return Ok(result); // 200

            return BadRequest(result); // 400
        }

        // api/auth/resetpassword
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromForm]ResetPasswordViewModel model)
        {
            if(ModelState.IsValid)
            {
                var result = await _userService.ResetPasswordAsync(model);

                if (result.IsSuccess)
                    return Ok(result);

                return BadRequest(result);
            }

            return BadRequest("Some properties are not valid");
        }


    }
}