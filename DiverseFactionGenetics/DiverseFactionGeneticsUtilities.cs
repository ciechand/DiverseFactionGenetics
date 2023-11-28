using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Verse;

namespace DiverseFactionGenetics
{
    public class DiverseFactionGeneticsUtilities
    {

        private static readonly int maxRerolls = 20;
        private static DiverseFactionGeneticsSettings settings => LoadedModManager.GetMod<DiverseFactionGeneticsMod>().Settings;

        public static XenotypeDef GenerateProceduralXenotype()
        {
            var generatedXenoDef = new XenotypeDef();
            Tuple<XenotypeDef, int> closestXeno = null;
            for(int j=0; j< maxRerolls; j++)
            {
                generatedXenoDef = new XenotypeDef();
                foreach (var pool in settings.genePools)
                {
                    if (pool == null)
                    {
                        Log.Error("Gene Pool is null.");
                    }
                    if (pool.CachedXenotype != null && pool.CachedXenotype.genes != null)
                    {
                        var genesToGen = Rand.RangeInclusive(pool.numberOfGenesToGenerate.min, pool.numberOfGenesToGenerate.max);
                        if (genesToGen == 0 || pool.CachedXenotype.genes.Count == 0)
                        {
                            continue;
                        }
                        List<GeneDef> cachedPool = pool.CachedXenotype.genes.ToList();

                        for (int i = 0; i < genesToGen; i++)
                        {
                            if (i >= pool.numberOfGenesToGenerate.min-1 && Rand.RangeInclusive(0, 100) >= pool.chancePerGene) {
                                continue;
                            }
                            if (cachedPool.Count == 0)
                            {
                                Log.Error($"[DiverserFactionGenetics] HOW IS THE GENE POOL for {pool.referenceXenotypeName} EMPTY?!?!?!?");
                                Log.Error($"Tried to generate {genesToGen} genes, but only got to {i}");
                                break;
                            }
                            GeneDef gene = cachedPool.RandomElement();
                            generatedXenoDef.genes.Add(gene);
                            cachedPool.Remove(gene);
                            var copyOfPool = cachedPool.ToList();
                            foreach (GeneDef g in copyOfPool)
                            {
                                if (gene.ConflictsWith(g))
                                {
                                    cachedPool.Remove(g);
                                }
                            }
                            if (gene.prerequisite != null && !generatedXenoDef.genes.Any(g => gene.prerequisite == g))
                            {
                                generatedXenoDef.genes.Add(gene.prerequisite);
                                if (gene.prerequisite.prerequisite != null)
                                {
                                    Log.Error("PreRequisite has Prerequisite, gonna need to recursively check...... this could be a problem.....");
                                }
                            }
                        }
                    }
                    else
                    {
                        Log.Error("Cached XenoType is null");
                    }
                }
                int totalMetaAbs = Math.Abs(generatedXenoDef.AllGenes.Aggregate(0, (totalmeta, nextGene) => totalmeta += nextGene.biostatMet));
                if (totalMetaAbs > 4)
                {
                    if (closestXeno == null)
                    {
                        closestXeno = new Tuple<XenotypeDef, int>(generatedXenoDef, totalMetaAbs);
                    }
                    else {
                        if (totalMetaAbs < closestXeno.Item2) {
                            closestXeno = new Tuple<XenotypeDef, int>(generatedXenoDef, totalMetaAbs);
                        }
                    }
                    if (j == maxRerolls-1) {
                        generatedXenoDef = closestXeno.Item1;
                    }
                    //Log.Error($"generated metabolism is outside the allowed values: {totalMetaAbs}");
                }
                else {
                    break;
                }
            }
            string name = GeneUtility.GenerateXenotypeNameFromGenes(generatedXenoDef.genes);
            generatedXenoDef.defName = DiverseFactionGeneticsMod.cleanseWorldName(Find.World.info.name) + "_" + DiverseFactionGeneticsMod.cleanseWorldName(name);
            generatedXenoDef.label = name;
            generatedXenoDef.inheritable = true;
            generatedXenoDef.iconPath = DefDatabase<XenotypeIconDef>.AllDefs.RandomElement().texPath;
            generatedXenoDef.descriptionShort = "A XenoDef with genes based on the rules defined in settings.";
            if (DefDatabase<XenotypeDef>.AllDefs.Any(x => x.defName == generatedXenoDef.defName))
            {
                generatedXenoDef = DefDatabase<XenotypeDef>.GetNamed(generatedXenoDef.defName);
            }
            else
            {
                string directoryPath = Path.Combine(new DirectoryInfo(GenFilePaths.ModsFolderPath).ToString(), DiverseFactionGeneticsMod.ModName, "Defs", "XenotypeDefs");
                DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }
                XDocument doc = new XDocument(new XElement("Defs"));
                XElement xElement = new XElement("XenotypeDef", new XElement("defName", generatedXenoDef.defName), new XElement("label", generatedXenoDef.label), new XElement("description", generatedXenoDef.descriptionShort), new XElement("descriptionShort", generatedXenoDef.descriptionShort), new XElement("iconPath", generatedXenoDef.iconPath), new XElement("inheritable", generatedXenoDef.inheritable), new XElement("genes"));
                XElement xElement2 = xElement.Element("genes");
                foreach (GeneDef gene in generatedXenoDef.genes)
                {
                    xElement2.Add(new XElement("li", gene.defName));
                }
                doc.Element("Defs").Add(xElement);
                doc.Save(Path.Combine(directoryPath, generatedXenoDef.defName + ".xml"));
                DefDatabase<XenotypeDef>.Add(generatedXenoDef);
            }
            return generatedXenoDef;

        }

        public static void LoadXenotypeFilesIgnoringModList(string path, ScribeMetaHeaderUtility.ScribeHeaderMode mode, Action loadAct)
        {
            try
            {
                Scribe.loader.InitLoadingMetaHeaderOnly(path);
                ScribeMetaHeaderUtility.LoadGameDataHeader(mode, logVersionConflictWarning: false);
                Scribe.loader.FinalizeLoading();
            }
            catch (Exception ex)
            {
                Log.Warning("Exception loading " + path + ": " + ex);
                Scribe.ForceStop();
            }
            loadAct();
        }
    }
}
