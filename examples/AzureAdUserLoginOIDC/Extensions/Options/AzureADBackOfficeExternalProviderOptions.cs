using System.Security.Claims;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.BackOffice.Security;

namespace AzureAdUserLoginOIDC.Extensions.Options;

public class AzureADBackOfficeExternalProviderOptions() : IConfigureNamedOptions<BackOfficeExternalLoginProviderOptions>
{
    public const string SchemeName = "AzureAD";

    public void Configure(string? name, BackOfficeExternalLoginProviderOptions options)
    {
        if (name != Constants.Security.BackOfficeExternalAuthenticationTypePrefix + SchemeName)
        {
            return;
        }

        Configure(options);
    }

    public void Configure(BackOfficeExternalLoginProviderOptions options)
    {
        // Change this if you want another icon on the login button
        options.Icon = "icon-cloud";

        // The following options are relevant if you
        // want to configure auto-linking on the authentication.
        options.AutoLinkOptions = new ExternalSignInAutoLinkOptions(

            // Set to true to enable auto-linking
            autoLinkExternalAccount: true,

            // [OPTIONAL]
            // Default: The culture specified in appsettings.json.
            // Specify the default culture to create the Member as.
            // It can be dynamically assigned in the OnAutoLinking callback.
            defaultCulture: null,

            // [OPTIONAL]
            // Default: "Editor"
            // Specify User Group.
            defaultUserGroups: []

        )
        {
            // [OPTIONAL] Callbacks
            OnAutoLinking = (autoLinkUser, loginInfo) =>
            {
            },
            OnExternalLogin = (user, loginInfo) =>
            {
                // Use roles defined and assigned in the app registration role section to keep access mangement in azure.
                // We can simply use 1 : 1 mapping, by matching the default umbraco role aliases  defined in the contants:
                // Constants.Security.WriterGroupAlias
                // Constants.Security.EditorGroupAlias
                // Constants.Security.AdminGroupAlias
                // ...
                var roleId = loginInfo.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                var email = loginInfo.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var name = loginInfo.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                // Check if the user has been assigned access through azure
                if (roleId is null || email is null || name is null)
                {
                    // User fields are not persisted when login is disabled per v13.1.0
                    // Thus we cannot disable or do anything clever like that on the user object
                    // it will need to be carried out manually in umbraco backoffice

                    // Returning false to disallow login, since we are missing claims
                    return false;
                }

                // Reset roles first to remove any previously assigned roles that might no longer be assigned in azure
                user.Roles = [];

                // Add the designated role returned by claims
                user.AddRole(roleId);

                // Approve the user (this cannot be set to false again - a user needs to be manually cleaned up in the backoffice)
                user.IsApproved = true;

                // Set user fields just in case - these might have been editted at some point
                user.Email = email;
                user.UserName = email;
                user.Name = name;

                // Returns a boolean indicating if sign-in should continue or not.
                return true;
            },
        };

        // [OPTIONAL]
        // Disable the ability for users to login with a username/password.
        // If set to true, it will disable username/password login
        // even if there are other external login providers installed.
        options.DenyLocalLogin = false;

        // [OPTIONAL]
        // Choose to automatically redirect to the external login provider
        // effectively removing the login button.
        options.AutoRedirectLoginToExternalProvider = false;
    }
}