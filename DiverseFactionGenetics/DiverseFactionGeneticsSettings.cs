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

        public List<DFGGenePoolSettingsSection> genePools;

        public static List<CustomXenotype> cachedXenos;
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
                        }, skipOnMismatch: true);
                    }
                }
                return cachedXenos;
            }
        }

        public DiverseFactionGeneticsSettings() {
            genePools = new List<DFGGenePoolSettingsSection>
            {
                new DFGGenePoolSettingsSection("General")
            };
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<TraitGenSettings>(ref tgSetting, "tgSetting", defaultValue: TraitGenSettings.Default);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            bool flag = optionsViewRectHeight > inRect.height;
            Rect viewRect = new Rect(inRect.x, inRect.y, inRect.width - (flag ? 26f : 0f), optionsViewRectHeight);
            Widgets.BeginScrollView(inRect, ref settingsScrollPosition, viewRect);
            Listing_Standard listingStandard = new Listing_Standard();
            Rect rect = new Rect(viewRect.x, viewRect.y, viewRect.width, 999999f);
            listingStandard.Begin(rect);
            if (listingStandard.ButtonTextLabeled("Trait Generation Setting", tgSetting.ToString()))
            {
                List<FloatMenuOption> fm = new List<FloatMenuOption>();
                foreach (TraitGenSettings setting in Enum.GetValues(typeof(TraitGenSettings)))
                {
                    fm.Add(new FloatMenuOption(setting.ToString(), delegate () { tgSetting = setting; }));
                }
                Find.WindowStack.Add(new FloatMenu(fm));
            }
            Texture2D addTex = ContentFinder<Texture2D>.Get("UI/Buttons/Plus");
            if (listingStandard.ButtonImage(addTex, 20, 20)) {
                genePools.Add(new DFGGenePoolSettingsSection("Please Rename"));
            }
            AddAllGenePools(ref listingStandard);
            optionsViewRectHeight = listingStandard.CurHeight;
            listingStandard.End();
            Widgets.EndScrollView();
        }

        public void AddAllGenePools(ref Listing_Standard ls) {
            foreach (DFGGenePoolSettingsSection s in genePools.ToList()) {
                s.AddSettingsBox(ref ls);
            }
        }

        public static void removeGenePool(DFGGenePoolSettingsSection item) {
            if (item.genePoolName == "General")
            {
                //Add an error here explaining that general cannot be removed.
                return;
            }
            LoadedModManager.GetMod<DiverseFactionGeneticsMod>().Settings.genePools.Remove(item);
        }
    }
}
