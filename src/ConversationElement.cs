namespace MedTalk
{
    public class ConversationElement
    {
        public string Id { get; }
        public string Text { get; }
        public bool IsPlayerLine { get; }

        public ConversationElement(string text, bool isPlayerLine = false)
        {
            Id = System.Guid.NewGuid().ToString();
            Text = text;
            IsPlayerLine = isPlayerLine;
        }
    }
}
