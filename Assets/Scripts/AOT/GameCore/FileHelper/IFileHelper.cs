namespace GameCore.FileHelper
{
    public interface IFileHelper
    {
        /// <summary>
        /// 获取游戏缓存目录
        /// </summary>
        /// <returns></returns>
        string GetCacheFilePath();
        
        /// <summary>
        /// 文件是否存在
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        bool FileExists(string filePath);
        
        /// <summary>
        /// 保存数据到指定文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="bytes"></param>
        void SaveFile(string filePath, byte[] bytes);

        /// <summary>
        /// 删除指定文件
        /// </summary>
        /// <param name="filePath"></param>
        void DelFile(string filePath);
        
        /// <summary>
        /// 读取文件内容
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        byte[] ReadFile(string filePath);
        
        /// <summary>
        /// 读取文件内容
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        string ReadString(string filePath);
        
        /// <summary>
        /// 写入文件内容
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="bytes"></param>
        void WriteFile(string filePath, byte[] bytes);
        
        /// <summary>
        /// 写入文件内容
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="text"></param>
        void WriteString(string filePath, string text);
        
        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="dirPath"></param>
        void MkDir(string dirPath);
        
    }
}