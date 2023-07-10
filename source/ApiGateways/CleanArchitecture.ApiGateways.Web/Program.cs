using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using Ocelot.Values;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOcelot().AddConsul();
builder.Services.AddSwaggerForOcelot(builder.Configuration,
  (o) =>
  {
      o.GenerateDocsDocsForGatewayItSelf(opt =>
      {
          opt.FilePathsForXmlComments = new string[] { "WebApiGateway.xml" };
          opt.GatewayDocsTitle = "Web ApiGateway";
          opt.GatewayDocsOpenApiInfo = new()
          {
              Title = "Web ApiGateway",
              Version = "v1",
          };

          opt.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
          {
              Type = SecuritySchemeType.OAuth2,
              Flows = new OpenApiOAuthFlows
              {
                  AuthorizationCode = new OpenApiOAuthFlow
                  {
                      AuthorizationUrl = new Uri("http://identity" + "/connect/authorize"),
                      TokenUrl = new Uri("http://identity" + "/connect/token"),
                      Scopes = new Dictionary<string, string>
                                {
                                    { "catalog", "Access read/write operations" },
                                    { "payment", "Access read/write operations" },
                                    { "order", "Access read/write operations" }
                                }
                  }
              }
          });
          opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                            },
                            new[] { "catalog", "payment", "order"}
                        }
                    });
      });
  });

var app = builder.Build();


app.UseHttpsRedirection();

app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
});
app.MapGet("/", () =>
{
    return "ApiGateway Web";
});
await app.UseOcelot();
app.Run();

