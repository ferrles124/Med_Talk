using StardewModdingAPI;
using StardewValley;

namespace MedTalk
{
    public static class TextInputManager
    {
        private static IModHelper _helper;
        
        public static void Initialize(IModHelper helper)
        {
            _helper = helper;
        }
        
        public static void RequestTextInput(string prompt, NPC npc)
        {
            AsyncBuilder.Instance.RequestNpcBasic(npc, "mobile_input", prompt);
        }
    }
}
