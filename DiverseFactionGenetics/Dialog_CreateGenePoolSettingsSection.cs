using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DiverseFactionGenetics
{
    public class Dialog_CreateGenePoolSettingsSection : Dialog_Rename
    {
        DFGGenePoolSettingsSection targetPool;
        public Dialog_CreateGenePoolSettingsSection(DFGGenePoolSettingsSection genePool) { 
            targetPool = genePool;
            closeOnClickedOutside = false;
            doCloseX = false;
            closeOnAccept = true;
        }

        protected override AcceptanceReport NameIsValid(string name)
{
            return base.NameIsValid(name) && !LoadedModManager.GetMod<DiverseFactionGeneticsMod>().Settings.genePools.Any<DFGGenePoolSettingsSection>(gp => gp.genePoolName == name);
        }

        protected override void SetName(string name)
        {
            targetPool.genePoolName = name;
        }

        public void DoWindowContents(Rect inRect) {
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(inRect);
            ls.Label("Please input a distinct name for this gene group.");
            ls.End();
            base.DoWindowContents(inRect);
        }
    }
}
