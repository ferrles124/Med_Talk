using HarmonyLib;
using StardewValley;

namespace MedTalk
{
    [HarmonyPatch(typeof(Game1), "drawDialogue", new[] { typeof(Dialogue) })]
    public class Game1_DrawDialogue_Patch
    {
        public static bool Prefix(Dialogue dialogue)
        {
            if (dialogue == null || dialogue.dialogues == null || dialogue.dialogues.Count == 0)
                return true;

            var first = dialogue.dialogues[0];
            if (first != null && first.Text != null && first.Text.StartsWith("skip#"))
                return false;

            return true;
        }
    }
}
