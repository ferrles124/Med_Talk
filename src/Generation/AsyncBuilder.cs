using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StardewModdingAPI.Events;
using StardewValley;

namespace MedTalk
{
    public class AsyncBuilder
    {
        private static AsyncBuilder _instance = new AsyncBuilder();
        public static AsyncBuilder Instance => _instance;

        private bool _awaitingGeneration = false;
        private GenerationType _awaitedType = GenerationType.None;
        private NPC _speakingNpc = null;
        private string _currentDialogueKey = "";
        private string _originalLine = null;
        private List<ConversationElement> _currentConversation = null;
        private StardewValley.Object _currentGift = null;
        private int _currentTaste = 0;

        public bool AwaitingGeneration => _awaitingGeneration;
        public NPC SpeakingNpc => _speakingNpc;

        private AsyncBuilder()
        {
            ModEntry.SHelper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (_awaitingGeneration && Game1.activeClickableMenu == null)
            {
                _awaitingGeneration = false;
                _ = PerformGeneration();
            }
        }

        private async Task PerformGeneration()
        {
            string dialogueText = "...";
            
            try
            {
                var npc = _speakingNpc;

                switch (_awaitedType)
                {
                    case GenerationType.Basic:
                        dialogueText = await DialogueBuilder.Instance.Generate(npc, _currentDialogueKey, _originalLine);
                        break;
                    case GenerationType.Conversation:
                        dialogueText = await DialogueBuilder.Instance.GenerateResponse(npc, _currentConversation, true);
                        break;
                    case GenerationType.Gift:
                        dialogueText = await DialogueBuilder.Instance.GenerateGift(npc, _currentGift, _currentTaste);
                        break;
                }
            }
            catch (Exception ex)
            {
                ModEntry.SMonitor?.Log($"AsyncBuilder error: {ex.Message}", StardewModdingAPI.LogLevel.Error);
                dialogueText = "...";
            }
            finally
            {
                if (_speakingNpc != null && !string.IsNullOrEmpty(dialogueText))
                {
                    _speakingNpc.CurrentDialogue.Push(new Dialogue(dialogueText, _speakingNpc));
                    Game1.drawDialogue(_speakingNpc);
                }
                Reset();
            }
        }

        private void Reset()
        {
            _awaitingGeneration = false;
            _speakingNpc = null;
            _currentDialogueKey = "";
            _originalLine = null;
            _currentConversation = null;
            _currentGift = null;
            _currentTaste = 0;
            _awaitedType = GenerationType.None;
        }

        internal void RequestNpcBasic(NPC npc, string key, string original)
        {
            if (_awaitingGeneration) return;
            _speakingNpc = npc;
            _currentDialogueKey = key;
            _originalLine = original;
            _awaitedType = GenerationType.Basic;
            _awaitingGeneration = true;
        }

        internal void RequestNpcResponse(NPC npc, List<ConversationElement> conversation)
        {
            if (_awaitingGeneration) return;
            _speakingNpc = npc;
            _currentConversation = conversation;
            _awaitedType = GenerationType.Conversation;
            _awaitingGeneration = true;
        }

        internal void RequestNpcGiftResponse(NPC npc, StardewValley.Object gift, int taste)
        {
            if (_awaitingGeneration) return;
            _speakingNpc = npc;
            _currentGift = gift;
            _currentTaste = taste;
            _awaitedType = GenerationType.Gift;
            _awaitingGeneration = true;
        }
    }

    internal enum GenerationType
    {
        None, Basic, Conversation, Gift
    }
}
