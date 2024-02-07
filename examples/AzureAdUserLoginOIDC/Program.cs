using AzureAdUserLoginOIDC.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .AddAzureAdAuthentication(() =>
    {
        return new AzureAdOICDConfig
        {
            ClientId = builder.Configuration.GetSection("AzureADOpenIdConnect").GetValue<string>("ClientId"),
            ClientSecret = builder.Configuration.GetSection("AzureADOpenIdConnect").GetValue<string>("ClientSecret"),
            TenantId = builder.Configuration.GetSection("AzureADOpenIdConnect").GetValue<string>("TenantId"),
            CallbackPath = builder.Configuration.GetSection("AzureADOpenIdConnect").GetValue<string>("CallbackPath"),
            LoginBtnDisplayName = builder.Configuration.GetSection("AzureADOpenIdConnect").GetValue<string>("LoginBtnDisplayName"),
        };
    })
    .Build();

WebApplication app = builder.Build();

await app.BootUmbracoAsync();


app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseInstallerEndpoints();
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();
