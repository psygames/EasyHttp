using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace EasyHttp
{
    public class Log
    {
        public static void Error(object log)
        {
#if UNITY_5_3_OR_NEWER
            UnityEngine.Debug.LogError($"[EasyHttp][Error] {log}");
#else
            Console.WriteLine($"[EasyHttp][Error] {log}");
#endif
        }

    }
}
