using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace MedTalk
{
    public class Character
    {
        public string Name { get; }
        private NPC _npc;

        public Character(string name, NPC npc)
        {
            Name = name;
            _npc = npc;
        }

        public async Task<string> CreateDialogue(string originalLine)
        {
            if (Llm.Instance == null) return originalLine;
            var prompt = BuildPrompt(originalLine);
            var result = await Llm.Instance.GenerateDialogue(prompt);
            return string.IsNullOrEmpty(result) ? "..." : result;
        }

        public async Task<string> CreateResponse(List<ConversationElement> conversation)
        {
            if (Llm.Instance == null) return "...";
            var prompt = BuildConversationPrompt(conversation);
            var result = await Llm.Instance.GenerateDialogue(prompt);
            return string.IsNullOrEmpty(result) ? "..." : result;
        }

        public async Task<string> CreateGiftResponse(StardewValley.Object gift, int taste)
        {
            if (Llm.Instance == null) return "...";
            var tasteText = taste switch
            {
                0 => "hates",
                1 => "dislikes",
                2 => "is neutral about",
                4 => "likes",
                8 => "loves",
                _ => "reacts to"
            };
            var prompt = $"You are {Name} from Stardew Valley. You {tasteText} receiving {gift.DisplayName} as a gift. Respond in character with 1-2 sentences.";
            var result = await Llm.Instance.GenerateDialogue(prompt);
            return string.IsNullOrEmpty(result) ? "..." : result;
        }

        private string BuildPrompt(string context)
        {
            var config = DialogueBuilder.Instance.Config;
            if (!string.IsNullOrEmpty(config.PromptFormat))
            {
                try
                {
                    return string.Format(config.PromptFormat, Name, context);
                }
                catch { }
            }
            
            var sb = new StringBuilder();
            sb.AppendLine($"You are {Name} from Stardew Valley.");
            sb.AppendLine("Respond naturally and in character. Keep your response short (1-2 sentences).");
            if (!string.IsNullOrEmpty(context))
                sb.AppendLine($"Context: {context}");
            sb.AppendLine("Say something to the farmer.");
            return sb.ToString();
        }

        private string BuildConversationPrompt(List<ConversationElement> conversation)
        {
            var config = DialogueBuilder.Instance.Config;
            if (!string.IsNullOrEmpty(config.PromptFormat))
            {
                try
                {
                    var conversationText = string.Join("\n", conversation.ConvertAll(e => $"{(e.IsPlayerLine ? "Farmer" : Name)}: {e.Text}"));
                    return string.Format(config.PromptFormat, Name, conversationText);
                }
                catch { }
            }
            
            var sb = new StringBuilder();
            sb.AppendLine($"You are {Name} from Stardew Valley.");
            sb.AppendLine("Respond naturally and in character. Keep your response short (1-2 sentences).");
            sb.AppendLine("Conversation so far:");
            foreach (var element in conversation)
            {
                var speaker = element.IsPlayerLine ? "Farmer" : Name;
                sb.AppendLine($"{speaker}: {element.Text}");
            }
            sb.AppendLine($"{Name}:");
            return sb.ToString();
        }

        public void ClearConversationHistory() { }

        internal void AddDialogue(IEnumerable<StardewValley.DialogueLine> dialogues, int year, StardewValley.Season season, int dayOfMonth, int timeOfDay) { }
        internal void AddEventDialogue(List<StardewValley.DialogueLine> dialogues, IEnumerable<NPC> actors, string festivalName, int year, StardewValley.Season season, int dayOfMonth, int timeOfDay) { }
        internal void AddOverheardDialogue(NPC speaker, List<StardewValley.DialogueLine> dialogues, int year, StardewValley.Season season, int dayOfMonth, int timeOfDay) { }
        internal void AddConversation(List<ConversationElement> chatHistory, int year, StardewValley.Season season, int dayOfMonth, int timeOfDay) { }
        internal bool MatchLastDialogue(List<StardewValley.DialogueLine> dialogues) => false;
        internal bool SpokeJustNow() => false;
    }
}
