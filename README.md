# [HealthForms.io](https://healthforms.io) Public API Library
[HealthForms.io](https://healthforms.io) is API driven and we've created a .Net library to facilitate the integration with [HealthForms.io](https://healthforms.io) and external applications. The library is available at [nuget.org](https://www.nuget.org/packages/HealthForms.Api/).

## Build Status
Library Build Status
[![Build Status](https://dev.azure.com/southport/HealthForms/_apis/build/status%2FHealthForms.PublicApi.Library?branchName=main)](https://dev.azure.com/southport/HealthForms/_build/latest?definitionId=44&branchName=main)

Core Build Status
[![Build Status](https://dev.azure.com/southport/HealthForms/_apis/build/status%2FHealthForms.PublicApi.Core?branchName=main)](https://dev.azure.com/southport/HealthForms/_build/latest?definitionId=43&branchName=main)

## Authentications & Authorization

HealthForms API uses OAuth to authenticate and authorize use of the API. Each customer will need to authorize your app. We have created methods in this library to facilitate the authorization. 

### Configuration Items Required (JSON Example)

```
{
    "ClientId": "",
    "ClientSecret": "",
    "RedirectUrl": "",
}
```

authorizing your app to access a [HealthForms.io](https://healthforms.io) tenant will required 3 steps. 

1. Ask the user to get their Tenant ID which is available at https://app.healthforms.io/manage/billing.

1. Using the tenant ID get the redirect URL that will be used to redirect to [HealthForms.io](https://healthforms.io) which will authorize the access to the tenant.
    ```
    HealthFormsApiHttpClient.GetRedirectUrl(tenantID)
    ```
    The redirect URL and the code verification value will be returned. Store the code verification value where the return endpoint can access it, then redirect to the returned URL, the user will be promted to consent to your app accessing their data. After they click Allow, the system will redirect back to your RedirectUrl that was stored in the configuration.

1. When the request returns to your app, it will include an authorization code. Using the authorization code and the code verification value that was generated in the last step, call the **GetTenantToken** method to get the authorization token. When the **GetTenantToken** returns with the token, you will need to store it in a safe place. This token will allow your app to access the tenant. 

## .Net 7, .Net 8

To add the required services for dependency injection call the extension method **AddHealthForms**. This method requires the configuration section that contains the HealthFormsApiOptions definitions.

```
var healthFormsSection = builder.Configuration.GetSection(HealthFormsApiOptions.Key);
builder.Services.AddHealthForms(healthFormsSection);
```

To use the API interface class inject **IHealthFormsApiHttpClient** into the class that needs to use it. 

```
public SampleController(IHealthFormsApiHttpClient healthFormsApiHttpClient){ }
```

Use the available methods to Get, Add, and Delete Session Memebers, Get Session Information, and manage Webhook subscriptions.


## .Net 4.8

This documentation assumes you're not using dependency injection. 

Store the required configuration items **ClientId, ClientSecret, RedirectUrl** with the rest of your configuration. 

Use the **HealthFormsApiHttpClientDisposable** class. This class is designed to manage the disposal of the HttpClient. 

Follow the **Authentications & Authorization** to authenticate a tenant.
