using HarmonyLib;
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using System;
using UnityEngine.UIElements;

namespace DiverseFactionGenetics
{
    [HarmonyPatch(typeof(FactionGenerator), nameof(FactionGenerator.NewGeneratedFaction))]
    public class NewGeneratedFactionPatch
    {
        private static DiverseFactionGeneticsSettings settings => LoadedModManager.GetMod<DiverseFactionGeneticsMod>().Settings;

        [HarmonyPrefix]
        public static void Prefix(ref FactionGeneratorParms parms)
        {
            if (!parms.factionDef.humanlikeFaction) {
                return;
            }
            //var xenoChanceList = Traverse.Create(parms.factionDef.xenotypeSet).Field("xenotypeChances").GetValue<List<XenotypeChance>>();
            XenotypeDef generatedXenoDef = null;
            if (settings.genePools.Count >= 0)
            {
                generatedXenoDef = DiverseFactionGeneticsUtilities.GenerateProceduralXenotype();
            }
            var xenoChanceList = new List<XenotypeChance>();
            if (generatedXenoDef != null && generatedXenoDef.ConfigErrors().Count() == 0)
            {
                xenoChanceList.Clear();
                xenoChanceList.Add(new XenotypeChance(generatedXenoDef, 100f));
                parms.factionDef.xenotypeSet = new XenotypeSet();
                Traverse.Create(parms.factionDef.xenotypeSet).Field("xenotypeChances").SetValue(xenoChanceList);

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
                parms.factionDef.xenotypeSet = new XenotypeSet();
                Traverse.Create(parms.factionDef.xenotypeSet).Field("xenotypeChances").SetValue(xenoChanceList);
            }
            FactionDef tempFacDef = DefDatabase<FactionDef>.GetNamed(parms.factionDef.defName);
            Traverse.Create(tempFacDef.xenotypeSet).Field("xenotypeChances").SetValue(xenoChanceList);
            var fa = Find.World.GetComponent<DiverseFactionGenetics_FactionAssociations>();
            if (fa.associations == null) {
                fa.associations = new Dictionary<string, string>();
            }
            fa.associations.Add(parms.factionDef.defName, generatedXenoDef.defName);
        }
        
    }
}
