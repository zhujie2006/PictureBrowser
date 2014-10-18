namespace TestPictureBrowser.Utils
{
    public class FileHelper
    {
        /// <summary>
        /// 获取文件后缀(.*)
        /// </summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        public static string GetFileExt(string strPath)
        {
            if (!string.IsNullOrEmpty(strPath))
            {
                int iPos = strPath.LastIndexOf('.');
                if (-1 != iPos)
                {
                    return strPath.Substring(iPos);
                }
            }

            return null;
        }

        /// <summary>
        /// 获取文件名称
        /// </summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        public static string GetFileName(string strPath)
        {
            if (!string.IsNullOrEmpty(strPath))
            {
                int iPos = strPath.LastIndexOf('\\');
                if (-1 != iPos)
                {
                    return strPath.Substring(iPos + 1);
                }
            }

            return null;
        }
    }
}
