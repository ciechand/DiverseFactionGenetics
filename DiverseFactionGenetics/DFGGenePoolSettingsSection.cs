using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;
using System.Runtime.CompilerServices;

namespace DiverseFactionGenetics
{
    public class DFGGenePoolSettingsSection : IExposable
    {
        private CustomXenotype cachedXenotype;
        public CustomXenotype CachedXenotype
        {
            get
            {
                if (referenceXenotypeName == null) {
                    return null;
                }
                if (cachedXenotype == null) {
                    reloadXenotype();
                 }
                return cachedXenotype;
            }
            set {
                if (value == null)
                {
                    reloadXenotype();
                }
                else
                {
                    cachedXenotype = value;
                    referenceXenotypeName = value.name;
                }
            }
        }
        public string referenceXenotypeName;
        public float chancePerGene;
        private string chanceBuffer;
        public IntRange numberOfGenesToGenerate;

        public string genePoolName;

        public DFGGenePoolSettingsSection()
        {
        }

        public void AddSettingsBox(ref Listing_Standard SettingsView, int depth) {
            Listing_Standard localSection = SettingsView.BeginSection(150);

            //Adding the Minus Button to the rule section
            Texture2D addTex = ContentFinder<Texture2D>.Get("UI/Buttons/Minus");
            if (localSection.ButtonImage(addTex, 20, 20))
            {
                DiverseFactionGeneticsSettings.removeGenePool(this);
            }

            //Adding the rename box next to the minus box - hence the negative gap and indent
            localSection.Gap(-(20f+2f));
            localSection.Indent(24f);
            localSection.ColumnWidth -= 24;
            string tempStr = localSection.TextEntry(genePoolName);
            //Perhaps we need more validation here eventually?
            genePoolName = tempStr;
            localSection.ColumnWidth += 24;
            localSection.Outdent(24f);

            //Adding the dropdown for selecting the genepool
            AddGenePoolDropdown(ref localSection);

            //Adding the percentage Chance Text Box
            localSection.TextFieldNumericLabeled<float>("Percent Chance Per gene chosen will get added to the pool: ",ref chancePerGene, ref chanceBuffer, 0, 100);
            
            //Adding the Slider to nominate the number of genes to generate
            Rect rect = localSection.GetRect(28f);
            int maxCount = (referenceXenotypeName == null) ? 0: CachedXenotype.genes.Count();
            Widgets.IntRange(rect, depth+1, ref numberOfGenesToGenerate,0, maxCount);


            SettingsView.EndSection(localSection);
        }

        public void AddGenePoolDropdown(ref Listing_Standard SettingsView) {
            if (SettingsView.ButtonText(referenceXenotypeName, "This Xenotpye will specify the pool of genes that this section will pull from.")) {
                
                //Adding a Menu option for editing the selected xenotype
                string LabelForEditOption = (referenceXenotypeName == null) ? "Create New GenePool": "Edit Selected GenePool";
                List<FloatMenuOption> fm = new List<FloatMenuOption>(){
                    new FloatMenuOption(LabelForEditOption, delegate
                    {
                        Find.WindowStack.Add(new Dialog_CustomXenotypeEditor(CachedXenotype, -1, delegate { 
                            DiverseFactionGeneticsSettings.cachedXenos = null;
                            cachedXenotype = null;
                            chancePerGene = 0;
                            chanceBuffer = "0";
                            numberOfGenesToGenerate = new IntRange();
                        }));
                    }) 
                };

                //Adding all the custom xenotypes as Menu Options
                List<CustomXenotype> tempXenos = DiverseFactionGeneticsSettings.allCustomXenos;
                foreach (CustomXenotype x in tempXenos.ToList()) {
                    fm.Add(new FloatMenuOption(x.name, delegate {
                        if (referenceXenotypeName != null) {
                            chancePerGene = 0;
                            chanceBuffer = "0";
                            numberOfGenesToGenerate = new IntRange();
                        }
                        CachedXenotype = x;
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(fm));
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look<string>(ref genePoolName, "GenePoolName");
            Scribe_Values.Look<string>(ref referenceXenotypeName, "referenceXenotypeName");
            Scribe_Values.Look<float>(ref chancePerGene, "chancePerGene");
            Scribe_Values.Look<IntRange>(ref numberOfGenesToGenerate, "numberOfGenesToGenerate");
        }

        private void reloadXenotype() {
            string path = GenFilePaths.AbsFilePathForXenotype(referenceXenotypeName);
            if (!GameDataSaveLoader.TryLoadXenotype(path, out CustomXenotype tempXeno))
            {
                Log.Error($"Could not Load Xenotype matching the name: {referenceXenotypeName}");
            }
            cachedXenotype = tempXeno;
        }
    }
}
