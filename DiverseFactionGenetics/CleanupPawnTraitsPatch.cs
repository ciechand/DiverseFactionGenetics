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
    [HarmonyPatch(typeof(PawnGenerator), "GenerateTraits")]
    public class CleanupPawnTraitsPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, PawnGenerationRequest request)
        {
            if (DiverseFactionGeneticsSettings.tgSetting == DiverseFactionGeneticsSettings.TraitGenSettings.Default)
            {
                return true;
            }
            return false;
        }

        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, PawnGenerationRequest request)
        {
            if (DiverseFactionGeneticsSettings.tgSetting == DiverseFactionGeneticsSettings.TraitGenSettings.OnlyForced)
            {
                if (pawn.story.Childhood != null && pawn.story.Childhood.forcedTraits != null)
                {
                    foreach (BackstoryTrait t in pawn.story.Childhood.forcedTraits)
                    {
                        pawn.story.traits.GainTrait(new Trait(t.def, t.degree, forced: true));
                    }
                }

                if (pawn.story.Adulthood != null && pawn.story.Adulthood.forcedTraits != null)
                {
                    foreach (BackstoryTrait t in pawn.story.Adulthood.forcedTraits)
                    {
                        if (!pawn.story.traits.allTraits.Any(bt => bt.def == t.def))
                        {
                            pawn.story.traits.GainTrait(new Trait(t.def, t.degree, forced: true));
                        }
                    }
                }

                if (pawn.kindDef.forcedTraits != null)
                {
                    foreach (TraitRequirement t in pawn.kindDef.forcedTraits)
                    {
                        if (!pawn.story.traits.allTraits.Any(bt => bt.def == t.def))
                        {
                            pawn.story.traits.GainTrait(new Trait(t.def, t.degree ?? 0, forced: true));
                        }
                    }
                }

                if (request.ForcedTraits != null)
                {
                    foreach (TraitDef t in request.ForcedTraits)
                    {
                        if (!pawn.story.traits.allTraits.Any(bt => bt.def == t))
                        {
                            pawn.story.traits.GainTrait(new Trait(t, 0, forced: true));
                        }
                    }
                }
            }
        }
    }
}
