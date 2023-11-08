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
        private static DiverseFactionGeneticsSettings settings => LoadedModManager.GetMod<DiverseFactionGeneticsMod>().Settings;

        public static XenotypeDef GenerateProceduralXenotype()
        {
            var generatedXenoDef = new XenotypeDef();
            foreach (var pool in settings.genePools)
            {
                if (pool.CachedXenotype != null)
                {
                    var genesToGen = Rand.RangeInclusive(pool.numberOfGenesToGenerate.min, pool.numberOfGenesToGenerate.max);
                    List<GeneDef> copyOfPool = new List<GeneDef>();
                    for (int i=0; i<15; i++) {
                        copyOfPool = pool.CachedXenotype.genes.TakeRandom(genesToGen).ToList();
                        var totalMeta = copyOfPool.Aggregate(0,(totalmeta, nextGene) => totalmeta += nextGene.biostatMet);
                        if (totalMeta >= -3 && totalMeta <= 3) {
                            break;
                        }
                    }
                    if (copyOfPool.Count == 0) {
                        Log.Error($"[DiverserFactionGenetics] HOW IS THE GENE POOL for {pool.referenceXenotypeName} EMPTY?!?!?!?");
                    }
                    for (int i = 1; i <= genesToGen; i++)
                    {
                        if (i < pool.numberOfGenesToGenerate.min || (i >= pool.numberOfGenesToGenerate.min && Rand.Range(0, 101) <= pool.chancePerGene))
                        {
                            bool conflict = false;
                            GeneDef gene = copyOfPool.RandomElement();
                            foreach (GeneDef g in generatedXenoDef.genes)
                            {
                                if (gene.ConflictsWith(g))
                                {
                                    conflict = true;
                                }
                            }
                            if (!conflict)
                            {
                                generatedXenoDef.genes.Add(gene);
                            }
                            else {
                                i--;
                            }
                            copyOfPool.Remove(gene);
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
                }
            }
            string name = GeneUtility.GenerateXenotypeNameFromGenes(generatedXenoDef.genes);
            generatedXenoDef.defName = DiverseFactionGeneticsMod.cleanseWorldName(Find.World.info.name)+"_"+DiverseFactionGeneticsMod.cleanseWorldName(name);
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
                doc.Save(Path.Combine(directoryPath, generatedXenoDef.defName+".xml"));
                DefDatabase<XenotypeDef>.Add(generatedXenoDef);
            }
            return generatedXenoDef;

        }
    }
}
