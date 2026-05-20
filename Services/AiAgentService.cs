using AiAgent2.ChatFormat;
using AiAgent2.LLM;
using AiAgent2.Memory;
using AiAgent2.Tools;

namespace WebApplication1.Services.AiAgent
{
    public class AiAgentService
    {
        private readonly GroqAi _llm;
        private readonly SerperWebSearch _serper;

        // 🔥 KEY CHANGE: per-session memory
        private readonly Dictionary<string, ChatMemory> _sessions = new();

        public AiAgentService()
        {
            var apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY")
                         ?? throw new Exception("GROQ_API_KEY not set.");

            _llm = new GroqAi(apiKey, "llama-3.1-8b-instant");
            _serper = new SerperWebSearch();
        }

        private ChatMemory GetMemory(string sessionId)
        {
            if (!_sessions.ContainsKey(sessionId))
                _sessions[sessionId] = new ChatMemory();

            return _sessions[sessionId];
        }

        public async Task<string> ProcessAsync(string sessionId, string userInput)
        {
            var memory = GetMemory(sessionId);

            // STEP 1 — Decide if search is needed
            string checkPrompt =
                $"User asked: \"{userInput}\".\nDoes this require a websearch? YES or NO only.";

            string check = await _llm.SendAsync(new()
            {
                new("system", "Answer only YES or NO."),
                new("user", checkPrompt)
            });

            bool requiresSearch = check.Trim().StartsWith("Y", StringComparison.OrdinalIgnoreCase);

            if (requiresSearch)
            {
                string searchJson = await _serper.SearchAsync(userInput);

                string summary = await _llm.SendAsync(new()
                {
                    new("system", "Summarise search results into bullet points."),
                    new("user", searchJson)
                });

                string finalPrompt =
                    $"User question: {userInput}\n\n" +
                    $"Search results:\n{summary}\n\n" +
                    $"Answer using ONLY the provided info.";

                memory.AddUser(finalPrompt);

                string reply = await _llm.SendAsync(memory.GetMessages());
                memory.AddAssistant(reply);

                return reply;
            }

            // NORMAL CHAT
            memory.AddUser(userInput);

            string normalReply = await _llm.SendAsync(memory.GetMessages());
            memory.AddAssistant(normalReply);

            return normalReply;
        }
    }
}