# Eryph Identity Model
Eryph .NET library for claims-based identity and client authentication


[![Build Status](https://dev.azure.com/dbosoft/public/_apis/build/status/eryph-org.dotnet-identitymodel?branchName=main)](https://dev.azure.com/dbosoft/public/_build/latest?definitionId=31&branchName=main)

## Description

**Features**:
- HttpClient extensions to retrieve access token from Eryph identity server (Eryph.IdentityModel)
- common types for Eryph clients (Eryph.IdentityModel.Clients)
- private key handling for Eryph clients (Eryph.IdentityModel.Clients)

## Platforms & Prerequisites

**.NET**

The library requires .NET Standard 2.0 or higher. 

All platforms and runtimes (.NET Framework / .NET Core / all .NET supported operating systems) are supported.


## Getting started

The packages of this library (**Eryph.IdentityModel** and **Eryph.IdentityModel.Client**) are currently only available as CI build on the dbosoft public nuget feed:

https://dev.azure.com/dbosoft/public/_packaging?_a=feed&feed=Public


Take a look at the [Using](#using) section learning how to configure. 


## Using

This sample shows how to lookup a client from the current system and requests an access token from the eryph identity service. You will have to add a reference to the Eryph.ClientRuntime.Configuration nuget package to use this example.

```csharp

//the client lookup searches for a valid client. 
var lookup = new ClientCredentialsLookup(new DefaultEnvironment());
var credentials = lookup.FindCredentials();

//request access token from the identity endpoint
var token = await credentials.GetAccessToken();


```



## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/eryph-org/dotnet-identitymodel/tags). 

## Authors

* **Frank Wagner** - *Initial work* - [fw2568](https://github.com/fw2568)

See also the list of [contributors](https://github.com/eryph-org/dotnet-identitymodel/contributors) who participated in this project.


## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details
