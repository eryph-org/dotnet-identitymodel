using System.IO;

namespace Eryph.IdentityModel.Clients
{
    public interface IFileSystem
    {

        Stream OpenStream(string filepath);
        Stream CreateStream(string filepath);

        bool FileExists(string infoFilePath);
        bool DirectoryExists(string path);
        void CreateDirectory(string path);

        void FileDelete(string path);
    }
}