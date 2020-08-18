using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Haipa.IdentityModel.Clients
{
    public interface IFileSystem
    {
        TextReader OpenText(string filepath);
        TextWriter CreateText(string filepath);
        bool FileExists(string infoFilePath);
    }
}
