using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UploadControlCore.Helpers;

namespace UploadControlCore.Models {
    public interface IFileStorage {
        void SaveFile(Guid id, byte[] data, string fileName);
        List<FileInfo> GetFilesByPortfolioId(Guid id);
        List<FileInfo> GetAllFiles();

        void RemoveOldFiles();
    }
    public class MemoryFileStorage : IFileStorage {
        private int timeout = 5;
        private string sessionKey = "dxFileStorage";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;
        public MemoryFileStorage(IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;
            SaveFilesList(new List<FileInfo>());
        }
        public List<FileInfo> GetFilesByPortfolioId(Guid id) {
            return GetAllFiles().Where(f => f.PortfolioId == id).ToList();
        }
        public List<FileInfo> GetAllFiles() {
            return _session.GetObjectFromJson<List<FileInfo>>(sessionKey)?? new List<FileInfo>();
        }
        public void SaveFile(Guid id, byte[] data, string fileName) {
            FileInfo info = new FileInfo { PortfolioId = id, Bytes = data, FileName = fileName, Created = DateTime.Now, Length = data.Length };
            List<FileInfo> list = GetAllFiles();
            list.Add(info);
            SaveFilesList(list);
        }
        private void SaveFilesList(List<FileInfo> files) {
            _session.SetObjectAsJson(sessionKey, files);
        }
        public void RemoveOldFiles() {
            List<FileInfo> list = GetAllFiles();
            var toRemove = list.Where(file => (DateTime.Now - file.Created).TotalMinutes > timeout);
            SaveFilesList(list.Except(toRemove).ToList());
        }
    }

    public class PhysicalFileStorage : IFileStorage {
        private int timeout = 5;
        private string rootDirectory;
        public PhysicalFileStorage(IHostingEnvironment env) {
            rootDirectory = Path.Combine(env.WebRootPath, "Content");
        }
        public List<FileInfo> GetAllFiles() {
            var directories = Directory.GetDirectories(rootDirectory);
            var files = new List<FileInfo>();
            foreach (string directory in directories)
                files.AddRange(
                    Directory.GetFiles(directory).Select(f => new FileInfo {
                        PortfolioId = new Guid(new DirectoryInfo(directory).Name),
                        Created = File.GetCreationTime(f),
                        FileName = Path.GetFileName(f),
                        Length = File.ReadAllBytes(f).Length,
                        Bytes = File.ReadAllBytes(f)
                    }));
            return files;
        }

        public List<FileInfo> GetFilesByPortfolioId(Guid id) {
            return GetAllFiles().Where(f => f.PortfolioId == id).ToList();
        }

        public void RemoveOldFiles() {
            List<FileInfo> list = GetAllFiles();
            var toRemove = list.Where(file => (DateTime.Now - file.Created).TotalMinutes > timeout).ToList();
            toRemove.ForEach(f => File.Delete(Path.Combine(rootDirectory, f.PortfolioId.ToString(), f.FileName)));
            RemoveEmptyDirectories();
        }

        private void RemoveEmptyDirectories() {
            Directory
                .GetDirectories(rootDirectory)
                .Where(d => Directory.GetFiles(d).Length == 0)
                .ToList()
                .ForEach(Directory.Delete);
        }

        public void SaveFile(Guid id, byte[] data, string fileName) {
            FileInfo info = new FileInfo { PortfolioId = id, Bytes = data, FileName = fileName, Created = DateTime.Now, Length = data.Length };
            var directory = Path.Combine(rootDirectory, info.PortfolioId.ToString());
            var fullName = Path.Combine(directory, info.FileName);
            Directory.CreateDirectory(directory);
            File.WriteAllBytes(fullName, info.Bytes);
        }
    }
}
