using FootballPredictionAPI.DTOs.Auth;
using FootballPredictionAPI.Services.Interfaces;
using FootballPredictionAPI.Data;
using FootballPredictionAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FootballPredictionAPI.Enums;
using Google.Apis.Auth;
using FootballPredictionAPI.Exceptions;
using System.IdentityModel.Tokens.Jwt;

namespace FootballPredictionAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        // It comes from the NuGet package we installed earlier:Microsoft.Extensions.Identity.Core. Namespace:using Microsoft.AspNetCore.Identity; 
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext context,IPasswordHasher<User> passwordHasher, IJwtTokenService jwtTokenService, IConfiguration configuration,ILogger<AuthService> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _configuration = configuration;
            _logger = logger;
        }


        private static DateTime GetTokenExpiration(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            var jwtToken = handler.ReadJwtToken(token);

            return jwtToken.ValidTo;
        }
        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            string normalizedEmail = request.Email.Trim().ToLowerInvariant();

            bool emailExists = await _context.Users
                    .AnyAsync(u => u.Email == normalizedEmail);
            //AnyAsync(...) asks the database whether at least one matching user exists.

            if (emailExists)
            {
                throw new ConflictException("Email is already registered.");
            }


            var user = new User
            {
                UserName = request.UserName,
                Email = normalizedEmail,
                Role = UserRole.Player,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash= _passwordHasher.HashPassword(user, request.Password);

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            _logger.LogInformation("New user registered successfully. UserId: {UserId}",user.Id);

            string token = _jwtTokenService.GenerateToken(user);

           

            return new AuthResponseDto
            {
                AccessToken = token,
                ExpiresAt = GetTokenExpiration(token),
                User = new UserResponseDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = user.Role.ToString()
                }
            };


        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            string normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user == null || user.PasswordHash == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var result = _passwordHasher.VerifyHashedPassword(
                 user,
                 user.PasswordHash,
                 request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Failed password login attempt for UserId: {UserId}",user.Id);
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _passwordHasher.HashPassword(
                    user,
                    request.Password);

                await _context.SaveChangesAsync();
            }


            //This records a successful password verification.
            _logger.LogInformation( "User logged in successfully. UserId: {UserId}", user.Id);

            string token = _jwtTokenService.GenerateToken(user);
            

            return new AuthResponseDto
            {
                AccessToken = token,
                ExpiresAt = GetTokenExpiration(token),
                User = new UserResponseDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = user.Role.ToString()
                }
            };

        }

        public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request)
        {

            if (string.IsNullOrWhiteSpace(request.IdToken))
            {
                throw new UnauthorizedAccessException(
                    "Google ID token is required.");
            }
            string clientId = _configuration["Google:ClientId"]
             ?? throw new InvalidOperationException(
           "Google Client ID is missing.");

            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            };

            GoogleJsonWebSignature.Payload payload;

            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(
                    request.IdToken,
                    settings);
            }
            catch (InvalidJwtException)
            {
                throw new UnauthorizedAccessException(
                    "Invalid Google ID token.");
            }
            catch (ArgumentException)
            {
                throw new UnauthorizedAccessException(
                    "Invalid Google ID token.");
            }

            if (payload.EmailVerified != true)
            {
                throw new UnauthorizedAccessException(
                    "Google email address is not verified.");
            }

            string googleId = payload.Subject;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.GoogleId == googleId);


            if (user == null)
            {
                string normalizedEmail = payload.Email.Trim().ToLowerInvariant();

                bool emailExists = await _context.Users
                    .AnyAsync(u => u.Email == normalizedEmail);

                if (emailExists)
                {
                    throw new ConflictException(
                                    "An account with this email already exists. " +
                                     "Please sign in using your existing login method.");
                }

                user = new User
                {
                    UserName = payload.Name ?? normalizedEmail.Split('@')[0],
                    Email = normalizedEmail,
                    GoogleId = googleId,
                    Role = UserRole.Player,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);

                await _context.SaveChangesAsync();
            }

            //succesfull google login logged
            _logger.LogInformation("Google authentication succeeded. UserId: {UserId}",user.Id);

            string token = _jwtTokenService.GenerateToken(user);

            

            return new AuthResponseDto
            {
                AccessToken = token,
                ExpiresAt = GetTokenExpiration(token),
                User = new UserResponseDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = user.Role.ToString()
                }
            };
        }
    }
}
