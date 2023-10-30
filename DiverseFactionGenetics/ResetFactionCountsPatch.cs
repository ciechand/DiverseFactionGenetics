using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace DiverseFactionGenetics
{
    [HarmonyPatch(typeof(Page_CreateWorldParams), "ResetFactionCounts")]
    public class ResetFactionCountsPatch
    {

        [HarmonyPostfix]
        public static void Postfix(List<FactionDef> ___factions) {
            var generatedXeno = DefDatabase<XenotypeDef>.AllDefs.FirstOrDefault(xd => xd.defName == "GeneratedXenotype");
            foreach (FactionDef f in ___factions) {
                if (f.humanlikeFaction){
                    if (f.xenotypeSet != null)
                    {
                        var xenoChanceList = Traverse.Create(f.xenotypeSet).Field("xenotypeChances").GetValue<List<XenotypeChance>>();
                        xenoChanceList.Clear();
                        xenoChanceList.Add(new XenotypeChance(generatedXeno, 100f));
                    }
                    else {
                        f.xenotypeSet = new XenotypeSet();
                        var xenoChanceList = Traverse.Create(f.xenotypeSet).Field("xenotypeChances").GetValue<List<XenotypeChance>>();
                        xenoChanceList.Clear();
                        xenoChanceList.Add(new XenotypeChance(generatedXeno, 100f));
                    }
                }
            }
        }
    }
}
