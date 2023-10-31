using HarmonyLib;
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using System;

namespace DiverseFactionGenetics
{
    [HarmonyPatch(typeof(FactionGenerator), nameof(FactionGenerator.NewGeneratedFaction))]
    public class NewGeneratedFactionPatch
    {
        private static DiverseFactionGeneticsSettings settings => LoadedModManager.GetMod<DiverseFactionGeneticsMod>().Settings;

        [HarmonyPostfix]
        public static void Postfix(ref Faction __result)
        {
            if (!__result.def.humanlikeFaction) {
                return;
            }
            if (__result.def.xenotypeSet != null)
            {
                var xenoChanceList = Traverse.Create(__result.def.xenotypeSet).Field("xenotypeChances").GetValue<List<XenotypeChance>>();
                XenotypeDef generatedXenoDef = null;
                if (settings.genePools.Count >= 0)
                {
                    generatedXenoDef = new XenotypeDef();
                    foreach (var pool in settings.genePools)
                    {
                        if (pool.CachedXenotype != null)
                        {
                            List<GeneDef> copyOfPool = pool.CachedXenotype.genes.ToList();
                            var genesToGen = Rand.RangeInclusive(pool.numberOfGenesToGenerate.min, pool.numberOfGenesToGenerate.max);
                            for (int i = 1; i <= genesToGen; i++)
                            {
                                GeneDef gene = copyOfPool[Rand.Range(0, copyOfPool.Count)];
                                if (Rand.Range(0, 101) <= pool.chancePerGene)
                                {
                                    generatedXenoDef.genes.Add(gene);
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
                    generatedXenoDef.defName = GeneUtility.GenerateXenotypeNameFromGenes(generatedXenoDef.genes);
                    Log.Error(generatedXenoDef.defName);
                    generatedXenoDef.label = generatedXenoDef.defName;
                    generatedXenoDef.inheritable = true;
                    generatedXenoDef.iconPath = DefDatabase<XenotypeIconDef>.AllDefs.RandomElement().texPath;
                    generatedXenoDef.descriptionShort = "A XenoDef with genes based on the rules defined in settings.";
                }
                if (generatedXenoDef != null && generatedXenoDef.ConfigErrors().Count() == 0)
                {
                    if (DefDatabase<XenotypeDef>.AllDefs.Any(x => x.defName == generatedXenoDef.defName)) {
                        generatedXenoDef.defName += "+";
                        generatedXenoDef.label = generatedXenoDef.defName;
                    }
                    DefDatabase<XenotypeDef>.Add(generatedXenoDef);
                    xenoChanceList.Clear();
                    xenoChanceList.Add(new XenotypeChance(generatedXenoDef, 100f));
                    Traverse.Create(__result.def.xenotypeSet).Field("xenotypeChances").SetValue(xenoChanceList);

                }
                else {
                    foreach (string error in generatedXenoDef.ConfigErrors()) { 
                        Log.Error(error);
                    }
                    if (generatedXenoDef == null)
                    {
                        Log.Error("GeneratedXenoDef is Null... HOW?!");
                    }
                    xenoChanceList.Clear();
                    xenoChanceList.Add(new XenotypeChance(XenotypeDefOf.Baseliner, 100));
                    Traverse.Create(__result.def.xenotypeSet).Field("xenotypeChances").SetValue(xenoChanceList);
                }
                
            }
        }
    }
}
