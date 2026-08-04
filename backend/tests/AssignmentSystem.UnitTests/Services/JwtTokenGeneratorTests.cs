using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Entities.Enums;
using AssignmentSystem.Api.Services;
using AssignmentSystem.UnitTests.Fixtures;
using Microsoft.Extensions.Configuration;

namespace AssignmentSystem.UnitTests.Services;

public class JwtTokenGeneratorTests
{
    private static IConfiguration BuildConfig() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "unit-test-signing-key-at-least-32-characters-long",
            ["Jwt:Issuer"] = "AssignmentSystemTests",
            ["Jwt:Audience"] = "AssignmentSystemTestsClient",
            ["Jwt:ExpiryMinutes"] = "60"
        })
        .Build();

    [Fact]
    public void GenerateToken_IncludesCorrectRoleAndUserIdClaims()
    {
        var generator = new JwtTokenGenerator(BuildConfig(), new FakeClock());
        var user = new User
        {
            Id = 42,
            FullName = "Ada Lovelace",
            Email = "ada@test.local",
            Role = RoleType.Teacher
        };

        var (token, _) = generator.GenerateToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("Teacher", jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        Assert.Equal("42", jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal("ada@test.local", jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);
    }

    [Fact]
    public void GenerateToken_UsesConfiguredIssuerAndAudience()
    {
        var generator = new JwtTokenGenerator(BuildConfig(), new FakeClock());
        var user = new User { Id = 1, FullName = "Test", Email = "t@test.local", Role = RoleType.Admin };

        var (token, _) = generator.GenerateToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("AssignmentSystemTests", jwt.Issuer);
        Assert.Equal("AssignmentSystemTestsClient", jwt.Audiences.Single());
    }
}
