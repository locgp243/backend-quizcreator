using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backendV1.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backendV1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly IConfiguration _configuration;

        public AccountsController(DBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration; 
        }

        // GET: api/Accounts
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Account>>> GetAccounts()
        {
            return await _context.Accounts.ToListAsync();
        }

        [HttpGet("")]
        public async Task<ActionResult<IEnumerable<Account>>> GetAccounts([FromQuery] int page, [FromQuery] int pageSize)
        {
            var accounts = await _context.Accounts
                                        .Skip((page - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToListAsync();

            return accounts;
        }



        // GET: api/Accounts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Account>> GetAccount(string id)
        {
            var account = await _context.Accounts.FindAsync(id);

            if (account == null)
            {
                return NotFound();
            }

            return account;
        }

        [HttpPut("{accountId}")]
        public async Task<IActionResult> PutAccount(int accountId, [FromForm] UserUpdateModel model)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountId);
            if (account == null)
            {
                return NotFound();
            }

            account.Username = model.Username;
            account.Role = model.Role;

            if (model.Avatar != null)
            {
                var avatarUrl = await SaveAvatarAsync(model.Avatar);
                account.Avatar = avatarUrl;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccountExists(accountId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        private bool AccountExists(int accountId)
        {
            return _context.Accounts.Any(e => e.AccountId == accountId);
        }





        public class UserUpdateModel
        {
            public string Username { get; set; }
            public string Role { get; set; }
            public IFormFile Avatar { get; set; }
        }


        public class UserCreateModel
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
            public string? Email { get; set; }
            public string? Role { get; set; }
            public IFormFile? Avatar { get; set; }
        }

        // POST: api/Accounts
        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromForm] UserCreateModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var avatarUrl = model.Avatar != null ? await SaveAvatarAsync(model.Avatar) : null;
                    var user = new Account
                    {
                        Username = model.Username ?? string.Empty,
                        Password = model.Password ?? string.Empty,
                        Email = model.Email ?? string.Empty,
                        Role = model.Role ?? string.Empty,
                        Avatar = avatarUrl
                    };

                    _context.Accounts.Add(user);
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        user.Username,
                        user.Email,
                        user.Role,
                        AvatarUrl = avatarUrl // Trả về URL công khai của ảnh
                    });
                }
                catch (Exception ex)
                {
                    // Log lỗi
                    Console.Error.WriteLine($"Error occurred: {ex.Message}");
                    return StatusCode(500, "Internal server error");
                }
            }
            return BadRequest(ModelState);
        }   


        private async Task<string> SaveAvatarAsync(IFormFile avatar)
        {
            try
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(avatar.FileName);
                var filePath = Path.Combine("avatars", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)); // Đảm bảo thư mục tồn tại

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }

                var url = $"/avatars/{fileName}"; // Đường dẫn công khai
                return url;
            }
            catch (Exception ex)
            {
                // Log lỗi
                Console.Error.WriteLine($"Error saving avatar: {ex.Message}");
                throw;
            }
        }



        // DELETE: api/Accounts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /*[HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);
            if (user == null)
            {
                return Unauthorized(new { Message = "Email hoặc mật khẩu không đúng." });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role) // Add role claim
        }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { Token = tokenString });
        }*/
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);
            if (user == null)
            {
                return Unauthorized(new { Message = "Email hoặc mật khẩu không đúng." });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role) // Add role claim
        }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                Token = tokenString,
                User = new
                {
                    user.AccountId,
                    user.Username,
                    user.Email,
                    user.Role,
                    Avatar = user.Avatar // Assuming Avatar is a URL or path to the image
                }
            });
        }



        public class LoginModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }


    }
}
