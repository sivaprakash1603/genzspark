using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Domain.Entities;
using BusBooking.Api.Domain.Enums;
using BusBooking.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Api.Application.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthService(AppDbContext db, IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator, IEmailService emailService, ILogger<AuthService> logger)
    {
        _db = db;
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterPassengerAsync(RegisterPassengerRequest request)
    {
        _logger.LogInformation("RegisterPassenger requested. Username={Username} Email={Email}", request.Username, request.Email);
        await EnsureUniqueUserAsync(request.Username, request.Email);
        var role = await _db.Roles.FirstAsync(x => x.Name == RoleNames.Passenger);

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            RoleId = role.Id,
            IsActive = true
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _userRepository.AddAsync(user);

        _logger.LogInformation("Passenger registered. UserId={UserId} Username={Username}", user.Id, user.Username);

        await _emailService.SendAsync(user.Email, "Registration Successful", "Welcome to Bus Booking Platform", "user-registration", user.Id);

        var token = _jwtTokenGenerator.Generate(user, role.Name);
        return new AuthResponse(token, user.Username, role.Name);
    }

    public async Task<AuthResponse> RegisterOperatorAsync(RegisterOperatorRequest request)
    {
        _logger.LogInformation("RegisterOperator requested. Username={Username} Email={Email} VehicleNumber={VehicleNumber}", request.Username, request.Email, request.VehicleNumber);
        await EnsureUniqueUserAsync(request.Username, request.Email);
        var role = await _db.Roles.FirstAsync(x => x.Name == RoleNames.Operator);

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            RoleId = role.Id,
            IsActive = true
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        _db.OperatorProfiles.Add(new OperatorProfile
        {
            User = user,
            VehicleNumber = request.VehicleNumber,
            ApprovalStatus = "Pending",
            IsEnabled = false
        });

        await _db.SaveChangesAsync();

        _logger.LogInformation("Operator registered (pending approval). UserId={UserId} Username={Username}", user.Id, user.Username);

        await _emailService.SendAsync(user.Email, "Operator Registration Submitted", "Your operator account is pending admin approval.", "operator-registration", user.Id);

        var token = _jwtTokenGenerator.Generate(user, role.Name);
        return new AuthResponse(token, user.Username, role.Name);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Login requested. Username={Username}", request.Username);
        var user = await _userRepository.GetByUsernameAsync(request.Username)
            ?? throw new InvalidOperationException("Invalid username or password");

        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verify == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Login failed (bad password). Username={Username} UserId={UserId}", request.Username, user.Id);
            throw new InvalidOperationException("Invalid username or password");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login blocked (user disabled). Username={Username} UserId={UserId}", request.Username, user.Id);
            throw new InvalidOperationException("User is disabled");
        }

        if (user.Role?.Name == RoleNames.Operator)
        {
            var profile = await _db.OperatorProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id)
                ?? throw new InvalidOperationException("Operator profile not found");

            if (profile.ApprovalStatus != "Approved" || !profile.IsEnabled)
            {
                _logger.LogWarning("Login blocked (operator not approved/enabled). Username={Username} UserId={UserId} Approval={Approval} Enabled={Enabled}", request.Username, user.Id, profile.ApprovalStatus, profile.IsEnabled);
                throw new InvalidOperationException("Operator not approved or disabled");
            }
        }

        var roleName = user.Role?.Name ?? RoleNames.Passenger;
        var token = _jwtTokenGenerator.Generate(user, roleName);

        _logger.LogInformation("Login successful. Username={Username} UserId={UserId} Role={Role}", request.Username, user.Id, roleName);
        return new AuthResponse(token, user.Username, roleName);
    }

    private async Task EnsureUniqueUserAsync(string username, string email)
    {
        if (await _db.Users.AnyAsync(x => x.Username == username))
        {
            _logger.LogWarning("Registration blocked: username already exists. Username={Username}", username);
            throw new InvalidOperationException("Username already exists");
        }

        if (await _db.Users.AnyAsync(x => x.Email == email))
        {
            _logger.LogWarning("Registration blocked: email already exists. Email={Email}", email);
            throw new InvalidOperationException("Email already exists");
        }
    }
}
