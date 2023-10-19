using FluentValidation;
using GithubInspector.Endpoints;
using GithubInspector.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient<GithubService>();
builder.Services.AddSingleton<OutputFormatter>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddMediatR(x => 
    x.RegisterServicesFromAssemblyContaining(typeof(Program)));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapGithubEndpoint();

app.Run();