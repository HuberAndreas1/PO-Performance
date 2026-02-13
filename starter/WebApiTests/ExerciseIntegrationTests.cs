using System.Net;
using System.Net.Http.Json;

namespace WebApiTests;

public class ExerciseIntegrationTests(WebApiTestFixture fixture) : IClassFixture<WebApiTestFixture>
{
    [Fact]
    public async Task GetExercises_ReturnsOkAndJsonArray()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/api/exercises");

        // Assert — endpoint exists and returns 200 OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var contentType = response.Content.Headers.ContentType?.MediaType;
        Assert.Equal("application/json", contentType);
    }

    [Fact]
    public async Task GetExerciseHistory_NonExistentId_ReturnsOkOrNotFound()
    {
        // Act — request history for an exercise that doesn't exist
        var response = await fixture.HttpClient.GetAsync("/api/exercises/999/history");

        // Assert — endpoint exists and returns either 200 (empty list) or 404
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound,
            $"Expected OK or NotFound but got {response.StatusCode}");
    }

    // TODO: Add a third integration test here.
    //   Suggestion: Seed the database with test data, then verify that
    //   GET /api/exercises returns the correct ExerciseOverviewDto structure
    //   with the expected field values (Id, Name, Current1RM, etc.).
}
