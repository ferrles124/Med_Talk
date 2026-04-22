using System;
using System.Collections.Generic;
using HarmonyLib;
using StardewModdingAPI;

namespace MedTalk
{
    public partial class ModEntry : Mod
    {
        public static IMonitor SMonitor;
        public static IModHelper SHelper { get; private set; }
        public static ModConfig Config;

        public static Dictionary<string, Type> LlmMap
        {
            get
            {
                if (_llmMap == null)
                {
                    _llmMap = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase)
                    {
                        {"Google", typeof(LlmGemini)}
                    };
                }
                return _llmMap;
            }
        }

        private static Dictionary<string, Type> _llmMap;

        public override void Entry(IModHelper helper)
        {
            SHelper = helper;
            SMonitor = Monitor;

            try { Config = Helper.ReadConfig<ModConfig>(); }
            catch { Config = new ModConfig(); }

            if (!Config.EnableMod) return;

            Log.Initialize(Monitor);
            Log.Info("MedTalk starting...");
            Log.Info($"Provider: {Config.Provider}, Model: {Config.ModelName}");

            if (!LlmMap.TryGetValue(Config.Provider, out var llmType))
            {
                Log.Error($"Invalid provider: {Config.Provider}");
                return;
            }

            Llm.SetLlm(llmType, apiKey: Config.ApiKey, modelName: Config.ModelName, url: Config.ServerAddress);
            Log.Info($"LLM set: {Llm.Instance?.GetType().Name}");

            DialogueBuilder.Instance.Config = Config;
            TextInputManager.Initialize(helper);

            var _ = AsyncBuilder.Instance;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            Log.Info("MedTalk loaded!");
        }
    }
}
