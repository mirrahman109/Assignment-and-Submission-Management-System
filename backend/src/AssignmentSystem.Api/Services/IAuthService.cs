using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Api.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher<Models.Entities.User> _passwordHasher;

    public AuthService(AppDbContext db, IJwtTokenGenerator jwtTokenGenerator, IPasswordHasher<Models.Entities.User> passwordHasher)
    {
        _db = db;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);
        if (user is null)
        {
            // Deliberately identical for "no such user" and "wrong password" so the response
            // can't be used to enumerate which email addresses exist.
            throw new Common.UnauthorizedAppException("Invalid email or password.");
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            // Deliberately identical for "no such user" and "wrong password" so the response
            // can't be used to enumerate which email addresses exist.
            throw new Common.UnauthorizedAppException("Invalid email or password.");
        }

        var (token, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(user);

        return new LoginResponse(
            token,
            expiresAtUtc,
            new UserSummary(user.Id, user.FullName, user.Email, user.Role.ToString(), user.ClassCourseId));
    }
}
