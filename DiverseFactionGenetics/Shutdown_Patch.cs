using Verse;
using HarmonyLib;
using RimWorld;
using System.IO;
using TMPro;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Steamworks;
using System;

namespace DiverseFactionGenetics
{
    [HarmonyPatch(typeof(Root), nameof(Root.Shutdown))]
    public class Shutdown_Patch
    {
        [HarmonyPrefix]
        public static void PreFix()
        {
            
            var path = Path.Combine(GenFilePaths.ModsFolderPath, DiverseFactionGeneticsMod.ModName, VersionControl.CurrentMajor.ToString() + "." + VersionControl.CurrentMinor.ToString(), "Defs/XenotypeDefs/");
            Directory.CreateDirectory(path);
            DirectoryInfo xenoDir = new DirectoryInfo(path);

            var XenoFiles = Directory.GetFiles(xenoDir.FullName).ToList();
            if (xenoDir == null || XenoFiles.Count == 0)
            {
                return;
            }

            foreach (FileInfo saveFile in GenFilePaths.AllSavedGameFiles)
            {
                Log.Error("SaveFileName: "+saveFile.FullName);
                XmlDocument doc = new XmlDocument();
                doc.Load(saveFile.FullName);
                var nodes = doc.SelectNodes("/savegame/game/world/info/name");
                foreach (XmlNode node in nodes)
                {
                    Log.Error("NodeInnerText: "+node.InnerText);
                    var cleansedWorldName = DiverseFactionGeneticsMod.cleanseWorldName(node.InnerText);
                    foreach (string xenoFile in XenoFiles.ToList())
                    {
                        var fileName = Path.GetFileNameWithoutExtension(xenoFile).Split('_')[0];
                        Log.Error($"Comparing {fileName} to {cleansedWorldName}");
                        if (fileName == cleansedWorldName)
                        {
                            XenoFiles.Remove(xenoFile);
                        }
                    }
                }
            }

            if (XenoFiles.Count > 0)
            {
                Log.Error("[ShutdownPatch] Cleaning up xenotype files that are unused.");
                foreach (var files in XenoFiles)
                {
                    File.Delete(files);
                    Log.Error($"[ShutdownPatch] Deleting Xenotype: {files}");
                }
            }
        }
    }
}
