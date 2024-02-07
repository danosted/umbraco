using System.Security.Claims;
using Umbraco.Cms.Web.BackOffice.Security;
using AzureAdUserLoginOIDC.Extensions.Options;

namespace AzureAdUserLoginOIDC.Extensions;
public struct AzureAdOICDConfig
{
    public string? TenantId { get; init; }
    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }
    public string? CallbackPath { get; init; }
    public string? LoginBtnDisplayName { get; init; }
    public bool IsValid =>
        string.IsNullOrEmpty(TenantId) is false
        && string.IsNullOrEmpty(ClientId) is false
        && string.IsNullOrEmpty(ClientSecret) is false
        && string.IsNullOrEmpty(CallbackPath) is false;
}

public static class BackOfficeUserAuthenticationExtensions
{

    static string GetOpenIdMetaDataAddress(string tenantId)
    {
        return $"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration";
    }

    public static IUmbracoBuilder AddAzureAdAuthentication(this IUmbracoBuilder builder, Func<AzureAdOICDConfig> optionsDelegate)
    {
        // Get the options and validate they are set correctly
        var optionValues = optionsDelegate();

        // If optionvalues are not set correctly, do not add the external login.
        if (optionValues.IsValid is not true) return builder;

        builder.Services.ConfigureOptions<AzureADBackOfficeExternalProviderOptions>();
        builder.AddBackOfficeExternalLogins(logins =>
           {
               logins.AddBackOfficeLogin(
                   userAuthBuilder =>
                   {
                       userAuthBuilder.AddOpenIdConnect(
                              // The scheme must be set with this method to work for the umbraco members
                              GetSchemeName(userAuthBuilder),

                              // fallback to generic login btn text
                              optionValues.LoginBtnDisplayName ?? "Azure AD",
                              options =>
                              {
                                  options.ResponseType = "code";
                                  options.Scope.Add("openid");
                                  options.Scope.Add("profile");
                                  options.Scope.Add("email");

                                  options.MetadataAddress = GetOpenIdMetaDataAddress(optionValues.TenantId!);
                                  options.ClientId = optionValues.ClientId;
                                  options.CallbackPath = optionValues.CallbackPath;
                                  options.ClientSecret = optionValues.ClientSecret;

                                  options.RequireHttpsMetadata = true;
                                  options.SaveTokens = true;
                                  options.TokenValidationParameters.SaveSigninToken = true;

                                  options.Events.OnTokenValidated = async context =>
                                  {
                                      // If context is null we can continue.
                                      if (context == null) throw new InvalidOperationException("Context was null.");

                                      // Fetch claims from Azure AD
                                      var claims = context.Principal?.Claims.ToList();
                                      if (claims is null || !claims.Any()) throw new InvalidOperationException("Did not receive claims.");

                                      // The name claims is required and it is not using the ClaimTypes.Name for some reason.
                                      var name = claims.FirstOrDefault(c => c.Type == "name");
                                      if (name == null) throw new InvalidOperationException("Claims with type 'name' was not received.");

                                      // Add name with correct claimtype
                                      claims?.Add(new Claim(ClaimTypes.Name, name.Value));

                                      // Since we added new claims create a new principal.
                                      var authenticationType = context.Principal?.Identity?.AuthenticationType;
                                      context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType));

                                      // Safe guarding so we do not login with broken identity
                                      // Umbraco requires the Identity.Name to be set through claims.
                                      if (context.Principal?.Identity?.Name == null) throw new InvalidOperationException("Identity.Name is still null.");

                                      // To avoid warning
                                      await Task.FromResult(0);
                                  };
                                  options.Events.OnRedirectToIdentityProviderForSignOut = async notification =>
                                  {
                                      // If you want to use umbraco logout as a global logout in azure, feel free to implement - but this is not in scope for this example.
                                      // To avoid warning
                                      await Task.FromResult(0);
                                  };
                              });
                   });
           });

        string GetSchemeName(BackOfficeAuthenticationBuilder userAuthBuilder)
        {
            var value = userAuthBuilder.SchemeForBackOffice(AzureADBackOfficeExternalProviderOptions.SchemeName);
            if (value is null) throw new InvalidOperationException("Did not get a valid scheme for azure backoffice login.");
            return value;
        }

        return builder;
    }
}