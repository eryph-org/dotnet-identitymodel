using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;

namespace Haipa.IdentityModel.Clients
{
    [ExcludeFromCodeCoverage]
    public class DefaultEnvironment : IEnvironment
    {
        public bool IsOsPlatform(OSPlatform platform)
        {
            return RuntimeInformation.IsOSPlatform(platform);
        }

        public bool IsWindowsAdminUser
        {
            get
            {
                using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);

            }
        }

        public IFileSystem FileSystem => new DefaultFileSystem();
        public bool IsProcessRunning(int processId)
        {
            return !Process.GetProcessById(processId).HasExited;
        }
    }
}