using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Haipa.IdentityModel.Clients
{
    [ExcludeFromCodeCoverage]
    internal class DefaultFileSystem : IFileSystem
    {
        public TextReader OpenText(string filepath)
        {
            return File.OpenText(filepath);
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

        public string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }
    }
}