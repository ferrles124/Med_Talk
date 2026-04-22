using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StardewValley;

namespace MedTalk
{
    internal class DialogueBuilder
    {
        public static DialogueBuilder Instance => _instance ??= new DialogueBuilder();
        private static DialogueBuilder _instance;

        public ModConfig Config { get; internal set; }
        public bool LlmDisabled { get; set; } = false;

        private Dictionary<string, Character> _characters = new();
        private Random _random = new();

        public Character GetCharacter(NPC instance)
        {
            if (instance == null) return null;
            if (!_characters.ContainsKey(instance.Name))
                _characters[instance.Name] = new Character(instance.Name, instance);
            return _characters[instance.Name];
        }

        public Character GetCharacterByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || !_characters.ContainsKey(name))
                return null;
            return _characters[name];
        }

        internal async Task<string> Generate(NPC instance, string dialogueKey, string originalLine = "")
        {
            var character = GetCharacter(instance);
            var theLine = await character.CreateDialogue(originalLine);
            return theLine ?? "...";
        }

        internal async Task<string> GenerateResponse(NPC instance, List<ConversationElement> conversation, bool dontSkipNext = false)
        {
            var character = GetCharacter(instance);
            var theLine = await character.CreateResponse(conversation);
            return theLine ?? "...";
        }

        internal async Task<string> GenerateGift(NPC instance, StardewValley.Object gift, int taste)
        {
            var character = GetCharacter(instance);
            var theLine = await character.CreateGiftResponse(gift, taste);
            return theLine ?? "...";
        }

        internal void AddConversation(NPC npc, string dialogue, bool isPlayerLine = false) { }
        internal void ClearContext() { }
        internal void AddDialogueLine(NPC instance, List<StardewValley.DialogueLine> lines) { }
        internal void AddEventLine(NPC instance, IEnumerable<NPC> actors, string festivalName, List<StardewValley.DialogueLine> lines) { }
        internal void AddOverheardLine(NPC otherNpc, NPC instance, List<StardewValley.DialogueLine> lines) { }

        internal bool PatchNpc(NPC n, int probability = 4, bool retainResult = false)
        {
            if (LlmDisabled || Config == null || !Config.EnableMod) return false;
            if (Config.DisabledCharactersList.Contains(n.Name)) return false;
            if (probability < 4 && _random.Next(4) >= probability) return false;
            return true;
        }
    }
}
