﻿using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;

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
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public IFileSystem FileSystem => new DefaultFileSystem();

        public bool IsProcessRunning(string processName, int processId)
        {
            var processesWithName = Process.GetProcessesByName(processName);

            return processesWithName.Any(x => !x.HasExited && x.Id == processId);
        }
    }
}