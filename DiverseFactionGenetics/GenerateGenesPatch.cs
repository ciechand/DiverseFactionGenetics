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
                if (request.Faction == null || request.Faction.def == null || request.Faction.IsPlayer)
                {
                    XenotypeDef generatedXenoDef = DiverseFactionGeneticsUtilities.GenerateProceduralXenotype();

                    pawn.genes.SetXenotype(generatedXenoDef);
                }
                else
                {
                    if (request.Faction.def.xenotypeSet == null)
                    {
                        Log.Warning($"Faction def set is of type: {request.Faction.def.label}");
                        Log.Warning("XenotypeSet is null");
                        return;
                    }
                    if (request.Faction.def.xenotypeSet.Count == 0)
                    {
                        Log.Warning($"Faction def set is of type: {request.Faction.def.label}");
                        Log.Warning("XenoTypeSet is empty.");
                        return;
                    }
                    if ( request.Faction.def.xenotypeSet[0] == null)
                    {
                        Log.Warning($"Faction def set is of type: {request.Faction.def.label}");
                        Log.Warning("FIrst index of Xenotype Set is null");
                        return;
                    }
                    if (request.Faction.def.xenotypeSet[0].xenotype == null)
                    {
                        Log.Warning($"Faction def set is of type: {request.Faction.def.label}");
                        Log.Warning("xenotype set xenotype is null");
                        return;
                    }
                    if (request.Faction.def.xenotypeSet[0].xenotype.genes == null)
                    {
                        Log.Warning($"Faction def set is of type: {request.Faction.def.label}");
                        Log.Warning("Genes are null");
                        return;
                    }
                    //Log.Error($"Generating {request.Faction.Name} pawn as {request.Faction.def.label}");
                    pawn.genes.SetXenotype(request.Faction.def.xenotypeSet[0].xenotype);
                }
               
            }
        }
    }
}