using System.Net;
using System.Text.Json;
using FluentResults;
using GithubInspector.Models;
using GithubInspector.Requests;

namespace GithubInspector.Services;

public class GithubService
{
    private readonly Uri _baseUri;
    private readonly HttpClient _httpClient;
    private const int MaxResults = 100;
    private const string UserAgent = "User-Agent";

    public GithubService(HttpClient httpClient)
    {
        _baseUri = new("https://api.github.com/");
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _httpClient.DefaultRequestHeaders.Add(UserAgent, "GithubInspector");
    }

    public async Task<Result<List<Commit>>> GetCommits(GithubRequest request, CancellationToken cancel)
    {
        Uri uri = new(_baseUri, $"/repos/{request.Owner}/{request.Repo}/commits?per_page={MaxResults}");
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, uri);
        using var response = await _httpClient
            .SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancel)
            .ConfigureAwait(false);

        return response switch
        {
            { StatusCode: HttpStatusCode.OK } => await ProcessResponse<Commit>(response, cancel),
            _ => Result.Fail(response.ReasonPhrase)
        };
    }

    private static async Task<Result<List<T>>> ProcessResponse<T>(HttpResponseMessage response, CancellationToken cancel)
    {
        await using var contentStream = await response.Content.ReadAsStreamAsync(cancel);

        var payload = await JsonSerializer
            .DeserializeAsync<List<T>>(contentStream, cancellationToken: cancel)
            .ConfigureAwait(false);
        
        return payload is not null 
            ? payload.ToResult()
            : Result.Fail("Unable to parse the response from Github");
    }
}
