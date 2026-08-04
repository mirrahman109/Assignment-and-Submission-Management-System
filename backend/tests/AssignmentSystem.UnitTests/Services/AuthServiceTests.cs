using AssignmentSystem.Api.Common;
using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.DTOs.Auth;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Entities.Enums;
using AssignmentSystem.Api.Services;
using AssignmentSystem.UnitTests.Fixtures;
using Microsoft.AspNetCore.Identity;

namespace AssignmentSystem.UnitTests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly SqliteInMemoryDbContextFactory _factory;
    private readonly AppDbContext _db;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthServiceTests()
    {
        _factory = new SqliteInMemoryDbContextFactory();
        _db = _factory.Context;
    }

    public void Dispose() => _factory.Dispose();

    private async Task<User> SeedUserAsync(string email, string password)
    {
        var user = new User
        {
            FullName = "Test User",
            Email = email,
            Role = RoleType.Teacher,
            IsActive = true
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenPasswordIsWrong()
    {
        await SeedUserAsync("teacher@test.local", "CorrectPassword1");
        var service = new AuthService(_db, new FakeJwtTokenGenerator(), _passwordHasher);

        await Assert.ThrowsAsync<ValidationAppException>(() =>
            service.LoginAsync(new LoginRequest("teacher@test.local", "WrongPassword")));
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenEmailDoesNotExist()
    {
        var service = new AuthService(_db, new FakeJwtTokenGenerator(), _passwordHasher);

        await Assert.ThrowsAsync<ValidationAppException>(() =>
            service.LoginAsync(new LoginRequest("nobody@test.local", "WhateverPassword")));
    }

    [Fact]
    public async Task LoginAsync_Succeeds_WhenPasswordIsCorrect()
    {
        await SeedUserAsync("teacher2@test.local", "CorrectPassword1");
        var service = new AuthService(_db, new FakeJwtTokenGenerator(), _passwordHasher);

        var result = await service.LoginAsync(new LoginRequest("teacher2@test.local", "CorrectPassword1"));

        Assert.Equal("Teacher", result.User.Role);
        Assert.False(string.IsNullOrEmpty(result.Token));
    }
}
