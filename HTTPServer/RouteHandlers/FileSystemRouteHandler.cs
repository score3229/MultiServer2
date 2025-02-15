﻿// Copyright (C) 2016 by Barend Erasmus, David Jeske and donated to the public domain
using BackendProject.FileHelper.Utils;
using BackendProject.MiscUtils;
using HTTPServer.Extensions;
using HTTPServer.Models;
using System.Text;

namespace HTTPServer.RouteHandlers
{
    public class FileSystemRouteHandler
    {
        public static HttpResponse Handle(HttpRequest request, string filepath, string httpdirectoryrequest, string clientip, string? clientport)
        {
            if (Directory.Exists(filepath) && filepath.EndsWith("/"))
                return Handle_LocalDir(request, filepath, httpdirectoryrequest, clientip, clientport);
            else if (File.Exists(filepath))
                return Handle_LocalFile(request, filepath);
            else
                return HttpBuilder.NotFound();
        }

        public static HttpResponse HandleHEAD(HttpRequest request, string local_path)
        {
            if (File.Exists(local_path))
            {
                HttpResponse response = new(request.GetHeaderValue("Connection") == "keep-alive")
                {
                    HttpStatusCode = HttpStatusCode.OK
                };
                string ContentType = HTTPUtils.GetMimeType(Path.GetExtension(local_path));
                if (ContentType == "application/octet-stream")
                {
                    bool matched = false;
                    byte[] VerificationChunck = VariousUtils.ReadSmallFileChunck(local_path, 10);
                    foreach (var entry in HTTPUtils.PathernDictionary)
                    {
                        if (VariousUtils.FindbyteSequence(VerificationChunck, entry.Value))
                        {
                            matched = true;
                            response.Headers["Content-Type"] = entry.Key;
                            break;
                        }
                    }
                    if (!matched)
                        response.Headers["Content-Type"] = ContentType;
                }
                else
                    response.Headers["Content-Type"] = ContentType;

                response.Headers.Add("Content-Length", new FileInfo(local_path).Length.ToString());

                return response;
            }
            else
                return HttpBuilder.NotFound();
        }

        private static HttpResponse Handle_LocalFile(HttpRequest request, string local_path)
        {
            HttpResponse? response = null;
            string? encoding = request.GetHeaderValue("Accept-Encoding");

            string ContentType = HTTPUtils.GetMimeType(Path.GetExtension(local_path));
            if (ContentType == "application/octet-stream")
            {
                byte[] VerificationChunck = VariousUtils.ReadSmallFileChunck(local_path, 10);
                foreach (var entry in HTTPUtils.PathernDictionary)
                {
                    if (VariousUtils.FindbyteSequence(VerificationChunck, entry.Value))
                    {
                        ContentType = entry.Key;
                        break;
                    }
                }
            }
            if (request.GetHeaderValue("User-Agent").Contains("PSHome") && (ContentType == "video/mp4" || ContentType == "video/mpeg" || ContentType == "audio/mpeg"))
                response = new(request.GetHeaderValue("Connection") == "keep-alive", "1.0") // Home has a game bug where media files do not play well in screens/jukboxes with http 1.1.
                {
                    HttpStatusCode = HttpStatusCode.OK
                };
            else
                response = new(request.GetHeaderValue("Connection") == "keep-alive")
                {
                    HttpStatusCode = HttpStatusCode.OK
                };
            response.Headers.Add("Accept-Ranges", "bytes");
            response.Headers.Add("Content-Type", ContentType);
            if (!string.IsNullOrEmpty(encoding) && encoding.Contains("deflate") && new FileInfo(local_path).Length <= 80000000) // We must be reasonable on the file-size here (80 Mb).
            {
                response.Headers.Add("Content-Encoding", "deflate");
                response.ContentStream = HTTPUtils.InflateStream(File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            }
            else
                response.ContentStream = File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            return response;
        }

        public static HttpResponse Handle_LocalFile_Download(HttpRequest request, string local_path)
        {
            string? encoding = request.GetHeaderValue("Accept-Encoding");

            HttpResponse response = new(request.GetHeaderValue("Connection") == "keep-alive")
            {
                HttpStatusCode = HttpStatusCode.OK,
            };
            response.Headers["Content-disposition"] = $"attachment; filename={Path.GetFileName(local_path)}";
             if (!string.IsNullOrEmpty(encoding) && encoding.Contains("deflate") && new FileInfo(local_path).Length <= 80000000) // We must be reasonable on the file-size here (80 Mb).
            {
                response.Headers.Add("Content-Encoding", "deflate");
                response.ContentStream = HTTPUtils.InflateStream(File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            }
            else
                response.ContentStream = File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            return response;
        }

        public static HttpResponse Handle_ByteSubmit_Download(HttpRequest request, byte[]? Data, string FileName)
        {
            HttpResponse response = new(request.GetHeaderValue("Connection") == "keep-alive");
            if (Data != null)
            {
                response.HttpStatusCode = HttpStatusCode.OK;
                response.Headers["Content-disposition"] = $"attachment; filename={FileName}";
                response.ContentStream = new MemoryStream(Data);
            }
            else
                response.HttpStatusCode = HttpStatusCode.InternalServerError;

            return response;
        }

        private static HttpResponse Handle_LocalDir(HttpRequest request, string local_path, string httpdirectoryrequest, string clientip, string? clientport)
        {
            string? queryparam = null;
            string? encoding = request.GetHeaderValue("Accept-Encoding");

            if (request.QueryParameters != null && request.QueryParameters.TryGetValue("directory", out queryparam) && queryparam == "on")
            {
                if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                    return HttpResponse.Send(HTTPUtils.Compress(
                            Encoding.UTF8.GetBytes(FileStructureToJson.GetFileStructureAsJson(local_path[..^1], httpdirectoryrequest))), "application/json", new string[][] { new string[] { "Content-Encoding", "gzip" } });
                else
                    return HttpResponse.Send(FileStructureToJson.GetFileStructureAsJson(local_path[..^1], httpdirectoryrequest), "application/json");
            }
            else
            {
                foreach (string indexFile in HTTPUtils.DefaultDocuments)
                {
                    if (File.Exists(local_path + indexFile))
                    {
                        if (indexFile.Contains(".php") && Directory.Exists(HTTPServerConfiguration.PHPStaticFolder))
                        {
                            (byte[]?, string[][]) CollectPHP = PHP.ProcessPHPPage(local_path + indexFile, HTTPServerConfiguration.PHPStaticFolder, HTTPServerConfiguration.PHPVersion, clientip, clientport, request);
                            if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip") && CollectPHP.Item1 != null)
                                return HttpResponse.Send(HTTPUtils.Compress(CollectPHP.Item1), "text/html", VariousUtils.AddElementToLastPosition(CollectPHP.Item2, new string[] { "Content-Encoding", "gzip" }));
                            else
                                return HttpResponse.Send(CollectPHP.Item1, "text/html", CollectPHP.Item2);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                            {
                                using FileStream stream = new(local_path + indexFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                byte[]? buffer = null;

                                using (MemoryStream ms = new())
                                {
                                    stream.CopyTo(ms);
                                    buffer = ms.ToArray();
                                    ms.Flush();
                                }

                                stream.Flush();

                                return HttpResponse.Send(HTTPUtils.Compress(buffer), "text/html", new string[][] { new string[] { "Content-Encoding", "gzip" } });
                            }
                            else
                                return HttpResponse.Send(File.Open(local_path + indexFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), "text/html");
                        }
                    }
                }

                return new HttpResponse(request.GetHeaderValue("Connection") == "keep-alive")
                {
                    HttpStatusCode = HttpStatusCode.Not_Found,
                    ContentAsUTF8 = string.Empty
                };

            }
        }
    }
}
