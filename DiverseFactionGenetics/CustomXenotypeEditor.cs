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
    public class CustomXenotypeEditor : Dialog_CreateXenotype
    {
        private Action callback;

        public CustomXenotypeEditor(int index, Action callback) : base(index, callback) { 
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

        protected override void PostXenotypeOnGUI(float curX, float curY)
        {
        }

        protected override void DrawSearchRect(Rect rect)
        {
            base.DrawSearchRect(rect);
            ignoreRestrictions = true;
        }
    }
}
