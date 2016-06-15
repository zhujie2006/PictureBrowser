namespace TestPictureBrowser.Helpers
{
    /// <summary>
    /// File helper
    /// </summary>
    public class FileHelper
    {
        /// <summary>
        /// Get file extension(.*)
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
        /// Get file name
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
