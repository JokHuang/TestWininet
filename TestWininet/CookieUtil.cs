using System;
using System.Text;
using System.Net;
using System.Runtime.InteropServices;

namespace TestWininet
{
    public class CookieUtil
    {
        private const Int32 InternetCookieHttponly = 0x2000;
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool InternetGetCookie(string url, string cookieName, StringBuilder cookieData, ref int size, Int32 dwFlags, IntPtr lpReserved);

        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool InternetSetCookie(string url, string cookieName, string cookieData, Int32 dwFlags, IntPtr dwReserved);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InternetSetOption(int hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

        /// <summary>
        /// Get CookieContainer for the given Uri.
        /// </summary>
        /// <param name="uri">Uri of the cookie container</param>
        /// <returns>An instance of CookieContainer that contains all the cookies for the given uri.</returns>
        public virtual CookieContainer GetUriCookieContainer(Uri uri)
        {
            CookieContainer cookies = null!;
            // Determine the size of the cookie
            int datasize = 8192 * 16;
            StringBuilder cookieData = new StringBuilder(datasize);
            if (!InternetGetCookie(uri.ToString(), null!, cookieData, ref datasize, InternetCookieHttponly, IntPtr.Zero))
            {
                if (datasize < 0)
                    return cookies;
                // Allocate stringbuilder large enough to hold the cookie
                cookieData = new StringBuilder(datasize);
                if (!InternetGetCookie(uri.ToString(), null!, cookieData, ref datasize, InternetCookieHttponly, IntPtr.Zero))
                    return cookies;
            }
            if (cookieData.Length > 0)
            {
                cookies = new CookieContainer();
                var rootUri = new UriBuilder(uri.Scheme, uri.Host, uri.Port).Uri;
                cookies.SetCookies(rootUri, cookieData.ToString().Replace(';', ','));
            }
            return cookies;
        }

        /// <summary>
        /// Creates a cookie with a specified name that is associated with a specified Uri
        /// </summary>
        /// <param name="uri">Uri of the cookie</param>
        /// <param name="cookieName">Name of the cookie</param>
        /// <param name="cookieData">Data of the cookie</param>
        /// <returns>If the function succeeds, returns true. If the function fails, returns false.</returns>
        public virtual bool SetUriCookieContainer(string uri, string cookieName, string cookieData)
        {
            return InternetSetCookie(uri, cookieName, cookieData, InternetCookieHttponly, IntPtr.Zero);
        }

        /// <summary>
        /// Suppresses the persistence of cookies, even if the server has specified them as persistent.
        /// </summary>
        /// <returns>If the function succeeds, returns true. If the function fails, returns false.</returns>
        public virtual bool SupressCookiePersist()
        {
            // 3 = INTERNET_SUPPRESS_COOKIE_PERSIST 
            // 81 = INTERNET_OPTION_SUPPRESS_BEHAVIOR
            return SetOption(81, 3);
        }

        /// <summary>
        /// Remove all the session cookies.
        /// </summary>
        /// <returns>If the function succeeds, returns true. If the function fails, returns false.</returns>
        public virtual bool EndBrowserSession()
        {
            // 42 = INTERNET_OPTION_END_BROWSER_SESSION 
            return SetOption(42, null);
        }

        private bool SetOption(int settingCode, int? option)
        {
            IntPtr optionPtr = IntPtr.Zero;
            int size = 0;
            if (option.HasValue)
            {
                size = sizeof(int);
                optionPtr = Marshal.AllocCoTaskMem(size);
                Marshal.WriteInt32(optionPtr, option.Value);
            }

            bool success = InternetSetOption(0, settingCode, optionPtr, size);

            if (optionPtr != IntPtr.Zero)
                Marshal.FreeCoTaskMem(optionPtr);
            return success;
        }
    }
}
