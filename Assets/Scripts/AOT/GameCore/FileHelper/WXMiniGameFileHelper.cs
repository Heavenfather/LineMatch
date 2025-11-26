#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
using WeChatWASM;
using System.IO;
using System.Text;
using UnityEngine;

namespace GameCore.FileHelper
{
    public class WXMiniGameFileHelper : IFileHelper
    {
        private WXFileSystemManager _wxFileSystemManager;

        public string GetCacheFilePath()
        {
            return $"{WX.PluginCachePath}/users/";
        }

        public bool FileExists(string filePath)
        {
            CheckFileSystemInit();
            var result = _wxFileSystemManager.AccessSync(filePath);
            return result == "access:ok";
        }

        public void SaveFile(string filePath, byte[] bytes)
        {
            CheckFileSystemInit();
            if (FileExists(filePath))
            _wxFileSystemManager.RemoveSavedFile(new RemoveSavedFileOption() { filePath = filePath ,complete =
                e =>
                {
                    _wxFileSystemManager.WriteFileSync(filePath, bytes);
                }});
            else
            {
                _wxFileSystemManager.WriteFileSync(filePath, bytes);
            }
        }

        public void DelFile(string filePath)
        {
            CheckFileSystemInit();
            if (FileExists(filePath))
                _wxFileSystemManager.RemoveSavedFile(new RemoveSavedFileOption() { filePath = filePath});
        }

        public byte[] ReadFile(string filePath)
        {
            CheckFileSystemInit();
            if (!FileExists(filePath))
                return null;
            return _wxFileSystemManager.ReadFileSync(filePath);
        }

        public string ReadString(string filePath)
        {
            CheckFileSystemInit();
            if (!FileExists(filePath))
                return null;
            var result = _wxFileSystemManager.ReadFileSync(filePath, "utf8");
            return result;
        }

        public void WriteFile(string filePath, byte[] bytes)
        {
            CheckFileSystemInit();
            string dirPath = Path.GetDirectoryName(filePath);
            MkDir(dirPath);
        }

        public void WriteString(string filePath, string text)
        {
            CheckFileSystemInit();
            string dirPath = Path.GetDirectoryName(filePath);
            MkDir(dirPath);
            _wxFileSystemManager.WriteFileSync(filePath, text);
        }

        public void MkDir(string dirPath)
        {
            CheckFileSystemInit();
            if (!FileExists(dirPath))
            {
                _wxFileSystemManager.MkdirSync(dirPath, true);
            }
        }

        private void CheckFileSystemInit()
        {
            if(_wxFileSystemManager == null) _wxFileSystemManager = WX.GetFileSystemManager();
        }
    }
}
#endif