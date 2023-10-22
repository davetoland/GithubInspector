# GithubInspector

# Notes
My first time actually using MediatR for a project, even though I've been aware of it for some time.
Very much like the producer/consumer model in MassTransit (https://masstransit.io/documentation/concepts/consumers), so feels comfortable already.
I'm sure I'm just scratching the surface of what's possible though..

# Features
### Minimal API
I wondered whether I should use a Minimal API for this, as it's probably not what you use in production, however it is a tool for small APIs and that's what this is.
Underneath it's doing basically the same thing anyway, and so we're really just looking at new syntax here, but I like the way we can now create our own structures that represent endpoints, loaded in through DI.
The nice benefit this brings is the ability to have the DI container inject only the dependencies required by a particular method, and the clean syntax you can use (via method groups) to automatically forward the mapped request on such methods. 
It would be the same logic to create traditional controllers, which I've been doing forever anyway, this is a really succinct new approach though.
```csharp
public static class GithubEndpoint
{
    public static void MapGithubEndpoint(this WebApplication app)
    {
        app.MapGet("/api/v1/{owner}/{repo}/contributors", GetCommits);
    }

    private static async Task<IResult> GetCommits(
        [AsParameters] GithubRequest request, 
        IMediator mediator,
        CancellationToken cancel)
    {
        var validation = await mediator.Send(new ValidateRequestQuery(request), cancel);
        if (!validation.IsValid)
            return Results.BadRequest(validation.Errors.Select(x => x.ErrorMessage));

        var response = await mediator.Send(new GetCommitsQuery(request), cancel);
        if (response.IsFailed)
            return Results.NotFound(response.Reasons.Single().Message);

        var commits = response.Value;
        if (!commits.Any())
            return Results.NoContent();

        var formatted = await mediator.Send(new FormatCommitsQuery(commits), cancel);

        return Results.Ok(formatted);
    }        
}
```


### Asynchronous deserialisation of JSON stream data
There's been various blog posts about this, but it's a really nice feature of the new JSON library from the dotnet team. 
When the response is returned from the Github API, by calling ```ReadAsStreamAsync``` on the HttpContent of the response, and using ```JsonSerializer.DeserializeAsync```, it's possible to deserialise the JSON directly from the stream, without buffering into memory or preloading.
An important point to remember in this is that the because we're reading the stream asynchronously, we need to ```await using ...``` the stream, as it'll be an IAsyncDispoable instance that we have:

```csharp GithubService.cs
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
```

### FluentResult return types
I'm not a fan of exceptions on the whole (of course, with exception) and I think they are unnecesarily overused where other more suitable options exist. If we're simply returning a failure/error result, most of the time we don't actually need to break the control flow (and the call stack) to do so. In my opinion it makes debugging more difficult, and it makes logs harder to follow. 
I've been experimenting with libraries that return a _result_ that encapsulates this _value-or-fail_ approach, and here I'm using FluentAssertions for that, which allows me to return data wrapped in a "success" (it has a nice ToResult extension method to accomplish this from any type) and a Result.Fail method which can wrap errors or a failure message.
I'm also not really a fan of using nullables and null as a return type either.
By not throwing exceptions we're improving the performance of the application as well, and I think returning typed Result instances improves the debugging experience for developers following the flow, especially when things don't follow the happy path.

### Record struct models
By using record structs for models we're getting an immutable value type (albeit containing reference types, but still less allocation), that's nice and concise and easy to generate (i.e. write the code).
There's no point using classes for these models, and no point having Properties with setters to hold the data. It is, after all, immutable.

### Pattern matching and Ranges
I didn't find that many chances to use the new ever increasing number of pattern matching techniques now available in dotnet, but you'll find a few instances of them:
```csharp
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
```

```csharp
public static List<string> FormatCommits(IEnumerable<Commit> commits)
    => commits.Select(x => 
        $"[{x.Details.Author.Date:yyyy-MM-dd}] " +
        $"{x.Details.Author.Name} | " + // ({x.Details.Author.Email}): " +
        $"{(x.Details.Message.Length > 120 ? x.Details.Message[..117]+"..." : x.Details.Message)}").ToList();
```

### Top 100/30? results
There was some ambiguity in the requirements, first asking for the last 100 commits, then in the AC stating 30. I wondered if this was a bit of a curve ball, as by default Github returns a default page size of 30, and an extra Url parameter is required to force this to 100.
I've added that, but the MaxResults in the GithubService.cs class will allow for it to be set to 30, or whatever you wish (I haven't taken into account the actual maximum page size Github allows, so test your luck!).

# Foot notes
Often these things are a bit of a bore, or worse, but this was fun. I enjoyed putting this together, and I feel like it's very much my bread and butter at the moment. 
Most of the recent work has been API related, microservices, pub/sub messaging, command and event type stuff. Using a mixture of Azure Service Bus, RabbitMQ, microservices, clean/onion architecure, etc.
I've not used MediatR before as I say, but it's nice, and feels very similar to what I've used elsewhere.

I hope you like my submission, I gave myself a 2-hour timebox to do this, and I've overrun by about 45 mins now ...mostly typing this! :-)

## What could I do with more time?
The Formatter/FormatService is garbage, I really only added it as another mediator example. 
I think perhaps something like a Redis cache, with say a 1 hour sliding window, in it's place would work well.
The initial request could go to the cache, and if it doesn't have a corresponding key (i.e. the api request path with owner/repo) then it would mediate a further request to the Github service to fetch the data from the API, caching that returned response.

For an example of Redis caching that I did on a project recently, see the following repo.
It uses a slightly different mechanism whereby a DelegatingHandler is injected into the Http pipeline, and that intercepts calls to the HttpClient, and checks/updates a cache but it's a similar concept:
https://github.com/davetoland/F1Api


# UPDATE!!

I found a bit of time on Sunday afternoon to sit down and have another look at this. Over the weekend I read about MediatR's pipeline behaviour system, which is essentially the same as a middleware or http pipeline, and in light of what I wrote above I decided I'd implement a Redis cache (albeit with a _fixed_ 1 hour expiration, rather than _sliding_). I've added an `IPipelineBehavior` class to intercept the call to the GithubService, this firtly checks the cache, using the owner and repo as a key in redis. If it finds a entry, that gets returned. If not, the next() delegate is executed and a successul result from that is then added to the cache for next time before being returned back along the pipeline.


