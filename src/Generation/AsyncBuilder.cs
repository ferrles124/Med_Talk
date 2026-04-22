using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
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

        private Dialogue CreateDialogue(string text, NPC npc)
        {
            try
            {
                var constructors = typeof(Dialogue).GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                Log.Info($"Found {constructors.Length} constructors for Dialogue");
                
                foreach (var ctor in constructors)
                {
                    var parameters = ctor.GetParameters();
                    var paramTypes = string.Join(", ", parameters.Select(p => p.ParameterType.Name));
                    Log.Info($"  Constructor with params: {paramTypes}");
                    
                    if (parameters.Length == 2)
                    {
                        if (parameters[0].ParameterType == typeof(string) && 
                            parameters[1].ParameterType == typeof(NPC))
                        {
                            return (Dialogue)ctor.Invoke(new object[] { text, npc });
                        }
                        else if (parameters[0].ParameterType == typeof(NPC) && 
                                 parameters[1].ParameterType == typeof(string))
                        {
                            return (Dialogue)ctor.Invoke(new object[] { npc, text });
                        }
                    }
                    else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                    {
                        return (Dialogue)ctor.Invoke(new object[] { text });
                    }
                }
                
                if (constructors.Length > 0)
                {
                    var firstCtor = constructors[0];
                    var parameters = firstCtor.GetParameters();
                    if (parameters.Length == 2)
                    {
                        return (Dialogue)firstCtor.Invoke(new object[] { text, npc });
                    }
                    else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                    {
                        return (Dialogue)firstCtor.Invoke(new object[] { text });
                    }
                }
                
                Log.Error($"No suitable constructor found for Dialogue");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error($"CreateDialogue error: {ex.Message}");
                return null;
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
                    var dialogue = CreateDialogue(dialogueText, _speakingNpc);
                    if (dialogue != null)
                    {
                        _speakingNpc.CurrentDialogue.Push(dialogue);
                        Game1.drawDialogue(_speakingNpc);
                    }
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
