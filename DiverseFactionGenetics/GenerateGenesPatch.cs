using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DiverseFactionGenetics
{
    [HarmonyPatch(typeof(PawnGenerator), "GenerateGenes")]
    public class GenerateGenesPatch
    {
        private static DiverseFactionGeneticsSettings settings;

        [HarmonyPrefix]
        public static bool PreFix(Pawn pawn, XenotypeDef xenotype, PawnGenerationRequest request) {
            settings = LoadedModManager.GetMod<DiverseFactionGeneticsMod>().Settings;
            if (settings.genePools.Count == 0)
            {
                return true;
            }
            return false;
        }

        [HarmonyPostfix]
        public static void PostFix(Pawn pawn, XenotypeDef xenotype, PawnGenerationRequest request)
        {
            if (pawn.genes == null)
            {
                return;
            }
            settings = LoadedModManager.GetMod<DiverseFactionGeneticsMod>().Settings;
            if (settings.genePools.Count >= 0 && request.ForcedCustomXenotype == null && request.AllowedDevelopmentalStages != DevelopmentalStage.Newborn)
            {
                XenotypeDef generatedXenoDef = null;
                if (request.Faction == null || request.Faction.def == null || request.Faction.IsPlayer)
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
                                    bool conflict = false;
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
                    generatedXenoDef.label = generatedXenoDef.defName;
                    generatedXenoDef.inheritable = true;
                    generatedXenoDef.iconPath = DefDatabase<XenotypeIconDef>.AllDefs.RandomElement().texPath;
                    generatedXenoDef.descriptionShort = "A XenoDef with genes based on the rules defined in settings.";

                    if (DefDatabase<XenotypeDef>.AllDefs.Any(x => x.defName == generatedXenoDef.defName))
                    {
                        generatedXenoDef.defName += "+";
                        generatedXenoDef.label = generatedXenoDef.defName;
                    }
                    DefDatabase<XenotypeDef>.Add(generatedXenoDef);

                    pawn.genes.SetXenotype(generatedXenoDef);
                }
                else
                {
                    if (request.Faction.def.xenotypeSet == null)
                    {
                        Log.Warning($"Faction def set is of type: {request.Faction.def.defName}");
                        Log.Warning("XenotypeSet is null");
                        return;
                    }
                    if (request.Faction.def.xenotypeSet[0] == null)
                    {
                        Log.Warning("FIrst index of Xenotype Set is null");
                        return;
                    }
                    if (request.Faction.def.xenotypeSet[0].xenotype == null)
                    {
                        Log.Warning("Sexnotype is null");
                        return;
                    }
                    if (request.Faction.def.xenotypeSet[0].xenotype.genes == null)
                    {
                        Log.Warning("Genes are null");
                        return;
                    }
                    pawn.genes.SetXenotype(request.Faction.def.xenotypeSet[0].xenotype);
                }
               
            }
        }
    }
}