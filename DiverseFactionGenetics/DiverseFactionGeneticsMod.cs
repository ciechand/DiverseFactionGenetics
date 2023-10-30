using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;

namespace DiverseFactionGenetics
{
    public class DiverseFactionGeneticsMod : Mod
    {
        public DiverseFactionGeneticsSettings Settings;


        public DiverseFactionGeneticsMod(ModContentPack content) : base(content) {
            Settings = GetSettings<DiverseFactionGeneticsSettings>();
            new Harmony("Cryonyx.DiverseFactionGenetics.rimworld").PatchAll();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() => "DiverseFactionGenetics";

    }
}
