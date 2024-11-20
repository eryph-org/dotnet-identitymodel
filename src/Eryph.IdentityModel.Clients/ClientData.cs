using JetBrains.Annotations;

namespace Eryph.IdentityModel.Clients;

[PublicAPI]
public sealed class ClientData(string id, string name)
{
    public string Id { get; } = id;

    public string Name { get; } = name;
}
