using CustomLogger;
using BackendProject.HomeTools.BARFramework;
using BackendProject.HomeTools.Crypto;
using BackendProject.MiscUtils;
using BackendProject.CryptoUtils;

namespace BackendProject.HomeTools.UnBAR
{
    public class RunUnBAR
    {
        public async Task Run(string filePath, string outputpath, bool edat)
        {
            if (edat)
                await RunDecrypt(filePath, outputpath);
            else
                await new RunUnBAR().RunExtract(filePath, outputpath);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect(); // We have no choice and it's not possible to remove them, HomeTools create a BUNCH of necessary objects.
        }

        public static void RunEncrypt(string filePath, string sdatfilePath, string? sdatnpdcopyfile)
        {
            try
            {
                new EDAT().encryptFile(filePath, sdatfilePath, sdatnpdcopyfile);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[RunEncrypt] - SDAT Encryption failed with exception : {ex}");
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect(); // We have no choice and it's not possible to remove them, HomeTools create a BUNCH of necessary objects.
        }

        private async Task RunDecrypt(string filePath, string outDir)
        {
            int status = new EDAT().decryptFile(filePath, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath) + ".dat"));

            if (status == 0)
                await new RunUnBAR().RunExtract(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath) + ".dat"), outDir);
            else
                LoggerAccessor.LogError($"[RunUnBAR] - RunDecrypt failed with status code : {status}");
        }

        private async Task RunExtract(string filePath, string outDir)
        {
            bool isSharc = false;
            bool isLittleEndian = false;
            string options = ToolsImpl.base64DefaultSharcKey;
            byte[]? RawBarData = null;

            if (File.Exists(filePath))
            {
                try
                {
                    RawBarData = await File.ReadAllBytesAsync(filePath);

                    if (VariousUtils.FindBytePattern(RawBarData, new byte[] { 0xAD, 0xEF, 0x17, 0xE1, 0x02, 0x00, 0x00, 0x00 }) != -1)
                        isSharc = true;
                    else if (VariousUtils.FindBytePattern(RawBarData, new byte[] { 0xE1, 0x17, 0xEF, 0xAD, 0x00, 0x00, 0x00, 0x02 }) != -1)
                    {
                        isSharc = true;
                        isLittleEndian = true;
                    }

                    if (isSharc && RawBarData.Length > 52)
                    {
                        ToolsImpl? toolsImpl = new();

                        try
                        {
                            byte[]? HeaderIV = new byte[16];

                            Buffer.BlockCopy(RawBarData, 8, HeaderIV, 0, HeaderIV.Length);

                            if (HeaderIV != null)
                            {
                                byte[] SharcHeader = new byte[28];

                                Buffer.BlockCopy(RawBarData, 24, SharcHeader, 0, SharcHeader.Length);

                                SharcHeader = AESCTR256EncryptDecrypt.InitiateCTRBuffer(SharcHeader,
                                 Convert.FromBase64String(options), HeaderIV);

                                if (SharcHeader == Array.Empty<byte>())
                                    return; // Sharc Header failed to decrypt.
                                else if (!VariousUtils.AreArraysIdentical(new byte[] { SharcHeader[0], SharcHeader[1], SharcHeader[2], SharcHeader[3] }, new byte[4]))
                                {
                                    options = ToolsImpl.base64CDNKey1;

                                    Buffer.BlockCopy(RawBarData, 24, SharcHeader, 0, SharcHeader.Length);

                                    SharcHeader = AESCTR256EncryptDecrypt.InitiateCTRBuffer(SharcHeader,
                                     Convert.FromBase64String(options), HeaderIV);

                                    if (SharcHeader == Array.Empty<byte>())
                                        return; // Sharc Header failed to decrypt.
                                    else if (!VariousUtils.AreArraysIdentical(new byte[] { SharcHeader[0], SharcHeader[1], SharcHeader[2], SharcHeader[3] }, new byte[4]))
                                    {
                                        options = ToolsImpl.base64CDNKey2;

                                        Buffer.BlockCopy(RawBarData, 24, SharcHeader, 0, SharcHeader.Length);

                                        SharcHeader = AESCTR256EncryptDecrypt.InitiateCTRBuffer(SharcHeader,
                                         Convert.FromBase64String(options), HeaderIV);

                                        if (SharcHeader == Array.Empty<byte>())
                                            return; // Sharc Header failed to decrypt.
                                        else if (!VariousUtils.AreArraysIdentical(new byte[] { SharcHeader[0], SharcHeader[1], SharcHeader[2], SharcHeader[3] }, new byte[4]))
                                            return; // All keys failed to decrypt.
                                    }
                                }

                                byte[] NumOfFiles = new byte[4];

                                if (isLittleEndian == true)
                                    Buffer.BlockCopy(SharcHeader, SharcHeader.Length - 20, NumOfFiles, 0, NumOfFiles.Length);
                                else
                                    Buffer.BlockCopy(Org.BouncyCastle.util.EndianTools.ReverseEndiannessInChunks(SharcHeader, 4), SharcHeader.Length - 20, NumOfFiles, 0, NumOfFiles.Length);

                                byte[]? SharcTOC = new byte[24 * BitConverter.ToUInt32(NumOfFiles, 0)];

                                Buffer.BlockCopy(RawBarData, 52, SharcTOC, 0, SharcTOC.Length);

                                if (SharcTOC != null)
                                {
                                    byte[] OriginalIV = new byte[HeaderIV.Length];

                                    Buffer.BlockCopy(HeaderIV, 0, OriginalIV, 0, OriginalIV.Length);

                                    toolsImpl.IncrementIVBytes(HeaderIV, 1); // IV so we increment.

                                    SharcTOC = AESCTR256EncryptDecrypt.InitiateCTRBuffer(SharcTOC, Convert.FromBase64String(options), HeaderIV);

                                    if (SharcTOC != Array.Empty<byte>())
                                    {
                                        byte[] SharcData = new byte[RawBarData.Length - (52 + SharcTOC.Length)];

                                        Buffer.BlockCopy(RawBarData, 52 + SharcTOC.Length, SharcData, 0, SharcData.Length);

                                        byte[] FileBytes = Array.Empty<byte>();

                                        if (isLittleEndian)
                                        {
                                            FileBytes = VariousUtils.CombineByteArrays(new byte[] { 0xE1, 0x17, 0xEF, 0xAD, 0x00, 0x00, 0x00, 0x02 }, new byte[][]
                                            {
                                                    OriginalIV,
                                                    SharcHeader,
                                                    SharcTOC,
                                                    SharcData
                                            });
                                        }
                                        else
                                        {
                                            FileBytes = VariousUtils.CombineByteArrays(new byte[] { 0xAD, 0xEF, 0x17, 0xE1, 0x02, 0x00, 0x00, 0x00 }, new byte[][]
                                            {
                                                    OriginalIV,
                                                    SharcHeader,
                                                    SharcTOC,
                                                    SharcData
                                            });
                                        }

                                        Directory.CreateDirectory(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)));

                                        File.WriteAllBytes(filePath, FileBytes);

                                        LoggerAccessor.LogInfo("Loading SHARC/dat: {0}", filePath);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerAccessor.LogError($"[RunUnBAR] - SHARC Decryption failed! with error - {ex}");
                        }

                        toolsImpl = null;
                    }
                    else
                    {
                        if (VariousUtils.FindBytePattern(RawBarData, new byte[] { 0xAD, 0xEF, 0x17, 0xE1 }) != -1)
                        {

                        }
                        else if (VariousUtils.FindBytePattern(RawBarData, new byte[] { 0xE1, 0x17, 0xEF, 0xAD }) != -1)
                            isLittleEndian = true;
                        else
                            return; // File not a BAR.

                        Directory.CreateDirectory(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)));

                        LoggerAccessor.LogInfo("Loading BAR/dat: {0}", filePath);
                    }
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[RunUnBAR] - Timestamp creation failed! with error - {ex}");
                }
            }

            if (Directory.Exists(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath))))
            {
                try
                {
                    BARArchive? archive = null;
                    archive = new(filePath, outDir);
                    archive.Load();
                    archive.WriteMap(filePath);
                    File.WriteAllText(Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)) + "/timestamp.txt", archive.BARHeader.UserData.ToString("X"));

                    // Create a list to hold the tasks
                    List<Task>? TOCTasks = new();

                    foreach (TOCEntry tableOfContent in archive.TableOfContents)
                    {
                        byte[] FileData = tableOfContent.GetData(archive.GetHeader().Flags);

                        if (FileData != null)
                        {
                            // Create a task for each iteration
                            Task task = Task.Run(() =>
                            {
                                try
                                {
                                    if (archive.GetHeader().Version == 512)
                                        ExtractToFileBarVersion2(archive.GetHeader().Key, archive, tableOfContent.FileName, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)));
                                    else
                                    {
                                        using (MemoryStream memoryStream = new(FileData))
                                        {
                                            ExtractToFileBarVersion1(RawBarData, archive, tableOfContent.FileName, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)),
                                                FileTypeAnalyser.Instance.GetRegisteredExtension(FileTypeAnalyser.Instance.Analyse(memoryStream)));
                                            memoryStream.Flush();
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LoggerAccessor.LogWarn(ex.ToString());
                                    if (archive.GetHeader().Version == 512)
                                        ExtractToFileBarVersion2(archive.GetHeader().Key, archive, tableOfContent.FileName, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)));
                                    else
                                        ExtractToFileBarVersion1(RawBarData, archive, tableOfContent.FileName, Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)), ".unknown");
                                }
                            });

                            TOCTasks.Add(task);
                        }
                    }

                    // Wait for all tasks to complete
                    await Task.WhenAll(TOCTasks);

                    TOCTasks = null;
                    archive = null;

                    if (File.Exists(filePath + ".map"))
                        File.Move(filePath + ".map", Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath) + $"/{Path.GetFileName(filePath)}.map"));
                    else if (filePath.Length > 4 && File.Exists(filePath[..^4] + ".sharc.map"))
                        File.Move(filePath[..^4] + ".sharc.map", Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath) + $"/{Path.GetFileName(filePath)}.map"));
                    else if (filePath.Length > 4 && File.Exists(filePath[..^4] + ".bar.map"))
                        File.Move(filePath[..^4] + ".bar.map", Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath) + $"/{Path.GetFileName(filePath)}.map"));
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[RunUnBAR] - RunExtract Errored out - {ex}");
                }
            }
        }

        public void ExtractToFileBarVersion1(byte[]? RawBarData, BARArchive archive, HashedFileName FileName, string outDir, string fileType)
        {
            TOCEntry? tableOfContent = archive.TableOfContents[FileName];
            string path = string.Empty;
            if (string.IsNullOrEmpty(tableOfContent.Path))
                path = string.Format("{0}{1}{2:X8}{3}", outDir, Path.DirectorySeparatorChar, FileName.Value, fileType);
            else
            {
                string str = tableOfContent.Path.Replace('/', Path.DirectorySeparatorChar);
                path = string.Format("{0}{1}{2}", outDir, Path.DirectorySeparatorChar, str);
            }
            string? outdirectory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(outdirectory))
            {
                Directory.CreateDirectory(outdirectory);
                using (FileStream fileStream = File.Open(path, (FileMode)2))
                {
                    byte[] data = tableOfContent.GetData(archive.GetHeader().Flags);
                    if (tableOfContent.Compression == CompressionMethod.Encrypted && data.Length > 28 && 
                        ((data[0] == 0x00 && data[1] == 0x00 && data[2] == 0x00 && data[3] == 0x01) || (data[0] == 0x01 && data[1] == 0x00 && data[2] == 0x00 && data[3] == 0x00)))
                    {
                        int dataStart = FindDataPosInBinary(RawBarData, data);

                        if (dataStart != -1)
                        {
                            BlowfishCTREncryptDecrypt? blowfish = new();
                            ToolsImpl? toolsImpl = new();
                            uint compressedSize = tableOfContent.CompressedSize;
                            uint fileSize = tableOfContent.Size;
                            int userData = archive.BARHeader.UserData;
#if DEBUG
                            LoggerAccessor.LogInfo("[RunUnBAR] - Encrypted Content Detected!, Running Decryption.");
                            LoggerAccessor.LogInfo($"CompressedSize - {compressedSize}");
                            LoggerAccessor.LogInfo($"Size - {fileSize}");
                            LoggerAccessor.LogInfo($"dataStart - 0x{dataStart:X}");
                            LoggerAccessor.LogInfo($"UserData - 0x{userData:X}");
#endif
                            byte[] SignatureIV = BitConverter.GetBytes(toolsImpl.BuildSignatureIv((int)fileSize, (int)compressedSize, dataStart, userData));

                            // If you want to ensure little-endian byte order explicitly, you can reverse the array
                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(SignatureIV);

                            byte[] EncryptedHeaderSHA1 = new byte[24];

                            // Copy the first 24 bytes from the source array to the destination array
                            Buffer.BlockCopy(data, 4, EncryptedHeaderSHA1, 0, EncryptedHeaderSHA1.Length);

                            byte[]? DecryptedHeaderSHA1 = blowfish.EncryptionProxyInit(EncryptedHeaderSHA1, SignatureIV);

                            if (DecryptedHeaderSHA1 != null)
                            {
                                string verificationsha1 = VariousUtils.ByteArrayToHexString(DecryptedHeaderSHA1);
#if DEBUG
                                LoggerAccessor.LogInfo($"SignatureHeader - {verificationsha1}");
#endif
                                // Create a new byte array to store the remaining content
                                byte[]? FileBytes = new byte[data.Length - 28];

                                // Copy the content after the first 28 bytes to the new array
                                Array.Copy(data, 28, FileBytes, 0, FileBytes.Length);

                                string sha1 = toolsImpl.ValidateSha1(FileBytes);

                                if (sha1 == verificationsha1[..^8]) // We strip the original file Compression size.
                                {
                                    toolsImpl.IncrementIVBytes(SignatureIV, 3);

                                    FileBytes = blowfish.InitiateCTRBuffer(FileBytes, SignatureIV);

                                    if (FileBytes != null)
                                    {
                                        try
                                        {
                                            FileBytes = toolsImpl.ComponentAceEdgeZlibDecompress(FileBytes);
                                        }
                                        catch (Exception ex)
                                        {
                                            LoggerAccessor.LogError($"[RunUnBar] - Errored out when processing Encryption Proxy encrypted content - {ex}");
                                            FileBytes = data;
                                        }

                                        fileStream.Write(FileBytes, 0, FileBytes.Length);
                                        fileStream.Close();
                                    }
                                    else
                                    {
                                        LoggerAccessor.LogWarn($"[RunUnBAR] - Encrypted file failed to decrypt, Writing original data.");
                                        fileStream.Write(data, 0, data.Length);
                                        fileStream.Close();
                                    }
                                }
                                else
                                {
                                    LoggerAccessor.LogWarn($"[RunUnBAR] - Encrypted file (SHA1 - {sha1}) has been tempered with! (Reference SHA1 - {verificationsha1.Substring(0, verificationsha1.Length - 8)}), Aborting decryption.");
                                    fileStream.Write(data, 0, data.Length);
                                    fileStream.Close();
                                }
                            }

                            blowfish = null;
                            toolsImpl = null;
                        }
                        else
                        {
                            LoggerAccessor.LogError("[RunUnBAR] - Encrypted data not found in BAR or false positive! Decryption has failed.");
                            fileStream.Write(data, 0, data.Length);
                            fileStream.Close();
                        }
                    }
                    else
                    {
                        fileStream.Write(data, 0, data.Length);
                        fileStream.Close();
                    }
                }
#if DEBUG
                LoggerAccessor.LogInfo("Extracted file {0}", new object[1]
                {
                    Path.GetFileName(path)
                });
#endif
            }

            tableOfContent = null;
        }

        public void ExtractToFileBarVersion2(byte[] Key, BARArchive archive, HashedFileName FileName, string outDir)
        {
            TOCEntry? tableOfContent = archive.TableOfContents[FileName];
            ToolsImpl? toolsImpl = new();
            string path = string.Empty;
            if (!string.IsNullOrEmpty(tableOfContent.Path))
            {
                string str = tableOfContent.Path.Replace('/', Path.DirectorySeparatorChar);
                path = string.Format("{0}{1}{2}", outDir, Path.DirectorySeparatorChar, str);
            }
            byte[] data = tableOfContent.GetData(archive.GetHeader().Flags);
            if (tableOfContent.Compression == CompressionMethod.Encrypted)
            {
#if DEBUG
                LoggerAccessor.LogInfo("[RunUnBAR] - Encrypted Content Detected!, Running Decryption.");
                LoggerAccessor.LogInfo($"Key - {VariousUtils.ByteArrayToHexString(Key)}");
                LoggerAccessor.LogInfo($"IV - {VariousUtils.ByteArrayToHexString(tableOfContent.IV)}");
#endif
                // Why we not use the ICSharp version, it turns out, there is 2 variant of edgezlib. ICSharp mostly handle the older type.
                // While packing sharc, original script used the zlib1.dll, which we have in c# equivalent, so we use it.
                // Weird issues can happen when decompressing some files if not using this version.

                byte[]? FileBytes = null;

                try
                {
                    FileBytes = toolsImpl.ComponentAceEdgeZlibDecompress(toolsImpl.ProcessLibsecureXTEABlocks(data, Key, tableOfContent.IV));
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[RunUnBar] - Errored out when processing XTEA Proxy encrypted content - {ex}");
                    FileBytes = data;
                }

                using (MemoryStream memoryStream = new(FileBytes))
                {
                    string registeredExtension = string.Empty;

                    try
                    {
                        registeredExtension = FileTypeAnalyser.Instance.GetRegisteredExtension(FileTypeAnalyser.Instance.Analyse(memoryStream));
                    }
                    catch (Exception)
                    {
                        registeredExtension = ".unknown";
                    }

                    if (path == string.Empty)
                        path = string.Format("{0}{1}{2:X8}{3}", outDir, Path.DirectorySeparatorChar, FileName.Value, registeredExtension);

                    string? outdirectory = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(outdirectory))
                    {
                        Directory.CreateDirectory(outdirectory);

                        using (FileStream fileStream = File.Open(path, (FileMode)2))
                        {
                            fileStream.Write(FileBytes, 0, FileBytes.Length);
                            fileStream.Close();
                        }
                    }

                    memoryStream.Flush();
                }
            }
            else
            {
                using (MemoryStream memoryStream = new(data))
                {
                    string registeredExtension = string.Empty;

                    try
                    {
                        registeredExtension = FileTypeAnalyser.Instance.GetRegisteredExtension(FileTypeAnalyser.Instance.Analyse(memoryStream));
                    }
                    catch (Exception)
                    {
                        registeredExtension = ".unknown";
                    }

                    if (path == string.Empty)
                        path = string.Format("{0}{1}{2:X8}{3}", outDir, Path.DirectorySeparatorChar, FileName.Value, registeredExtension);

                    string? outdirectory = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(outdirectory))
                    {
                        Directory.CreateDirectory(outdirectory);

                        using (FileStream fileStream = File.Open(path, (FileMode)2))
                        {
                            fileStream.Write(data, 0, data.Length);
                            fileStream.Close();
                        }
                    }

                    memoryStream.Flush();
                }
            }
#if DEBUG
            LoggerAccessor.LogInfo("Extracted file {0}", new object[1]
            {
                Path.GetFileName(path)
            });
#endif
            tableOfContent = null;
            toolsImpl = null;
        }

        private static byte[]? ExtractNumOfFilesBigEndian(byte[] input)
        {
            if (input == null || input.Length < 20)
            {
                LoggerAccessor.LogError("[ExtractNumOfFiles] - Input byte array must have at least 20 bytes.");
                return null;
            }

            // Create a new byte array to store the first 4 bytes from the last 20 bytes.
            byte[] result = new byte[4];
            // Copy the first 4 bytes from the last 20 bytes into the result array.
            Array.Copy(input, input.Length - 20, result, 0, 4);
            result = Org.BouncyCastle.util.EndianTools.ReverseEndiannessInChunks(result, 4);

            return result;
        }


        private static int FindDataPosInBinary(byte[] data1, byte[] data2)
        {
            for (int i = 0; i < data1.Length - data2.Length + 1; i++)
            {
                bool found = true;
                for (int j = 0; j < data2.Length; j++)
                {
                    if (data1[i + j] != data2[j])
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                    return i;
            }

            return -1; // Data2 not found in Data1
        }
    }
}
