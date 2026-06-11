using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Domain.Entities;
using BusBooking.Api.Domain.Enums;
using BusBooking.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Api.Application.Services;

internal class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthService(AppDbContext db, IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator, IEmailService emailService, IUnitOfWork unitOfWork, ILogger<AuthService> logger)
    {
        _db = db;
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterPassengerAsync(RegisterPassengerRequest request)
    {
        _logger.LogInformation("RegisterPassenger requested. Username={Username} Email={Email}", request.Username, request.Email);
        await EnsureUniqueUserAsync(request.Username, request.Email);
        
        var user = await CreateUserAsync(request.Username, request.Email, request.Password, RoleNames.Passenger);
        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Passenger registered. UserId={UserId} Username={Username}", user.Id, user.Username);
        await _emailService.SendAsync(user.Email, "Registration Successful", "Welcome to Bus Booking Platform", "user-registration", user.Id);

        var token = _jwtTokenGenerator.Generate(user, RoleNames.Passenger);
        return new AuthResponse(token, user.Username, RoleNames.Passenger);
    }

    public async Task<AuthResponse> RegisterOperatorAsync(RegisterOperatorRequest request)
    {
        _logger.LogInformation("RegisterOperator requested. Username={Username} Email={Email}", request.Username, request.Email);
        await EnsureUniqueUserAsync(request.Username, request.Email);
        
        var user = await CreateUserAsync(request.Username, request.Email, request.Password, RoleNames.Operator);
        await _userRepository.AddAsync(user);
        _db.OperatorProfiles.Add(new OperatorProfile { User = user, ApprovalStatus = ApprovalStatus.Pending, IsEnabled = false });
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Operator registered (pending approval). UserId={UserId} Username={Username}", user.Id, user.Username);
        await _emailService.SendAsync(user.Email, "Operator Registration Submitted", "Your operator account is pending admin approval.", "operator-registration", user.Id);

        var token = _jwtTokenGenerator.Generate(user, RoleNames.Operator);
        return new AuthResponse(token, user.Username, RoleNames.Operator);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Login requested. Username={Username}", request.Username);
        var user = await _userRepository.GetByUsernameAsync(request.Username) ?? throw new InvalidOperationException("Invalid username or password");

        VerifyPasswordAndStatus(user, request.Password);
        
        if (user.Role?.Name == RoleNames.Operator)
        {
            await ValidateOperatorProfileAsync(user);
        }

        var roleName = user.Role?.Name ?? RoleNames.Passenger;
        var token = _jwtTokenGenerator.Generate(user, roleName);

        _logger.LogInformation("Login successful. Username={Username} UserId={UserId} Role={Role}", request.Username, user.Id, roleName);
        return new AuthResponse(token, user.Username, roleName);
    }

    private async Task<User> CreateUserAsync(string username, string email, string password, string roleName)
    {
        var role = await _db.Roles.FirstAsync(x => x.Name == roleName);
        var user = new User { Username = username, Email = email, RoleId = role.Id, IsActive = true };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);
        return user;
    }

    private void VerifyPasswordAndStatus(User user, string password)
    {
        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verify == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Login failed (bad password). Username={Username} UserId={UserId}", user.Username, user.Id);
            throw new InvalidOperationException("Invalid username or password");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login blocked (user disabled). Username={Username} UserId={UserId}", user.Username, user.Id);
            throw new InvalidOperationException("User is disabled");
        }
    }

    private async Task ValidateOperatorProfileAsync(User user)
    {
        var profile = await _db.OperatorProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id) ?? throw new InvalidOperationException("Operator profile not found");

        if (profile.ApprovalStatus != ApprovalStatus.Approved || !profile.IsEnabled)
        {
            _logger.LogWarning("Login blocked (operator not approved/enabled). Username={Username} UserId={UserId} Approval={Approval} Enabled={Enabled}", user.Username, user.Id, profile.ApprovalStatus, profile.IsEnabled);
            throw new InvalidOperationException("Operator not approved or disabled");
        }
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
