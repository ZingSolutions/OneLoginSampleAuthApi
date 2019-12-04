# One Login Sample Auth Api
Sample .NET Core 2.1 API with custom authentication handler to process One Login access tokens generated through a SPA application configured to authenticate via OIDC. 

Also added examples on how to add global error handler and stared to tweak swagger support for self documentation using swashbuckle.

The global error handler logs the error to the configure logger(s) and returns a default error response. The console logging provider has been enabled by default and all other logging providers cleared. To configure the logging providers you want to use see ``Program.cs``.

## Usage
To run this project you will need to set some settings in a new ``OneLogin`` section in your config file. See the below example for details.

```json
"OneLogin": {
    "ClientId": "YOUR_ONELOGIN_APP_CLIENT_ID",
    "ClientSecret": "YOUR_ONELOGIN_APP_CLIENT_SECRET",
    "Domain": "YOUR_ONELOGIN_COMPANY_SUBDOMAIN.onelogin.com"
  }
```

Once set you should be able to run locally.
The homepage/root of the site has been setup to display the Swagger UI documentation.

A couple of controllers have been added to show how the athentication flow works and that the global error handler gets called correctly. 

You should notice that all return codes have been documented using controller/action attributes along with the response object that is to be returned. All errors (400,500,401,403) use the same body to provide a consistant interface.