using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace EasyUtils
{
    public class Http
    {
        private Dictionary<string, string> _headers = new Dictionary<string, string>()
        {
            // Windows
            // { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.100 Safari/537.36" },
            
            // Android
            { "User-Agent","Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.63 Mobile Safari/537.36" },
        };

        private int _http_retries = 3;
        private int _chunk_size = 4096;
        private float _timeout = 10;
        private string _proxy = null;

        private CookieContainer cookieContainer = new CookieContainer();
        private void _set_cookie(string domain, string name, string value)
        {
            cookieContainer.Add(new Cookie(name, value, null, domain));
        }

        public enum LogLevel
        {
            None = 0,
            Error = 1,
            Warning = 2,
            Info = 3,
        }

        private void Log(string log, LogLevel level)
        {
#if UNITY_5_3_OR_NEWER
            if (level == LogLevel.Info) UnityEngine.Debug.Log($"{log}");
            else if (level == LogLevel.Warning) UnityEngine.Debug.LogWarning($"{log}");
            else if (level == LogLevel.Error) UnityEngine.Debug.LogError($"{log}");
#else
            Console.WriteLine($"{log}");
#endif
        }

        private HttpClient _get_client(Dictionary<string, string> headers = null,
           float timeout = 0, bool allowRedirect = true, string proxy = null)
        {
            var handler = new HttpClientHandler();
            handler.UseCookies = true;
            handler.AllowAutoRedirect = allowRedirect;
            handler.CookieContainer = cookieContainer;

            var client = new HttpClient(handler, true);

            headers = headers ?? _headers;

            if (headers != null)
            {
                foreach (var item in headers)
                {
                    client.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }

            timeout = timeout > 0 ? timeout : _timeout;
            client.Timeout = new TimeSpan((long)(timeout * 10000000L));

            proxy = proxy ?? _proxy;
            if (proxy != null)
            {
                handler.UseProxy = true;
                handler.Proxy = new WebProxy(proxy);
            }

            return client;
        }

        public async Task<string> GetText(string url, string proxy = null)
        {
            string text = null;
            for (int i = 0; i < _http_retries; i++)
            {
                try
                {
                    using (var client = _get_client(null, 0, true, proxy))
                    {
                        using (var resp = await client.GetAsync(url))
                        {
                            resp.EnsureSuccessStatusCode();
                            text = await resp.Content.ReadAsStringAsync();
                        }
                    }
                    break;
                }
                catch (Exception ex)
                {
                    Log($"Http Error: {ex.Message}", LogLevel.Error);
                    if (i < _http_retries) Log($"Retry({i + 1}): {url}", LogLevel.Warning);
                }
            }
            return text;
        }

        public async Task<byte[]> GetBytes(string url, string proxy = null)
        {
            byte[] bytes = null;
            for (int i = 0; i < _http_retries; i++)
            {
                try
                {
                    using (var client = _get_client(null, 0, true, proxy))
                    {
                        using (var resp = await client.GetAsync(url))
                        {
                            resp.EnsureSuccessStatusCode();
                            bytes = await resp.Content.ReadAsByteArrayAsync();
                        }
                    }
                    break;
                }
                catch (Exception ex)
                {
                    Log($"Http Error: {ex.Message}", LogLevel.Error);
                    if (i < _http_retries) Log($"Retry({i + 1}): {url}", LogLevel.Warning);
                }
            }
            return bytes;
        }

        private async Task<HttpContentHeaders> GetHeaders(string url, string proxy)
        {
            HttpContentHeaders content_headers = null;
            for (int i = 0; i < _http_retries; i++)
            {
                try
                {
                    using (var client = _get_client(null, 0, false, proxy))
                    {
                        using (var resp = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                        {
                            content_headers = resp.Content.Headers;
                        }
                    }
                    break;
                }
                catch (Exception ex)
                {
                    Log($"Http Error: {ex.Message}", LogLevel.Error);
                    if (i < _http_retries) Log($"Retry({i + 1}): {url}", LogLevel.Info);
                }
            }
            return content_headers;
        }

        public async Task<bool> Download(string url, string path, bool overwrite, IProgress<long[]> progress = null, string proxy = null)
        {
            var _con_len = (await GetHeaders(url, proxy))?.ContentLength;
            if (_con_len == null) return false;

            long content_length = _con_len.Value;
            bool isDownloadSuccess = false;
            string tmp_file_path = path + ".download";          // 正在下载中的文件名
            long now_size = 0;
            bool is_downloaded = false;

            if (File.Exists(tmp_file_path))
            {
                now_size = new FileInfo(tmp_file_path).Length;  // 本地已经下载的文件大小
                if (now_size > content_length)                  // 大小错误，删除重新下载
                {
                    File.Delete(tmp_file_path);
                    now_size = 0;
                }
                else if (now_size == content_length)            // 已经下载完成，只是未改后缀名
                {
                    is_downloaded = true;
                }
            }

            if (is_downloaded)
            {
                Log($"Has already downloaded: {tmp_file_path}", LogLevel.Info);
                progress?.Report(new long[] { now_size, content_length });
                isDownloadSuccess = true;
            }
            else
            {
                await Task.Run(async () =>
                {
                    var headers = new Dictionary<string, string>(_headers);
                    headers.Add("Range", $"bytes={now_size}-");
                    int chunk_size = _chunk_size;
                    var chunk = new byte[chunk_size];
                    var p_downloading = new long[2] { now_size, content_length };
                    for (int i = 0; i < _http_retries; i++)
                    {
                        try
                        {
                            using (var client = _get_client(headers, 0, true, proxy))
                            {
                                using (var netStream = await client.GetStreamAsync(url))
                                {
                                    using (var fileStream = new FileStream(tmp_file_path, FileMode.Append,
                                         FileAccess.Write, FileShare.Read, chunk_size))
                                    {
                                        while (true)
                                        {
                                            var readLength = await netStream.ReadAsync(chunk, 0, chunk_size);
                                            if (readLength == 0)
                                                break;

                                            await fileStream.WriteAsync(chunk, 0, readLength);
                                            now_size += readLength;

                                            p_downloading[0] = now_size;
                                            progress?.Report(p_downloading);
                                        }
                                    }
                                }
                            }

                            isDownloadSuccess = true;
                            break;
                        }
                        catch (Exception ex)
                        {
                            Log($"Http Error: {ex.Message}", LogLevel.Error);
                            if (i < _http_retries) Log($"Retry({i + 1}): {url}", LogLevel.Warning);
                        }
                    }
                });
            }

            if (isDownloadSuccess)
            {
                if (overwrite && File.Exists(path))
                {
                    File.Delete(path);
                    Log($"Overwrire delete file path: {path}", LogLevel.Info);
                }

                Log($"Move tmp file to real path: {path}", LogLevel.Info);
                // 下载完成，改回正常文件名
                File.Move(tmp_file_path, path);

                progress?.Report(new long[] { now_size, content_length });

                await Task.Yield(); // 保证 progress report 到达
                await Task.Yield(); // 保证 progress report 到达
            }

            return isDownloadSuccess;
        }
    }
}
