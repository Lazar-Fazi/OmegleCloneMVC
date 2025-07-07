using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OmegleCloneMVC.Data;
using OmegleCloneMVC.Models;

using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace MvcMovie.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly OmegleCloneMVCContext _context;
        private readonly IConfiguration _configuration;
        public UsersController(OmegleCloneMVCContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        [HttpPost]
        [Route("Registration")]
        public IActionResult Registration(UserDto userDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var existingUser = _context.User.FirstOrDefault(x => x.Mail == userDTO.Mail);
            if (existingUser != null)
                return BadRequest("Korisnik sa tim emailom već postoji.");
            var hasher = new PasswordHasher<User>();
            var newUser = new User
            {
                Username = userDTO.Username,
                Mail = userDTO.Mail,
                RoleId = (int)Roles.User
            };
            newUser.Password = hasher.HashPassword(newUser, userDTO.Password);
            _context.User.Add(newUser);
            _context.SaveChanges();
            return Ok("Uspešna registracija");
        }





        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            var user = _context.User
                .Include(u => u.Role)
                .FirstOrDefault(x => x.Mail.ToLower() == loginDTO.Mail.ToLower());
            if (user == null)
                return BadRequest("Pogrešan email ili lozinka");
            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.Password, loginDTO.Password);
            if (result == PasswordVerificationResult.Failed)
                return BadRequest("Pogrešan email ili lozinka");

            user.IsActive = 1;
            _context.SaveChanges();

            //  COOKIE AUTENTIFIKACIJA
            var cookieClaims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Mail),
                new(ClaimTypes.Role, user.Role.Name)
            };
            var identity = new ClaimsIdentity(cookieClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);


            // JWT GENERISANJE
            var jwtClaims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("Mail", user.Mail),
                new Claim("User", user.Username),
                new Claim("Role", user.Role.Name)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                jwtClaims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: signIn
            );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            user.IsActive = 1;
            _context.SaveChanges();
            return Ok(new
            {
                Token = jwt,
                User = new
                {
                    user.UserId,
                    user.Username,
                    user.Mail,
                    Role = user.Role.Name
                },
                RedirectUrl = Url.Action("Index", "AdminPanel")
            });
        }

        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] int userId)
        {
            var user = await _context.User.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Korisnik nije pronađen.");
            }

            user.IsActive = 0;
            await _context.SaveChangesAsync();

            return Ok("Logout uspešan.");
        }



        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet]
        [Route("GetUser")]
        public IActionResult GetUser(int id)
        {
            var user = _context.User.Include(u => u.Role).FirstOrDefault(x => x.UserId == id);
            if (user != null)
            {
                return Ok(new
                {
                    user.UserId,
                    user.Username,
                    user.Mail,
                    Role = user.Role.Name
                });
            }
            return NotFound();
        }
    }
}