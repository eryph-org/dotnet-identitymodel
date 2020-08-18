using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using Haipa.IdentityModel.Clients.Internal;
using Newtonsoft.Json.Linq;

namespace Haipa.IdentityModel.Clients
{
    public class ClientLookup
    {
        private readonly IEnvironment _systemEnvironment;

        public ClientLookup(IEnvironment systemEnvironment)
        {
            _systemEnvironment = systemEnvironment;
        }

        [ExcludeFromCodeCoverage]
        public ClientLookupResult GetClient()
        {
            return GetSystemClient();
        }

        public ClientLookupResult GetSystemClient()
        {
            if (!_systemEnvironment.IsOsPlatform(OSPlatform.Windows) &&
                !_systemEnvironment.IsOsPlatform(OSPlatform.Linux))
            {
                throw new InvalidOperationException("The system client exists only on Windows and Linux systems.");
            }

            if (_systemEnvironment.IsOsPlatform(OSPlatform.Windows) && !_systemEnvironment.IsWindowsAdminUser)
            {
                throw new InvalidOperationException("This application has to be started as admin to access the Haipa system client. ");
            }
            
            var applicationDataPath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "haipa");

            var privateKeyPath = Path.Combine(applicationDataPath,
                $@"identity{Path.DirectorySeparatorChar}private{Path.DirectorySeparatorChar}clients{Path.DirectorySeparatorChar}system-client.key");

            var privateKey = PrivateKeyFile.Read(privateKeyPath, _systemEnvironment.FileSystem);

            var isHaipaZero = false;
            string endpoint;
            try
            {
                endpoint = GetLocalIdentityEndpoint();
            }
            catch (InvalidOperationException)
            {
                endpoint = GetLocalIdentityEndpoint(true);
                isHaipaZero = true;
            }

            if (isHaipaZero)
            {
                return new ClientLookupResult
                {
                    IsHaipaZero = true,
                    ApiEndpoint = new Uri(new Uri(endpoint), "api").ToString(),
                    IdentityEndpoint = new Uri(new Uri(endpoint), "identity").ToString(),
                    Client = new ClientData("system-client", privateKey)
                };
            }

            return new ClientLookupResult
            {
                IsHaipaZero = false,
                ApiEndpoint = null,
                IdentityEndpoint = endpoint,
                Client = new ClientData("system-client", privateKey)
            };
        }

        public string GetLocalIdentityEndpoint(bool forHaipaZero=false)
        {
            
            var applicationDataPath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "haipa");

            var moduleName = forHaipaZero ? "zero" : "identity";
            var infoFilePath = Path.Combine(Path.Combine(applicationDataPath, $@"{moduleName}{Path.DirectorySeparatorChar}.run_info"));

            JObject processInfo;
            try
            {
                using var reader = _systemEnvironment.FileSystem.OpenText(infoFilePath);
                var processInfoData = reader.ReadToEnd();
                processInfo = JObject.Parse(processInfoData);
            }
            catch
            {
                if(forHaipaZero)
                    throw new InvalidOperationException("process info for haipa zero not found.");

                throw new InvalidOperationException("process info for haipa identity not found.");

            }

            var processId = processInfo["process_id"];

            if (_systemEnvironment.IsProcessRunning(processId.ToObject<int>()))
            {
               return processInfo["url"].ToString();
            }


            if (forHaipaZero)
                throw new InvalidOperationException("Haipa zero is not running.");

            throw new InvalidOperationException("Haipa identity is not running.");

        }

    }
}