using FluentValidation;
using GithubInspector.Endpoints;
using GithubInspector.Services;
using GithubInspector.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient<GithubService>();
builder.Services.AddSingleton<OutputFormatter>();
builder.Services.AddMediatR(x => x.RegisterServicesFromAssemblyContaining(typeof(Program)));
builder.Services.AddValidatorsFromAssemblyContaining<GithubRequestValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGithubEndpoint();

app.Run();