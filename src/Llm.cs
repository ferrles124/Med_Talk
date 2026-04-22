using System;
using System.Threading.Tasks;

namespace MedTalk
{
    internal abstract class Llm
    {
        internal static Llm Instance { get; private set; }

        internal static void SetLlm(Type llmType, string url = "", string promptFormat = "", string apiKey = "", string modelName = null)
        {
            try
            {
                Instance = (Llm)Activator.CreateInstance(llmType, apiKey, modelName, url);
                Log.Info($"Llm.Instance created: {Instance?.GetType().Name}");
            }
            catch (Exception ex)
            {
                Log.Error($"SetLlm failed: {ex.Message}");
                throw;
            }
        }

        protected string _apiKey;
        protected string _modelName;
        protected string _url;

        public abstract Task<string> GenerateDialogue(string prompt);
    }
}
