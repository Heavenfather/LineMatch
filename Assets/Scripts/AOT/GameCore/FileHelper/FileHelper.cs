using System.Runtime.CompilerServices;

namespace GameCore.FileHelper
{
    public static class FileHelper
    {
        private static IFileHelper _fileHelper;

        /// <summary>
        /// 获取游戏缓存目录
        /// </summary>
        /// <returns></returns>
        public static string GetCacheFilePath()
        {
            PrepareFileHelper();
            return _fileHelper.GetCacheFilePath();
        }
        
        /// <summary>
        /// 文件是否存在
        /// 特殊的:在微信小游戏和抖音小游戏平台下也可以判断目录是否存在
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool FileExists(string filePath)
        {
            PrepareFileHelper();
            return _fileHelper.FileExists(filePath);
        }
        
        /// <summary>
        /// 保存数据到指定文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="bytes"></param>
        public static void SaveFile(string filePath, byte[] bytes)
        {
            PrepareFileHelper();
            _fileHelper.SaveFile(filePath, bytes);
        }
        
        /// <summary>
        /// 删除指定文件
        /// </summary>
        /// <param name="filePath"></param>
        public static void DelFile(string filePath)
        {
            PrepareFileHelper();
            _fileHelper.DelFile(filePath);
        }
        
        /// <summary>
        /// 读取文件内容
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static byte[] ReadFile(string filePath)
        {
            PrepareFileHelper();
            return _fileHelper.ReadFile(filePath);
        }
        
        /// <summary>
        /// 读取文件内容
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ReadString(string filePath)
        {
            PrepareFileHelper();
            return _fileHelper.ReadString(filePath);
        }

        /// <summary>
        /// 写入文件内容
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="bytes"></param>
        public static void WriteFile(string filePath, byte[] bytes)
        {
            PrepareFileHelper();
            _fileHelper.WriteFile(filePath, bytes);
        }

        /// <summary>
        /// 写入文件内容
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="text"></param>
        public static void WriteString(string filePath, string text)
        {
            PrepareFileHelper();
            _fileHelper.WriteString(filePath, text);
        }
        
        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="dirPath"></param>
        public static void MkDir(string dirPath)
        {
            PrepareFileHelper();
            _fileHelper.MkDir(dirPath);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PrepareFileHelper()
        {
            if (_fileHelper != null)
                return;
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
            _fileHelper = new WXMiniGameFileHelper();
#elif UNITY_WEBGL && DOUYINMINIGAME && !UNITY_EDITOR
            _fileHelper = new TTMiniGameFileHelper();
#else
            _fileHelper = new NativeFileHelper();
#endif
        }
    }
}