using System.IO;

namespace Haipa.IdentityModel.Clients
{
    public interface IFileSystem
    {
        TextReader OpenText(string filepath);
        TextWriter CreateText(string filepath);
        bool FileExists(string infoFilePath);
    }
}