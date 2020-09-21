using System.Runtime.InteropServices;

namespace Haipa.IdentityModel.Clients
{
    public class HaipaZeroInfo : LocalIdentityProviderInfo
    {
        public HaipaZeroInfo(IEnvironment environment) : base(environment, "zero")
        {
        }

        protected override bool GetIsRunning()
        {
            return Environment.IsOsPlatform(OSPlatform.Windows) && base.GetIsRunning();
        }
    }
}
