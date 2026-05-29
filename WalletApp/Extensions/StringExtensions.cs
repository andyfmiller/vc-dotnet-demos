namespace WalletApp.Extensions
{
    public static class StringExtensions
    {
        public static string EnsureTrailingSlash(this string url)
        {
            return url.EndsWith('/') ? url : string.Concat(url, "/");
        }

        public static string EnsureApiPrefix(this string url)
        {
            return url.EnsureTrailingSlash().EndsWith("ims/clr/v1p0/", StringComparison.OrdinalIgnoreCase)
                ? url.EnsureTrailingSlash()
                : url.EnsureTrailingSlash() + "ims/clr/v1p0/";
        }

        public static string ToHtmlId(this string value)
        {
            return value
                .Replace(':', '-')
                .Replace('/', '-')
                .Replace('.', '-')
                .Replace('?', '-')
                .Replace('=', '-');
        }
    }
}
