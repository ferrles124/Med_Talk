using HarmonyLib;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace MedTalk
{
    [HarmonyPatch(typeof(NPC), nameof(NPC.CurrentDialogue), MethodType.Getter)]
    public class NPC_CurrentDialogue_Patch
    {
        public static void Postfix(ref NPC __instance, ref Stack<Dialogue> __result)
        {
            if (__result == null || __result.Count == 0) return;

            var topDialogue = __result.Peek();
            if (topDialogue == null || topDialogue.dialogues == null || topDialogue.dialogues.Count == 0) return;

            var firstLine = topDialogue.dialogues[0];
            if (firstLine == null) return;

            if (firstLine.Text == "[[generate]]" || firstLine.Text.StartsWith("[[generate]]"))
            {
                __result.Pop();
                
                // VALLEY TALK'TAN ALINAN KRİTİK SATIR:
                Game1.currentSpeaker = __instance;
                
                AsyncBuilder.Instance.RequestNpcBasic(__instance, "default", "");
                __result.Clear();
                return;
            }
        }
    }
}
