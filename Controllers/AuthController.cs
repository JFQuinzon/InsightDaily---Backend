using backend.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration; // Add this using statement

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration; // Add this line

        public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration; // Initialize _configuration
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var user = new AppUser()
            {
                DisplayName = model.DisplayName,
                UserName = model.Email,
                Email = model.Email,
                PasswordHash = model.Password,
            };
            var result = await _userManager.CreateAsync(user, user.PasswordHash!);
            string Message = string.Empty;
            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var signInResult = await _signInManager.PasswordSignInAsync(
                userName: model.UserName!,
                password: model.Password!,
                isPersistent: false,
                lockoutOnFailure: false
                );

            if (signInResult.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.UserName);

                if (user != null)
                {
                    // Return user data

                   // generate token for user
                    var token = GenerateAccessToken(model.UserName);
                    // return access token for user's use
                    return Ok(new { 
                        AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                        user.Id,
                        user.UserName,
                        user.Email,
                        user.DisplayName
                    });

                }
                return BadRequest("User not found.");
            }
            return BadRequest(signInResult);
        }
        // Generating token based on user information
        private JwtSecurityToken GenerateAccessToken(string userName)
        {
            // Create user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userName),
                // Add additional claims as needed (e.g., roles, etc.)
            };

            var audiences = new List<string>
            {
                _configuration["JwtSettings:Audience1"],
                _configuration["JwtSettings:Audience2"]
            };


            // Create a JWT
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1), 
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"])),
                    SecurityAlgorithms.HmacSha256)
            );
            // Add audience claims manually
            foreach (var audience in audiences)
            {
                token.Payload["aud"] = audience;
            }


            return token;
        }

        [HttpPost("validate-token")]
        public IActionResult ValidateToken([FromBody] TokenRequest request)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["JwtSettings:Issuer"],
                    ValidAudiences = new[]
                    {
                _configuration["JwtSettings:Audience1"],
                _configuration["JwtSettings:Audience2"]
            },
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]))
                };

                tokenHandler.ValidateToken(request.Token, validationParameters, out _);
                return Ok(new { valid = true, message = "Token is valid" });
            }
            catch (Exception)
            {
                return Unauthorized(new { valid = false, message = "Token is invalid" });
            }
        }



    }
}
