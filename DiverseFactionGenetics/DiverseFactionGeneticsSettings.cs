using RimWorld;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace DiverseFactionGenetics
{
    public class DiverseFactionGeneticsSettings : ModSettings
    {
        public static UnityEngine.Vector2 settingsScrollPosition;

        public static float optionsViewRectHeight;

        public static TraitGenSettings tgSetting = TraitGenSettings.Default;
        public enum TraitGenSettings { Default, OnlyForced, None }

        public List<DFGGenePoolSettingsSection> genePools = new List<DFGGenePoolSettingsSection>();

        public static List<CustomXenotype> cachedXenos = null;
        public static List<CustomXenotype> allCustomXenos
        {
            get {
                if (cachedXenos == null)
                {
                    cachedXenos = new List<CustomXenotype>();
                    foreach (FileInfo item in GenFilePaths.AllCustomXenotypeFiles.OrderBy((FileInfo f) => f.LastWriteTime))
                    {
                        string filePath = GenFilePaths.AbsFilePathForXenotype(Path.GetFileNameWithoutExtension(item.Name));
                        PreLoadUtility.CheckVersionAndLoad(filePath, ScribeMetaHeaderUtility.ScribeHeaderMode.Xenotype, delegate
                        {
                            if (GameDataSaveLoader.TryLoadXenotype(filePath, out var xenotype))
                            {
                                cachedXenos.Add(xenotype);
                            }
                            else {
                                Log.Error($"Failed to load xenotype: {item.Name}");
                            }
                        }, skipOnMismatch: true);
                    }
                }
                return cachedXenos;
            }
        }

        //Display members
        private static float generalSettingsHeight = 70f;
        private static float rulesHeight = 150f+20f;

        public DiverseFactionGeneticsSettings() {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<TraitGenSettings>(ref tgSetting, "tgSetting", defaultValue: TraitGenSettings.Default);
            Scribe_Collections.Look<DFGGenePoolSettingsSection>(ref genePools,"genePools", LookMode.Deep);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            bool flag = optionsViewRectHeight > inRect.height;
            Rect viewRect = new Rect(inRect.x, inRect.y, inRect.width - (flag ? 26f : 0f), generalSettingsHeight+(genePools.Count*rulesHeight));
            Widgets.BeginScrollView(inRect, ref settingsScrollPosition, viewRect);
            Listing_Standard SettingsView = new Listing_Standard();
            Rect rect = new Rect(viewRect.x, viewRect.y, viewRect.width, generalSettingsHeight);
            SettingsView.Begin(rect);
            if (SettingsView.ButtonTextLabeled("Trait Generation Setting", tgSetting.ToString()))
            {
                List<FloatMenuOption> fm = new List<FloatMenuOption>();
                foreach (TraitGenSettings setting in Enum.GetValues(typeof(TraitGenSettings)))
                {
                    fm.Add(new FloatMenuOption(setting.ToString(), delegate () { tgSetting = setting; }));
                }
                Find.WindowStack.Add(new FloatMenu(fm));
            }
            SettingsView.GapLine();
            Texture2D addTex = ContentFinder<Texture2D>.Get("UI/Buttons/Plus");
            if (SettingsView.ButtonImage(addTex, 20, 20)) {
                //This needs FIXED
                var tempGenePool = new DFGGenePoolSettingsSection();
                Find.WindowStack.Add(new Dialog_CreateGenePoolSettingsSection(ref tempGenePool));
                genePools.Add(tempGenePool);
            }
            generalSettingsHeight = SettingsView.CurHeight;
            if (genePools.Any(g => g.referenceXenotypeName == null))
            {
                SettingsView.Gap(-22f);
                SettingsView.Indent(24f);
                SettingsView.Label("Please Make Sure All Rules have Assigned Xenotype Pools");
                SettingsView.Outdent(24f);
            }
            SettingsView.End();
            SettingsView.Begin(new Rect(viewRect.x, viewRect.y+generalSettingsHeight, viewRect.width, 999999f));
            AddAllGenePools(ref SettingsView);
            SettingsView.End();
            optionsViewRectHeight = SettingsView.CurHeight;
            Widgets.EndScrollView();
        }

        public void AddAllGenePools(ref Listing_Standard SettingsView) {
            foreach (var (s, index) in genePools.ToList().Select((item, index) => (item, index))) {
                s.AddSettingsBox(ref SettingsView, index);
                SettingsView.Gap();
            }
        }

        public static void removeGenePool(DFGGenePoolSettingsSection item) {
            LoadedModManager.GetMod<DiverseFactionGeneticsMod>().Settings.genePools.Remove(item);
        }
    }
}
