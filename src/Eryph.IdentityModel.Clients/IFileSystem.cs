using System.IO;

namespace Eryph.IdentityModel.Clients
{
    public interface IFileSystem
    {
        TextReader OpenText(string filepath);
        TextWriter CreateText(string filepath);
        bool FileExists(string infoFilePath);
        bool DirectoryExists(string path);
        void CreateDirectory(string path);

        void FileDelete(string path);
    }
}