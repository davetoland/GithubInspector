using System.Text.Json;
using FluentResults;
using GithubInspector.Models;
using GithubInspector.Queries;
using GithubInspector.Requests;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace GithubInspector.Pipeline;

public class GetCommitsQueryCachingBehaviour : IPipelineBehavior<GetCommitsQuery, Result<List<Commit>>>
{
     private readonly IDistributedCache _cache;

     private static readonly DistributedCacheEntryOptions CacheOptions = 
        new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) };

     public GetCommitsQueryCachingBehaviour(IDistributedCache cache)
     {
         _cache = cache;
     }

    public async Task<Result<List<Commit>>> Handle(GetCommitsQuery query, RequestHandlerDelegate<Result<List<Commit>>> next, CancellationToken cancel)
    {
        var key = DeriveCacheKey(query.Request);
        var handled = await GetCachedResponse(key, cancel);
        if (handled.IsSuccess)
            return handled.Value;

        var response = await next();
        if (response.IsSuccess)
            await AddResponseToCache(key, response.Value, cancel);

        return response;
    }

    private static string DeriveCacheKey(GithubRequest req) => $"{req.Owner}-{req.Repo}";

    private async Task<Result<List<Commit>>> GetCachedResponse(string key, CancellationToken cancel)
    {
        var cached = await _cache.GetStringAsync(key, token: cancel);
        
        if (cached is null) 
            return Result.Fail($"{key} is not a current cache key.");
        
        var commits = JsonSerializer.Deserialize<List<Commit>>(cached);
        if (commits is not null) 
            return commits.ToResult();
        
        await _cache.RemoveAsync(key, cancel);
        return Result.Fail($"{key} had a corrupted cache key, value removed.");
    }

    private async Task AddResponseToCache(string key, List<Commit> commits, CancellationToken cancel)
        =>  await _cache.SetAsync(key, JsonSerializer.SerializeToUtf8Bytes(commits), CacheOptions, cancel);
}
