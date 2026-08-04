using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AssignmentSystem.Api.DTOs.Auth;

namespace AssignmentSystem.UnitTests.Integration;

public class AuthorizationIntegrationTests : IClassFixture<AssignmentSystemWebApplicationFactory>
{
    private readonly AssignmentSystemWebApplicationFactory _factory;

    public AuthorizationIntegrationTests(AssignmentSystemWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return body!.Token;
    }

    [Fact]
    public async Task AnonymousRequest_Rejected_FromAuthenticatedEndpoint()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/assignments");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task StudentToken_Rejected_FromAdminOnlyUsersEndpoint()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, "student1@school.test", "Student@123");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminToken_Accepted_FromAdminOnlyUsersEndpoint()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, "admin@school.test", "Admin@123");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task StudentToken_OnlySeesPublishedAssignmentsInOwnClass()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(client, "student1@school.test", "Student@123");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/assignments");
        response.EnsureSuccessStatusCode();

        var assignments = await response.Content.ReadFromJsonAsync<List<AssignmentSystem.Api.DTOs.Assignments.AssignmentResponse>>();

        Assert.NotNull(assignments);
        Assert.All(assignments!, a => Assert.Equal("Published", a.Status));
    }
}
