using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace RestApi.Client.Pages
{
    public static class StringUtil
    {
        [JSInvokable]
        public static Task<string> Concat(string str1, string str2, string str3)
        {
            return Task.FromResult(string.Concat(str1, str2, str3));
        }
    }
}
