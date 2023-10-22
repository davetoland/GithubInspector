using FluentResults;
using FluentValidation;
using GithubInspector.Endpoints;
using GithubInspector.Models;
using GithubInspector.Pipeline;
using GithubInspector.Queries;
using GithubInspector.Services;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient<GithubService>();
builder.Services.AddTransient<OutputFormatter>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddTransient<IPipelineBehavior<GetCommitsQuery, Result<List<Commit>>>, GetCommitsQueryCachingBehaviour>();

builder.Services.AddStackExchangeRedisCache(x =>
{
    x.Configuration = "redis:6379";
    x.InstanceName = "github-inspector";
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapGithubEndpoint();

app.Run();