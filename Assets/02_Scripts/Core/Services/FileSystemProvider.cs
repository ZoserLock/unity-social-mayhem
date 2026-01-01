using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Zen.Debug;

namespace StrangeSpace
{
    public interface IFileSystemProvider
    {
        bool DirectoryExists(string path);
        void CreateDirectory(string data);
        
        bool FileExists(string path);
        bool DeleteFile(string path);
        
        string[] GetFiles(string path);
        string[] GetDirectories(string path);
        
        // Write and read values
        bool ReadAllText(string path, out string text);
        bool WriteAllText(string path, string text);
        
        bool ReadAllBytes(string path, out byte[] bytes);
        bool WriteAllBytes(string path, byte[] bytes);
        
        void EnsurePath(string path);
        
        Task<bool> WriteAllBytesAsync(string sFilenameBin, byte[] encryptedBytes);
        Task<byte[]> ReadAllBytesAsync(string sFilenameBin);
        
        Task<bool> WriteAllTextAsync(string sFilenameBin, string text);
        Task<string> ReadAllTextAsync(string sFilenameBin);
        
    }
    
    public class FileSystemProvider : IFileSystemProvider
    {
        private Dictionary<string, FileSystemWatcher> _directoryWatchers = new Dictionary<string, FileSystemWatcher>();
        
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public bool DeleteFile(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }
            
            try
            {
                File.Delete(path);
            }
            catch (Exception e)
            {
                ZenLog.Error(LogCategory.System, $"[FileSystemProvider] -> DeleteFile: Exception occurred: {e}");
                return false;
            }
            
            return true;
        }

        public string[] GetFiles(string path)
        {
            return Directory.GetFiles(path);
        }

        public string[] GetDirectories(string path)
        {   
            return Directory.GetDirectories(path);
        }

        public bool ReadAllText(string path, out string text)
        {
            try
            {
                text = File.ReadAllText(path);
            }
            catch (Exception e)
            {
                ZenLog.Error(LogCategory.System, $"[FileSystemProvider] -> ReadAllText: Exception occurred: {e}");
                text = string.Empty;
                return false;
            }
     
            return true;
        }

        public bool WriteAllText(string path, string text)
        {
            try
            {
                File.WriteAllText(path, text);
            }
            catch (Exception e)
            {
                ZenLog.Error(LogCategory.System, $"[FileSystemProvider] -> WriteAllText: Exception occurred: {e}");
                return false;
            }
            
            return true;
        }

        public bool ReadAllBytes(string path, out byte[] bytes)
        {
            try
            {
                bytes = File.ReadAllBytes(path);
            }
            catch (Exception e)
            {
                ZenLog.Error(LogCategory.System, $"[FileSystemProvider] -> ReadAllBytes: Exception occurred: {e}");
                bytes = null;
                return false;
            }
            
            return true;
        }

        public bool WriteAllBytes(string path, byte[] bytes)
        {
            try
            {
                File.WriteAllBytes(path, bytes);
            }
            catch (Exception e)
            {
                ZenLog.Error(LogCategory.System, $"[FileSystemProvider] -> WriteAllBytes: Exception occurred: {e}");
                return false;
            }
            
            return true;
        }

        public void EnsurePath(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public async Task<bool> WriteAllBytesAsync(string sFilenameBin, byte[] encryptedBytes)
        {
            try
            {
                await File.WriteAllBytesAsync(sFilenameBin, encryptedBytes);
                return true;
            }
            catch (Exception e)
            {
                ZenLog.Error(LogCategory.System, $"[FileSystemProvider] ->  WriteAllBytesAsync: Exception occurred: {e}");
                return false;
            }
        }

        public async Task<byte[]> ReadAllBytesAsync(string sFilenameBin)
        {
            try
            {
                return await File.ReadAllBytesAsync(sFilenameBin);
            }
            catch (Exception e)
            {
                ZenLog.Error(LogCategory.System, $"[FileSystemProvider] -> ReadAllBytesAsync: Exception occurred: {e}");
                return null;
            }
        }

        public async Task<bool> WriteAllTextAsync(string sFilenameBin, string text)
        {
            try
            {
                await File.WriteAllTextAsync(sFilenameBin, text);
                return true;
            }
            catch (Exception e)
            {
                ZenLog.Error(LogCategory.System, $"[FileSystemProvider] -> WriteAllTextAsync: Exception occurred: {e}");
                return false;
            }
        }

        public async Task<string> ReadAllTextAsync(string sFilenameBin)
        {
            try
            {
                return await File.ReadAllTextAsync(sFilenameBin);
            }
            catch (Exception e)
            {
                ZenLog.Error(LogCategory.System, $"[FileSystemProvider] -> ReadAllTextAsync: Exception occurred: {e}");
                return null;
            }
        }
    }
}