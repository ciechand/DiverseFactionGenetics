using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DiverseFactionGenetics
{
    public class Dialog_CustomXenotypeEditor : Dialog_CreateXenotype
    {
        private Action callback;

        public Dialog_CustomXenotypeEditor(CustomXenotype xenotype, int index, Action callback) : base(index, callback) {
            if (xenotype != null)
            {
                xenotypeName = xenotype.name;
                xenotypeNameLocked = true;
                SelectedGenes.Clear();
                SelectedGenes.AddRange(xenotype.genes);
                iconDef = xenotype.IconDef;
                OnGenesChanged();
            }

            ignoreRestrictions = true;
            closeOnAccept = true;
            this.callback = callback;

        }

        protected override void DoBottomButtons(Rect rect)
        {
            if (Widgets.ButtonText(new Rect(rect.xMax - GeneCreationDialogBase.ButSize.x, rect.y, GeneCreationDialogBase.ButSize.x, GeneCreationDialogBase.ButSize.y), AcceptButtonLabel) && CanAccept())
            {
                Accept();
                callback();
                Close();
            }
            if (Widgets.ButtonText(new Rect(rect.x, rect.y, GeneCreationDialogBase.ButSize.x, GeneCreationDialogBase.ButSize.y), "Close".Translate()))
            {
                Close();
                callback();
            }
        }

        protected override bool CanAccept()
        {
            string text = xenotypeName;
            if (text != null && text.Trim().Length == 0)
            {
                Messages.Message("XenotypeNameCannotBeEmpty".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }
            if (!WithinAcceptableBiostatLimits(showMessage: true))
            {
                return false;
            }
            if (!SelectedGenes.Any())
            {
                Messages.Message("MessageNoSelectedGenes".Translate(), null, MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }
            if (GenFilePaths.AllCustomXenotypeFiles.EnumerableCount() >= 200)
            {
                Messages.Message("MessageTooManyCustomXenotypes", null, MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }
            return true;
        }

        protected override void Accept()
        {
            AccessTools.Method(typeof(Dialog_CreateXenotype), "AcceptInner").Invoke(this, null);
        }

        protected override void PostXenotypeOnGUI(float curX, float curY)
        {
        }


        protected override void DrawSearchRect(Rect rect) {
            Rect rect2 = new Rect(rect.width - 300f - searchWidgetOffsetX, 11f, 300f, 24f);
            quickSearchWidget.OnGUI(rect2, UpdateSearchResults);
            ignoreRestrictions = true;
        }

    }
}
