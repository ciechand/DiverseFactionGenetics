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
    [HarmonyPatch(typeof(FactionIdeosTracker), nameof(FactionIdeosTracker.ChooseOrGenerateIdeo))]
    public class ChooseOrGenerateIdeoPatch
    {
        [HarmonyPostfix]
        public static void Postfix(FactionIdeosTracker __instance, Faction ___faction)
        {
            var prefXeno = __instance.PrimaryIdeo.PreferredXenotypes;
            var newXenotype = ___faction.def.xenotypeSet[0].xenotype;
            var prefXenoContained = prefXeno.FirstOrDefault(x => x.defName == newXenotype.defName);
            if (prefXenoContained == null && __instance.PrimaryIdeo.HasPrecept(PreceptDefOf.PreferredXenotype))
            {
                __instance.PrimaryIdeo.PreferredXenotypes.Add(newXenotype);
            }
        }
    }
}
