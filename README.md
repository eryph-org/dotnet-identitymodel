# Haipa Identity Model
Haipa .NET library for claims-based identity and client generation


## Description

**Features**:
- HttpClient extensions to retrieve access token from Haipa identity server
- Haipa client discovery 
- Haipa client key and certificate generator

## Platforms & Prerequisites

**.NET**

The library requires .NET Standard 2.0 or higher. 

All platforms and runtimes (.NET Framework / .NET Core / all .NET supported operating systems) are supported.


## Getting started

The easiest way to get started is by installing the available NuGet package [Haipa.IdentityModel](https://www.nuget.org/packages/Haipa.IdentityModel) and/or [Haipa.IdentityModel.Client](https://www.nuget.org/packages/Haipa.IdentityModel.Client). Take a look at the [Using](#using) section learning how to configure. 


## Using

This sample shows how to lookup a client from the current system and requests an access token from the haipa identity service. You will have to add a reference to both nuget packages for this example.

```csharp

//the client lookup searches for a valid client. 
var clientLockup = new ClientLookup(new DefaultEnvironment());
var result = clientLockup.GetClient();


var httpClient = new HttpClient{ BaseAddress = new Uri(result.IdentityEndpoint) };

//request access token from the identity endpoint
var token = await httpClient.GetClientAccessToken(
        result.Client.ClientName, 
        result.Client.KeyPair.ToRSAParameters())
    .ConfigureAwait(false);


```



## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/haipa/dotnet-identitymodel/tags). 

## Authors

* **Frank Wagner** - *Initial work* - [fw2568](https://github.com/fw2568)

See also the list of [contributors](https://github.com/haipa/dotnet-identitymodel/contributors) who participated in this project.


## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details
