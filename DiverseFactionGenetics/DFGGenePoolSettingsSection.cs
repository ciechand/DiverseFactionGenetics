using RimWorld;
using Verse;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace DiverseFactionGenetics
{
    public class DFGGenePoolSettingsSection : IExposable
    {
        public CustomXenotype genePool = null;
        public string referenceXenotypeName
        {
            get { 
                if (genePool == null)
                {
                    return "";
                }
                return genePool.name;
            }
        }
        public float chancePerGene;
        public float test;
        private string chanceBuffer;
        public IntRange numberOfGenesToGenerate;

        public string genePoolName;

        public DFGGenePoolSettingsSection(string name)
        {   
            genePoolName = name;
        }

        public void AddSettingsBox(ref Listing_Standard ls) {
            if (genePool == null) {
                genePool = DiverseFactionGeneticsSettings.allCustomXenos.First();
            }
            Listing_Standard localSection = ls.BeginSection(200);
            string tempStr = localSection.TextEntry(genePoolName);
            genePoolName = tempStr;
            Texture2D addTex = ContentFinder<Texture2D>.Get("UI/Buttons/Minus");
            if (localSection.ButtonImage(addTex, 20, 20))
            {
                DiverseFactionGeneticsSettings.removeGenePool(this);
            }
            AddGenePoolDropdown(ref localSection);
            localSection.TextFieldNumericLabeled<float>("Percent Chance Per gene chosen will get added to the pool",ref chancePerGene, ref chanceBuffer, 0, 100);
            localSection.IntRange(ref numberOfGenesToGenerate,0,genePool.genes.Count());
            ls.EndSection(localSection);
        }

        public void AddGenePoolDropdown(ref Listing_Standard ls) {
            if (ls.ButtonText(referenceXenotypeName, "This xenotype will specify the pool of genes that this section will pull from.")) {
                List<FloatMenuOption> fm = new List<FloatMenuOption>();
                fm.Add(new FloatMenuOption("Xenotype Editor", delegate
                {
                    Find.WindowStack.Add(new CustomXenotypeEditor(-1, delegate { DiverseFactionGeneticsSettings.cachedXenos = null; }));
                }));
                foreach (CustomXenotype x in DiverseFactionGeneticsSettings.allCustomXenos) {
                    fm.Add(new FloatMenuOption(x.name, delegate {
                        genePool = x;
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(fm));
            }
        }

        public void ExposeData()
        {
            throw new NotImplementedException();
        }
    }
}
