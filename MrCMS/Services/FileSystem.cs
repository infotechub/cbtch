using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
using MrCMS.Website;

namespace MrCMS.Services
{
    public class FileSystem : IFileSystem
    {
        private const string _mediaDirectory = "/content/upload/";
        public string MediaDirectory { get { return _mediaDirectory; } }

        public string SaveFile(Stream stream, string filePath, string contentType)
        {
            string path = GetPath(filePath);

            FileInfo fileInfo = new FileInfo(path);
            DirectoryInfo directoryInfo = fileInfo.Directory;

            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            using (Stream file = File.OpenWrite(path))
            {
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }
                stream.CopyTo(file);
            }
            stream.Close();
            stream.Dispose();

            string relativeFilePath = GetRelativeFilePath(filePath);
            return relativeFilePath;
        }

        private string GetPath(string relativeFilePath)
        {
            relativeFilePath = GetRelativeFilePath(relativeFilePath);
            string baseDirectory = ApplicationPath.Substring(0, ApplicationPath.Length - 1);
            string path = Path.Combine(baseDirectory, relativeFilePath.Substring(1));
            return path;
        }

        private static string GetRelativeFilePath(string relativeFilePath)
        {
            if (!relativeFilePath.StartsWith(_mediaDirectory) && !relativeFilePath.StartsWith("/" + _mediaDirectory))
                relativeFilePath = Path.Combine(_mediaDirectory, relativeFilePath);
            return relativeFilePath;
        }

        public void CreateDirectory(string filePath)
        {
            string path = GetPath(filePath);

            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists)
                directoryInfo.Create();
        }

        public void Delete(string filePath)
        {
            string path = GetPath(filePath);

            if (Directory.Exists(path))
                Directory.Delete(path, true);
            else
                File.Delete(path);
        }

        public bool Exists(string filePath)
        {
            return File.Exists(GetPath(filePath));
        }

        public string ApplicationPath { get { return HostingEnvironment.ApplicationPhysicalPath; } }


        public byte[] ReadAllBytes(string filePath) { return File.ReadAllBytes(GetPath(filePath)); }

        public Stream GetReadStream(string filePath)
        {
            string path = GetPath(filePath);

            return File.OpenRead(path);
        }

        public void WriteToStream(string filePath, Stream stream)
        {
            string path = GetPath(filePath);
            using (FileStream fileStream = File.OpenRead(path))
            {
                fileStream.CopyTo(stream);
            }
        }

        public IEnumerable<string> GetFiles(string filePath)
        {
            string path = GetPath(filePath);

            if (Directory.Exists(path))
            {
                foreach (string file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                    yield return "/" + file.Remove(0, ApplicationPath.Length).Replace('\\', '/');
            }
        }
    }
}