using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using Haipa.IdentityModel.Clients.Internal;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace Haipa.IdentityModel.Clients
{
    [PublicAPI]
    public sealed class ClientLookup
    {
        private readonly IEnvironment _systemEnvironment;

        public ClientLookup(IEnvironment systemEnvironment)
        {
            _systemEnvironment = systemEnvironment;
        }

        [ExcludeFromCodeCoverage]
        public ClientData GetClient()
        {
            throw new NotImplementedException();
        }

        public ClientData GetSystemClient()
        {
            if (!_systemEnvironment.IsOsPlatform(OSPlatform.Windows) &&
                !_systemEnvironment.IsOsPlatform(OSPlatform.Linux))
                throw new InvalidOperationException("The system client exists only on Windows and Linux systems.");

            if (_systemEnvironment.IsOsPlatform(OSPlatform.Windows) && !_systemEnvironment.IsWindowsAdminUser)
                throw new InvalidOperationException(
                    "This application has to be started as admin to access the Haipa system client. ");

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
                return new ClientData("system-client",
                    privateKey,
                    new Uri(new Uri(endpoint),
                        "identity"));

            return new ClientData("system-client",
                privateKey,
                new Uri(endpoint));
        }

        public string GetLocalIdentityEndpoint(bool forHaipaZero = false)
        {
            var applicationDataPath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "haipa");

            var moduleName = forHaipaZero ? "zero" : "identity";
            var infoFilePath = Path.Combine(Path.Combine(applicationDataPath,
                $@"{moduleName}{Path.DirectorySeparatorChar}.run_info"));

            JObject processInfo;
            try
            {
                using var reader = _systemEnvironment.FileSystem.OpenText(infoFilePath);
                var processInfoData = reader.ReadToEnd();
                processInfo = JObject.Parse(processInfoData);
            }
            catch
            {
                if (forHaipaZero)
                    throw new InvalidOperationException("process info for haipa zero not found.");

                throw new InvalidOperationException("process info for haipa identity not found.");
            }

            var processId = processInfo["process_id"];

            if (_systemEnvironment.IsProcessRunning(processId.ToObject<int>())) return processInfo["url"].ToString();


            if (forHaipaZero)
                throw new InvalidOperationException("Haipa zero is not running.");

            throw new InvalidOperationException("Haipa identity is not running.");

        }

    }
}