﻿using System.Text.RegularExpressions;

namespace BackendProject.HomeTools.AFS
{
    public class AFSClass
    {
        public static string MapperHelperFolder = $"{Directory.GetCurrentDirectory()}/static/HomeToolsXMLs";
        private static readonly string UUIDRegexModel = @"[0-9a-fA-F]{8}-[0-9a-fA-F]{8}-[0-9a-fA-F]{8}-[0-9a-fA-F]{8}";
        private static readonly Dictionary<string, string> MappedAFSHashesCache = new();

        public static async Task AFSMapStart(string CurrentFolder, string prefix, string BruteforceUUIDs)
        {
            Match objectmatch = new Regex(UUIDRegexModel).Match(CurrentFolder);

            // Create a list to hold the tasks
            List<Task>? HashMapTasks = new();

            if (objectmatch.Success) // We first map the corresponding object.
            {
                string Objectprefix = $"objects/{objectmatch.Groups[0].Value}/";

                foreach (string ObjectMetaDataRelativePath in new List<string>() { $"{Objectprefix}object.xml", $"{Objectprefix}resources.xml", $"{Objectprefix}localisation.xml" })
                {
                    string text = AFSHash.EscapeString(ObjectMetaDataRelativePath);
                    string CrcHash = AFSHash.ComputeAFSHash(text);

                    // Search for files with names matching the CRC hash, regardless of the extension
                    foreach (string filePath in Directory.GetFiles(CurrentFolder)
                      .Where(path => new Regex($"(?:0X)?{CrcHash}(?:\\.\\w+)?$").IsMatch(Path.GetFileNameWithoutExtension(path)))
                      .ToArray())
                    {
                        string NewfilePath = CurrentFolder + $"/{text}";
                        string? destinationDirectory = Path.GetDirectoryName(NewfilePath);

                        if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
                            Directory.CreateDirectory(destinationDirectory.ToUpper());

                        if (!File.Exists(NewfilePath))
                            File.Move(filePath, NewfilePath.ToUpper());

                        if (File.Exists(NewfilePath))
                            await AFSMap.SubHashMapBatch(CurrentFolder, Objectprefix, File.ReadAllText(NewfilePath));
                    }
                }
            }

            if (BruteforceUUIDs == "on" && Directory.Exists(MapperHelperFolder) && File.Exists(MapperHelperFolder + "/UUIDList.txt"))
            {
                // Open the file for reading
                using StreamReader reader = new(MapperHelperFolder + "/UUIDList.txt");
                string? line = null;

                // Read and display lines from the file until the end of the file is reached
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        objectmatch = new Regex(UUIDRegexModel).Match(line);

                        if (objectmatch.Success) // We first map the corresponding object.
                        {
                            string Objectprefix = $"objects/{objectmatch.Groups[0].Value}/";

                            foreach (string ObjectMetaDataRelativePath in new List<string>() { $"{Objectprefix}object.xml", $"{Objectprefix}resources.xml", $"{Objectprefix}localisation.xml" })
                            {
                                string text = AFSHash.EscapeString(ObjectMetaDataRelativePath);
                                string CrcHash = AFSHash.ComputeAFSHash(text);

                                // Search for files with names matching the CRC hash, regardless of the extension
                                foreach (string filePath in Directory.GetFiles(CurrentFolder)
                                  .Where(path => new Regex($"(?:0X)?{CrcHash}(?:\\.\\w+)?$").IsMatch(Path.GetFileNameWithoutExtension(path)))
                                  .ToArray())
                                {
                                    string NewfilePath = CurrentFolder + $"/{text}";
                                    string? destinationDirectory = Path.GetDirectoryName(NewfilePath);

                                    if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
                                        Directory.CreateDirectory(destinationDirectory.ToUpper());

                                    if (!File.Exists(NewfilePath))
                                        File.Move(filePath, NewfilePath.ToUpper());

                                    if (File.Exists(NewfilePath))
                                        await AFSMap.SubHashMapBatch(CurrentFolder, Objectprefix, File.ReadAllText(NewfilePath));
                                }
                            }
                        }
                    }
                }
            }

            lock (MappedAFSHashesCache)
            {
                foreach (string CrcHash in MappedAFSHashesCache.Keys)
                {
                    // Create a task for each iteration
                    HashMapTasks.Add(Task.Run(async () =>
                    {
                        // Search for files with names matching the CRC hash, regardless of the extension
                        foreach (string filePath in Directory.GetFiles(CurrentFolder)
                          .Where(path => new Regex($"(?:0X)?{CrcHash}(?:\\.\\w+)?$").IsMatch(Path.GetFileNameWithoutExtension(path)))
                          .ToArray())
                        {
                            string NewfilePath = CurrentFolder + $"/{MappedAFSHashesCache[CrcHash]}";
                            string? destinationDirectory = Path.GetDirectoryName(NewfilePath);

                            if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
                                Directory.CreateDirectory(destinationDirectory.ToUpper());

                            if (!File.Exists(NewfilePath))
                                File.Move(filePath, NewfilePath.ToUpper());

                            if (File.Exists(NewfilePath))
                                await AFSMap.SubHashMapBatch(CurrentFolder, prefix, File.ReadAllText(NewfilePath));
                        }
                    }));
                }
            }

            // Wait for all tasks to complete
            await Task.WhenAll(HashMapTasks);

            HashMapTasks = null;
        }

        public static void ScheduledUpdate(object? state)
        {
            lock (MappedAFSHashesCache)
            {
                if (MappedAFSHashesCache.Count == 0) // We have an optimization which is applied in eboot and that we replicate, we store static entries in a cache.
                {
                    MappedAFSHashesCache.Add("4E545585", "\\PLACEHOLDER_N.DDS");
                    MappedAFSHashesCache.Add("4EE3523A", "\\PLACEHOLDER_S.DDS");
                    MappedAFSHashesCache.Add("696E72D6", "\\HATBUBBLE.DDS");
                    MappedAFSHashesCache.Add("D3A7AF9F", "\\__$manifest$__");
                    MappedAFSHashesCache.Add("EDFBFAE9", "\\files.txt");
                }
            }

            if (Directory.Exists(MapperHelperFolder))
            {
                if (File.Exists(MapperHelperFolder + "/SceneList.xml")) // We Need the scenelist to do the scenes AFS map.
                {
                    // Match the patterns in the file content
                    Parallel.ForEach(new List<MappedList>(AFSRegexProcessor.ScanForSceneListPaths(MapperHelperFolder + "/SceneList.xml")), match => {
                        if (!string.IsNullOrEmpty(match.file))
                        {
                            string text = AFSHash.EscapeString(match.file);
                            string CrcHash = AFSHash.ComputeAFSHash(text);
                            lock (MappedAFSHashesCache)
                            {
                                if (!MappedAFSHashesCache.ContainsKey(CrcHash))
                                    MappedAFSHashesCache.Add(CrcHash, text);
                                else
                                    MappedAFSHashesCache[CrcHash] = text;
                            }
                        }
                    });
                }

                if (File.Exists(MapperHelperFolder + "/CoredataHelper.xml")) // This is done at runtime on Eboot, but we need a XML to establish a mapper list.
                {
                    // Match the patterns in the file content
                    Parallel.ForEach(new List<MappedList>(AFSRegexProcessor.ScanForString(File.ReadAllText(MapperHelperFolder + "/CoredataHelper.xml"))), match => {
                        if (!string.IsNullOrEmpty(match.file))
                        {
                            string text = AFSHash.EscapeString(match.file);
                            string CrcHash = AFSHash.ComputeAFSHash(text);
                            lock (MappedAFSHashesCache)
                            {
                                if (!MappedAFSHashesCache.ContainsKey(CrcHash))
                                    MappedAFSHashesCache.Add(CrcHash, text);
                                else
                                    MappedAFSHashesCache[CrcHash] = text;
                            }
                        }
                    });
                }
            }

        }
    }
}
