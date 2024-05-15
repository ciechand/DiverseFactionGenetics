using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DiverseFactionGenetics
{
    public class DiverseFactionGenetics_FactionAssociations : WorldComponent
    {
        public Dictionary<string, string> associations;

        public DiverseFactionGenetics_FactionAssociations(World world) : base(world) {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref associations, "factionAssociations", LookMode.Value);
        }

        public override void FinalizeInit()
        {
            foreach (FactionDef f in DefDatabase<FactionDef>.AllDefs)
            {
                if (associations.TryGetValue(f.defName, out string xeno))
                {
                    if (f.xenotypeSet == null)
                    {
                        f.xenotypeSet = new XenotypeSet();
                    }
                    //Log.Error($"[DiverFactionGenetics] Xenotype of {DefDatabase<XenotypeDef>.GetNamed(xeno)} associated with {f.defName}");
                    var xenoChanceList = new List<XenotypeChance>
                    {
                        new XenotypeChance(DefDatabase<XenotypeDef>.GetNamed(xeno),100)
                    };
                    Traverse.Create(f.xenotypeSet).Field("xenotypeChances").SetValue(xenoChanceList);
                    
                    
                }
            }
        }
    }
}
