using System.IO;
using System.Text;
using UnityEngine;

namespace GameCore.FileHelper
{
    /// <summary>
    /// 原生应用的文件操作类
    /// </summary>
    public class NativeFileHelper : IFileHelper
    {
        public string GetCacheFilePath()
        {
            return Application.persistentDataPath;
        }

        public bool FileExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            return true;
        }

        public void SaveFile(string filePath, byte[] bytes)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            fs.Write(bytes, 0, bytes.Length);
            fs.Flush();
            fs.Close();
        }

        public void DelFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public byte[] ReadFile(string filePath)
        {
            using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            int length = (int)fs.Length;
            byte[] bytes = new byte[length];
            fs.Read(bytes, 0, bytes.Length);
            fs.Close();
            return bytes;
        }

        public string ReadString(string filePath)
        {
            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        public void WriteFile(string filePath, byte[] bytes)
        {
            string dirPath = Path.GetDirectoryName(filePath);
            MkDir(dirPath);
            
            File.WriteAllBytes(filePath, bytes);
        }

        public void WriteString(string filePath, string text)
        {
            string dirPath = Path.GetDirectoryName(filePath);
            MkDir(dirPath);
            
            File.WriteAllText(filePath, text, Encoding.UTF8);
        }

        public void MkDir(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }
    }
}