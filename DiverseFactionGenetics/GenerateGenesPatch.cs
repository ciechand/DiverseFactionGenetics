using HarmonyLib;
using RimWorld;
using System;
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
        private static Random rand = new Random((int)System.DateTime.Now.Ticks);

        [HarmonyPrefix]
        public static bool PreFix(Pawn pawn, XenotypeDef xenotype, PawnGenerationRequest request) {
            if (ModsConfig.BiotechActive)
            {
                settings = LoadedModManager.GetMod<DiverseFactionGeneticsMod>().Settings;

                if (settings.genePools.Count == 0)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        public static void PostFix(Pawn pawn, XenotypeDef xenotype, PawnGenerationRequest request) {
            if (pawn.genes == null) {
                return;
            }
            if (ModsConfig.BiotechActive) {
                settings = LoadedModManager.GetMod<DiverseFactionGeneticsMod>().Settings;
                if (settings.genePools.Count >= 0 && request.ForcedCustomXenotype == null && request.AllowedDevelopmentalStages != DevelopmentalStage.Newborn)
                    if (request.Faction == null || request.Faction.def == null || request.Faction.IsPlayer)
                    {
                        foreach (var pool in settings.genePools)
                        {
                            if (pool.CachedXenotype != null)
                            {
                                List<GeneDef> copyOfPool = pool.CachedXenotype.genes.ToList();
                                var genesToGen = rand.Next(pool.numberOfGenesToGenerate.min, pool.numberOfGenesToGenerate.max);
                                for (int i = 1; i <= genesToGen; i++)
                                {
                                    GeneDef gene = copyOfPool[rand.Next(0, copyOfPool.Count)];
                                    if (rand.Next(0, 101) <= pool.chancePerGene)
                                    {
                                        pawn.genes.AddGene(gene, false);
                                        copyOfPool.Remove(gene);
                                        if (gene.prerequisite != null && pawn.genes.GetGene(gene.prerequisite) == null)
                                        {
                                            pawn.genes.AddGene(gene.prerequisite, false);
                                            if (gene.prerequisite.prerequisite != null)
                                            {
                                                Log.Error("PreRequisite has Prerequisite, gonna need to recursively check...... this could be a problem.....");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        pawn.genes.xenotypeName = GeneUtility.GenerateXenotypeNameFromGenes(pawn.genes.Xenotype.genes);
                        pawn.genes.iconDef = DefDatabase<XenotypeIconDef>.AllDefs.RandomElement();
                    }
                    else {
                        if (request.Faction.def == null) {
                            Log.Error("Faction def is null");
                            return;
                        }
                        if (request.Faction.def.xenotypeSet == null)
                        {
                            Log.Error("Faction def is null");
                            return;
                        }
                        if (request.Faction.def.xenotypeSet[0] == null)
                        {
                            Log.Error("Faction def is null");
                            return;
                        }
                        if (request.Faction.def.xenotypeSet[0].xenotype == null)
                        {
                            Log.Error("Faction def is null");
                            return;
                        }
                        if (request.Faction.def.xenotypeSet[0].xenotype.genes == null)
                        {
                            Log.Error("Faction def is null");
                            return;
                        }
                        pawn.genes.SetXenotype(request.Faction.def.xenotypeSet[0].xenotype);
                    }
                }
            }
        }
    }