namespace DocumentTools
{
    //此处用来存放自定义版本信息
    internal static class VersionInfo
    {
        /// <summary>
        /// 程序的版本号，格式为：主版本号.次版本号.修订版本号
        /// </summary>
        public static string VersionNumber = "0.4.3";

        /// <summary>
        /// 软件名称
        /// </summary>
        public static string VersionTitle = $"坤_Tools_{VersionNumber}";

        /// <summary>
        /// 程序的更新检查URL，为XML文件
        /// </summary>
        public static string VersionUpdateCheckUrl = "https://gitee.com/jidesheng666/document-tools-version/raw/master/MainUpdate.xml";

        /// <summary>
        /// 程序的更新日志URL
        /// </summary>
        public static string VersionUpdateLogUrl = "https://gitee.com/jidesheng666/document-tools-version/tree/master/ReleaseLog";

        /// <summary>
        /// 程序的公告信息
        /// </summary>
        public static string VersionNoticeMessageUrl = "https://jipkqhufibk.feishu.cn/wiki/AoxswyndqiO169kvpZ8cPxbtncb";
    }
}
