using HarmonyLib;
using StardewValley;

namespace MedTalk
{
    [HarmonyPatch(typeof(NPC), nameof(NPC.checkAction))]
    public class NPC_CheckAction_Patch
    {
        public static bool Prefix(ref NPC __instance, ref bool __result, Farmer who, GameLocation l)
        {
            if (__instance.IsInvisible ||
                !who.CanMove ||
                !DialogueBuilder.Instance.PatchNpc(__instance))
            {
                return true;
            }

            DialogueBuilder.Instance.ClearContext();
            TextInputManager.RequestTextInput($"What do you want to say to {__instance.displayName}?", __instance);

            __result = false;
            return false;
        }
    }
}
