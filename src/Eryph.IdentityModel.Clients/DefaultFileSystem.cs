using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Eryph.IdentityModel.Clients
{
    [ExcludeFromCodeCoverage]
    public class DefaultFileSystem : IFileSystem
    {
        public Stream OpenStream(string filepath)
        {
            return new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        public Stream CreateStream(string filepath)
        {
            return File.Create(filepath);
        }

        public bool FileExists(string filepath)
        {
            return File.Exists(filepath);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }
        
        public void FileDelete(string path)
        {
            File.Delete(path);
        }
    }
}