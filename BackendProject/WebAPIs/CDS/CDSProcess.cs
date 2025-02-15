﻿using BackendProject.HomeTools.Crypto;
using CustomLogger;

namespace BackendProject.WebAPIs.CDS
{
    public class CDSProcess
    {
        public static byte[]? CDSEncrypt_Decrypt(byte[] buffer, string sha1)
        {
            byte[]? digest = ConvertSha1StringToByteArray(sha1.ToUpper());
            if (digest != null)
                return new BlowfishCTREncryptDecrypt().InitiateCTRBuffer(buffer,
                    BitConverter.GetBytes(new ToolsImpl().Sha1toNonce(digest))); // Always big endian, so GetBytes() is fine as is.

            return null;
        }

        private static byte[]? ConvertSha1StringToByteArray(string sha1String)
        {
            if (sha1String.Length % 2 != 0)
            {
                LoggerAccessor.LogError("Input string length must be even.");
                return null;
            }

            byte[] byteArray = new byte[sha1String.Length / 2];

            for (int i = 0; i < sha1String.Length; i += 2)
            {
                string hexByte = sha1String.Substring(i, 2);
                byteArray[i / 2] = Convert.ToByte(hexByte, 16);
            }

            return byteArray;
        }
    }
}
