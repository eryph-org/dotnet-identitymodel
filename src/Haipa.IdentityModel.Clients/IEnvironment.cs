using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;

namespace Haipa.IdentityModel.Clients
{
    public interface IEnvironment
    {
        bool IsOsPlatform(OSPlatform platform);
        bool IsWindowsAdminUser { get;  }

        IFileSystem FileSystem { get;  }

        bool IsProcessRunning(int processId);
    }
}