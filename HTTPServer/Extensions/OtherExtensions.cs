﻿using System.Net.Sockets;
using System.Text;

namespace HTTPServer.Extensions
{
    public static class OtherExtensions
    {
        public static string ToHttpHeaders(this Dictionary<string, string> headers)
        {
            return string.Join("\r\n", headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value)));
        }

        public static string GetString(this Stream stream)
        {
            return Encoding.ASCII.GetString(((MemoryStream)stream).ToArray());
        }

        public static string RemoveQueryString(string input)
        {
            int indexOfQuestionMark = input.IndexOf('?');

            if (indexOfQuestionMark >= 0)
                return input[..indexOfQuestionMark];
            else
                return input;
        }

        public static bool IsConnected(this TcpClient tcpClient)
        {
            if (tcpClient.Client.Connected)
            {
                if (tcpClient.Client.Poll(0, SelectMode.SelectWrite) && !tcpClient.Client.Poll(0, SelectMode.SelectError))
                {
                    byte[] buffer = new byte[1];
                    if (tcpClient.Client.Receive(buffer, SocketFlags.Peek) == 0)
                        return false;
                    else
                        return true;
                }
                else
                    return false;
            }

            return false;
        }
    }
}
