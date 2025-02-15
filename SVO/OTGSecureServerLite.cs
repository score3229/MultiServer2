﻿using BackendProject.MiscUtils;
using CustomLogger;
using HttpMultipartParser;
using System.Net;
using System.Text;
using WatsonWebserver.Core;
using WatsonWebserver.Lite;

namespace SVO
{
    public class OTGSecureServerLite
    {
        public static bool IsStarted = false;
        private static WebserverLite? _Server;
        private readonly string ip;
        private readonly string certpath;
        private readonly string certpass;
        private readonly int port;

        public OTGSecureServerLite(string certpath, string certpass, string ip, int port)
        {
            this.certpath = certpath;
            this.certpass = certpass;
            this.ip = ip;
            this.port = port;
        }

        private static async Task AuthorizeConnection(HttpContextBase ctx)
        {
            if (SVOServerConfiguration.BannedIPs != null && SVOServerConfiguration.BannedIPs.Contains(ctx.Request.Source.IpAddress))
            {
                LoggerAccessor.LogError($"[SECURITY] - Client - {ctx.Request.Source.IpAddress}:{ctx.Request.Source.Port} Requested the OTG_HTTPS server while being banned!");
                ctx.Response.StatusCode = 403;
                await ctx.Response.Send();
            }
        }

        public void StartServer()
        {
            if (_Server != null && _Server.IsListening)
                LoggerAccessor.LogWarn($"OTG_HTTPS Server already initiated on Port:{port}");
            else
            {
                WebserverSettings settings = new()
                {
                    Hostname = ip,
                    Port = port,
                };

                settings.Ssl.PfxCertificateFile = certpath;
                settings.Ssl.PfxCertificatePassword = certpass;
                settings.Ssl.Enable = true;

                _Server = new WebserverLite(settings, DefaultRoute);
                _Server.Routes.AuthenticateRequest = AuthorizeConnection;
                _Server.Events.Logger = LoggerAccessor.LogInfo;
                _Server.Settings.Debug.Responses = true;
                _Server.Settings.Debug.Routing = true;

                _Server.Start();
                IsStarted = true;
                LoggerAccessor.LogInfo($"OTG_HTTPS Server initiated on Port:{port}...");
            }
        }

        private static async Task DefaultRoute(HttpContextBase ctx)
        {
            HttpStatusCode statusCode = HttpStatusCode.Forbidden;
            string fullurl = string.Empty;
            string absolutepath = string.Empty;
            string clientip = ctx.Request.Source.IpAddress;
            string clientport = ctx.Request.Source.Port.ToString();
            bool sent = false;

            try
            {
                try
                {
                    string? UserAgent = ctx.Request.Useragent.ToLower();
                    if (!string.IsNullOrEmpty(UserAgent) && (UserAgent.Contains("firefox") || UserAgent.Contains("chrome") || UserAgent.Contains("trident") || UserAgent.Contains("bytespider"))) // Get Away TikTok.
                    {
                        LoggerAccessor.LogInfo($"[OTG_HTTPS] - Client - {clientip}:{clientport} Requested the OTG_HTTPS Server while not being allowed!");

                        ctx.Response.StatusCode = (int)statusCode; // Send the other status.
                        ctx.Response.ContentType = "text/plain";
                        sent = await ctx.Response.SendFinalChunk(Array.Empty<byte>());

                        return;
                    }
                }
                catch (Exception)
                {

                }

                if (!string.IsNullOrEmpty(ctx.Request.Url.RawWithQuery))
                {
                    fullurl = HTTPUtils.DecodeUrl(ctx.Request.Url.RawWithQuery);

                    LoggerAccessor.LogInfo($"[OTG_HTTPS] - Client - {clientip}:{clientport} Requested the OTG_HTTPS Server with URL : {ctx.Request.Url.RawWithQuery}");

                    absolutepath = HTTPUtils.ExtractDirtyProxyPath(ctx.Request.RetrieveHeaderValue("Referer")) + HTTPUtils.RemoveQueryString(fullurl);
                    statusCode = HttpStatusCode.Continue;
                }
                else
                    LoggerAccessor.LogInfo($"[OTG_HTTPS] - Client - {clientip}:{clientport} Requested the OTG_HTTPS Server with invalid parameters!");
            }
            catch (Exception)
            {

            }

#if DEBUG
            foreach (string? key in ctx.Request.Headers.AllKeys)
            {
                string? value = ctx.Request.Headers[key];
                LoggerAccessor.LogInfo($"[CollectHeaders] - Debug Headers : HeaderIndex -> {key} | HeaderItem -> {value}");
            }
#endif

            ctx.Response.Headers.Add("Server", "Microsoft HTTP API 2.0");

            if (statusCode == HttpStatusCode.Continue)
            {
                if (absolutepath == "/dataloaderweb/queue")
                {
                    switch (ctx.Request.Method.ToString())
                    {
                        case "POST":
                            if (!string.IsNullOrEmpty(ctx.Request.ContentType))
                            {
                                ctx.Response.ChunkedTransfer = true;

                                statusCode = HttpStatusCode.OK;
                                ctx.Response.ContentType = "application/xml;charset=UTF-8";
                                ctx.Response.Headers.Set("Content-Language", string.Empty);
                                string? boundary = HTTPUtils.ExtractBoundary(ctx.Request.ContentType);

                                var data = MultipartFormDataParser.Parse(new MemoryStream(ctx.Request.DataAsBytes), boundary);

                                byte[] datatooutput = Encoding.UTF8.GetBytes(data.GetParameterValue("body"));

                                Directory.CreateDirectory($"{SVOServerConfiguration.SVOStaticFolder}/dataloaderweb/queue");

                                DirectoryInfo directory = new($"{SVOServerConfiguration.SVOStaticFolder}/dataloaderweb/queue");

                                FileInfo[] files = directory.GetFiles();

                                if (files.Length >= 20)
                                {
                                    FileInfo oldestFile = files.OrderBy(file => file.CreationTime).First();
                                    LoggerAccessor.LogInfo("[OTG_HTTPS] - Replacing Home Debug log file: " + oldestFile.Name);
                                    if (File.Exists(oldestFile.FullName))
                                        File.Delete(oldestFile.FullName);
                                }

                                File.WriteAllBytes($"{SVOServerConfiguration.SVOStaticFolder}/dataloaderweb/queue/{Guid.NewGuid()}.xml", datatooutput);

                                ctx.Response.StatusCode = (int)statusCode;
                                await ctx.Response.SendFinalChunk(datatooutput);
                            }
                            else
                            {
                                statusCode = HttpStatusCode.Forbidden;
                                ctx.Response.StatusCode = (int)statusCode;
                                ctx.Response.ContentType = "text/plain";
                                sent = await ctx.Response.Send();
                            }
                            break;
                        default:
                            statusCode = HttpStatusCode.Forbidden;
                            ctx.Response.StatusCode = (int)statusCode;
                            ctx.Response.ContentType = "text/plain";
                            sent = await ctx.Response.Send();
                            break;
                    }
                }
                else if (absolutepath.Contains("/wox_ws/"))
                    sent = await Wipeout2048.Wipeout2048_OTG(ctx, absolutepath);
                else
                {
                    // Only meant to be used with fairly small files.

                    string filePath = Path.Combine(SVOServerConfiguration.SVOStaticFolder, absolutepath[1..]);

                    if (File.Exists(filePath))
                    {
                        ctx.Response.ChunkedTransfer = true;

                        statusCode = HttpStatusCode.OK;
                        ctx.Response.StatusCode = (int)statusCode;
                        ctx.Response.ContentType = HTTPUtils.GetMimeType(Path.GetExtension(filePath));
                        ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                        ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                        ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                        sent = await ctx.Response.SendFinalChunk(File.ReadAllBytes(filePath));
                    }
                    else
                    {
                        statusCode = HttpStatusCode.NotFound;
                        ctx.Response.StatusCode = (int)statusCode;
                        ctx.Response.ContentType = "text/plain";
                        sent = await ctx.Response.Send();
                    }
                }
            }
            else
            {
                ctx.Response.StatusCode = (int)statusCode; // Send the other status.
                ctx.Response.ContentType = "text/plain";
                sent = await ctx.Response.Send();
            }

#if DEBUG
            if (!sent)
                LoggerAccessor.LogWarn($"[OTG_HTTPS] - {clientip}:{clientport} Failed to receive the response! Client might have closed the wire.");
#endif
        }
    }
}
