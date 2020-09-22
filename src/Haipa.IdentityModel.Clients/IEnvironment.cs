using System.Runtime.InteropServices;

namespace Haipa.IdentityModel.Clients
{
    public interface IEnvironment
    {
        bool IsWindowsAdminUser { get; }

        IFileSystem FileSystem { get; }
        bool IsOsPlatform(OSPlatform platform);

        bool IsProcessRunning(string processName, int processId);
    }
}