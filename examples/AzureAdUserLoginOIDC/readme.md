# Umbraco Azure AD OIDC example package
This example shows how OpenID Connect can be used for Azure AD managed backoffice users in Umbraco.

# Important files
1. <a href="./Extensions/Options/AzureADBackOfficeExternalProviderOptions.cs" target="_blank">AzureADBackOfficeExternalProviderOptions.cs</a><br />
This file is used to setup the auto link options.
2. <a href="./Extensions/BackOfficeUserAuthenticationExtensions.cs" target="_blank">BackOfficeUserAuthenticationExtensions.cs</a><br />
Extensions used to setup OpenID Connect and the related events.
3. <a href="Program.cs" target="_blank">Program.cs</a><br />
Adding the AzureAD login extension method to the builder setup.
3. <a href="appsettings.json" target="_blank">appsettings.json</a><br />
Configuration example - these values should be added through environment variables or some other means for each environment.

# Azure AD Setup
This example assumes you have knowledge of how to setup an app registration in Azure Portal.
The general requirements are the following:

1. Create an App Registration in Azure Portal to get the ClientId
2. Add a secret (ClientSecret)
3. Copy ClientId and ClientSecret from Azure Portal to your appsettings configuration
4. Go to back to App registration **Authentication** section and insert your redirect URI. Remember to append the **CallbackPath** as defined in your configuration.
5. Check the ID tokens checkbox in authentication and save.
6. Go to **Token configuration**. Add groups claim and choose the option for **Groups assigned to the application**.
7. Add optional claims (ID) and accept the API permissions prompt:
    - email
    - upn
8. Verify the section **API permissions** for Microsoft Graph:
    - email
    - profile
9. Under the **App roles** section, add at least one role to your App Registration. The **Value** is what is important here, since it needs to match the group alias i umbraco. Here is an example:
    - Display name = Admins
    - Allowed member types = Users/Groups
    - Value = admin
    - Description = Admins have the corresponding admin role in umbraco.
10. Go to the corresponding Enterprise App for you app registration
11. Add a User or Group with the designated App role you defined.
12. You can now login with your Azure AD managed user.
