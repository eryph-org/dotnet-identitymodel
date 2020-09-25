using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Haipa.IdentityModel.Clients
{
    [ExcludeFromCodeCoverage]
    public class DefaultFileSystem : IFileSystem
    {
        public TextReader OpenText(string filepath)
        {
            return new StreamReader(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        }

        public TextWriter CreateText(string filepath)
        {
            return File.CreateText(filepath);
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