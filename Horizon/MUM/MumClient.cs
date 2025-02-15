﻿using System.Net;
using System.Text;

namespace Horizon.MUM
{
    public class MumClient
    {
        public static string? GetServerResult(string ip, ushort port, string command, string key)
        {
#if NET7_0
            try
            {
                HttpClient client = new();
                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Safari/537.36");
                client.DefaultRequestHeaders.Add("method", "GET");
                client.DefaultRequestHeaders.Add("content-type", "text/xml; charset=UTF-8");
                HttpResponseMessage response = client.GetAsync($"http://{ip}:{port}/{command}/").Result;
                response.EnsureSuccessStatusCode();
                return Encoding.UTF8.GetString(BackendProject.CryptoUtils.AESCTR256EncryptDecrypt
                    .InitiateCTRBuffer(Convert.FromBase64String(response.Content.ReadAsStringAsync().Result.Replace("<Secure>", string.Empty).Replace("</Secure>", string.Empty)), Convert.FromBase64String(key), MumUtils.ConfigIV));
            }
            catch (Exception)
            {
                // Not Important.
            }
#else
            try
            {
#pragma warning disable // NET 6.0 and lower has a bug where GetAsync() is EXTREMLY slow to operate (https://github.com/dotnet/runtime/issues/65375).
                WebClient client = new();
                client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.77 Safari/537.36");
                client.Headers.Add("method", "GET");
                client.Headers.Add("content-type", "text/xml; charset=UTF-8");
                return Encoding.UTF8.GetString(BackendProject.CryptoUtils.AESCTR256EncryptDecrypt
                    .InitiateCTRBuffer(Convert.FromBase64String(client.DownloadStringTaskAsync(new Uri($"http://{ip}:{port}/{command}/")).Result.Replace("<Secure>", string.Empty).Replace("</Secure>", string.Empty)), Convert.FromBase64String(key), MumUtils.ConfigIV));
#pragma warning restore
            }
            catch (Exception)
            {
                // Not Important.
            }
#endif

            return null;
        }
    }
}
