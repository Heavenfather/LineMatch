#if UNITY_WEBGL && DOUYINMINIGAME && !UNITY_EDITOR
using UnityEngine;
using System.IO;
using System.Text;
using TTSDK;

namespace GameCore.FileHelper
{
    public class TTMiniGameFileHelper : IFileHelper
    {
        private TTFileSystemManager _ttFileSystemManager;

        public string GetCacheFilePath()
        {
            return Application.persistentDataPath;
        }

        public bool FileExists(string filePath)
        {
            CheckFileSystemInit();
            var result = _ttFileSystemManager.AccessSync(filePath);
            return result;
        }

        public void SaveFile(string filePath, byte[] bytes)
        {
            if (FileExists(filePath))
            {
                _ttFileSystemManager.Unlink(new UnlinkParam()
                {
                    filePath = filePath,
                    success = (rsp) =>
                    {
                        _ttFileSystemManager.WriteFileSync(filePath, datas);
                    }
                });
            }
            else
            {
                _ttFileSystemManager.WriteFileSync(filePath, datas);
            }
        }

        public void DelFile(string filePath)
        {
            CheckFileSystemInit();
            if (FileExists(filePath))
            {
                _ttFileSystemManager.Unlink(new UnlinkParam() { filePath = filePath });
            }
        }

        public byte[] ReadFile(string filePath)
        {
            CheckFileSystemInit();
            if (!FileExists(filePath))
                return null;
            return _ttFileSystemManager.ReadFileSync(filePath);
        }

        public string ReadString(string filePath)
        {
            CheckFileSystemInit();
            if (!FileExists(filePath))
                return null;
            return _ttFileSystemManager.ReadFileSync(filePath, "utf8");
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
            _ttFileSystemManager.WriteFileSync(filePath, text);
        }

        public void MkDir(string dirPath)
        {
            CheckFileSystemInit();
            if (!FileExists(path))
            {
                _ttFileSystemManager.MkdirSync(dirPath, true);
            }
        }

        private void CheckFileSystemInit()
        {
            if(_ttFileSystemManager == null) _ttFileSystemManager = TT.GetFileSystemManager();
        }
    }
}
#endif