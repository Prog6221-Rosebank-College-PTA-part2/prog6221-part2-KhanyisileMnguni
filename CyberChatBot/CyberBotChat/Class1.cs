using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberChatBot
{
    /// <summary>
    /// Core chatbot engine for Part 2.
    /// Implements keyword recognition, random responses, memory, 
    /// sentiment detection, conversation flow, and error handling.
    /// </summary>
    public class ChatbotEngine
    {
        // ─────────────────────────────────────────────
        //  Memory
        // ─────────────────────────────────────────────
        private readonly Dictionary<string, string> _memory = new Dictionary<string, string>();
        private string _lastTopic = "";

        // ─────────────────────────────────────────────
        //  Keyword → Multiple Responses
        // ─────────────────────────────────────────────
        private readonly Dictionary<string, List<string>> _keywordResponses = new Dictionary<string, List<string>>
     {
     ["password"] = new List<string>
    {
        "Make sure to use strong, unique passwords for each account. Avoid using personal details in your passwords — use a mix of uppercase, lowercase, numbers, and symbols!",
        "A good password is at least 12 characters long and doesn't contain your name or birthday. Consider using a passphrase like 'Coffee!Runs$Deep2024'.Never reuse passwords across different sites.",
        "If one account is breached, all others become vulnerable. A password manager can help you keep track!"
    },
    ["phishing"] = new List<string>
    {
        "Phishing attacks trick you into revealing personal info by pretending to be a trusted source. Always check the sender's email address carefully before clicking any links.",
        "Be cautious of emails with urgent language like 'Your account will be closed!' Legitimate companies rarely pressure you this way.Hover over links before clicking — the real URL often reveals whether it's a scam.",
        "When in doubt, go directly to the official website instead."
    },
    ["scam"] = new List<string>
    {
        "Online scams often appear as too-good-to-be-true offers or urgent warnings. Verify before you trust — legitimate businesses won't rush you into decisions.",
        "If someone contacts you unexpectedly asking for payment via gift cards or wire transfer, that's almost always a scam. Stop and verify through official channels.",
        "Report scams to the South African Police Service (SAPS) or the South African Banking Risk Information Centre (SABRIC) to protect others."
    },
    ["privacy"] = new List<string>
    {
        "Protect your privacy by reviewing the permissions you grant apps. Many apps request access they don't actually need.",
        "Use a VPN on public Wi-Fi to keep your browsing private. Public networks are easy targets for attackers to intercept your data.Check your social media privacy settings regularly.",
        "Oversharing personal details can help attackers craft convincing scams targeting you specifically."
    }, 
    ["malware"] = new List<string>
    {
        "Malware is malicious software that can damage your system or steal your data. Keep your antivirus up to date and avoid downloading software from unknown sources.",
        "Ransomware is a type of malware that locks your files and demands payment. Regular backups are your best defence — store them offline or in the cloud.",
        "Signs of malware infection include slow performance, unexpected pop-ups, and programs opening on their own. Run a full antivirus scan if you notice these."
    },
    ["safe browsing"] = new List<string>
    {
        "Always look for HTTPS in the address bar before entering sensitive information. The padlock icon means the connection is encrypted.",
        "Keep your browser and its extensions updated. Outdated software is one of the most common entry points for attackers.",
        "Avoid clicking on pop-up ads — they can lead to malicious sites. Use an ad blocker for extra protection."
    },
    ["two-factor"] = new List<string>
    {
        "Two-factor authentication (2FA) adds an extra layer of security beyond your password. Even if your password is stolen, 2FA can stop attackers from getting in.",
        "Use an authenticator app like Google Authenticator instead of SMS-based 2FA when possible — SMS can be intercepted through SIM-swapping attacks.",
        "Enable 2FA on your most important accounts first: email, banking, and social media. These are the accounts attackers target most."
    },
    ["social engineering"] = new List<string>
    {
        "Social engineering exploits human psychology rather than technical weaknesses. Attackers build trust before asking for something — always verify identities independently.",
        "Pretexting is a social engineering tactic where attackers invent a scenario to extract information. Be wary of unexpected calls from 'IT support' or 'your bank'.",
        "Never share your OTP (one-time PIN) with anyone, even someone claiming to be from your bank. Legitimate institutions will never ask for it."
    }
};

        private readonly Random random = new Random();

        // ─────────────────────────────────────────────
        //  Sentiment Keywords
        // ─────────────────────────────────────────────
        private readonly List<string> _worriedWords = new List<string>
        { "worried", "scared", "afraid", "nervous", "anxious", "fear", "terrified", "panic" };
        private readonly List<string> _frustratedWords = new List<string>
        { "frustrated", "annoyed", "confused", "lost", "don't understand", "dont understand", "difficult", "hard", "complicated" };
        private readonly List<string> _curiousWords = new List<string>
        { "curious", "interested", "want to know", "tell me more", "wondering", "how does", "what is", "explain" };

        // ─────────────────────────────────────────────
        //  Conversation Flow Keywords
        // ─────────────────────────────────────────────
        private readonly List<string> _followUpPhrases = new List<string>
        { "tell me more", "explain more", "give me another", "more info", "more details", "go on", "continue", "and then", "what else", "please explain" };

        // ─────────────────────────────────────────────
        //  Public API
        // ─────────────────────────────────────────────

        /// <summary>
        /// Stores a user detail in memory (e.g., name, favourite topic).
        /// </summary>
        public void Remember(string key, string value)
        {
            _memory[key.ToLower()] = value;
        }

        /// <summary>
        /// Retrieves a remembered value, or null if not found.
        /// </summary>
        public string Recall(string key)
        {
            return _memory.TryGetValue(key.ToLower(), out var val) ? val : null;
        }

        /// <summary>
        /// Main chat method. Processes input and returns a contextual response.
        /// Handles sentiment, follow-ups, keywords, memory, and fallback.
        /// </summary>
        public string GetResponse(string userInput)
        {
            string input = userInput.ToLower().Trim();
            string userName = Recall("name") ?? "there";

            // ── Conversation Flow — follow-up handling ────────
            if (_followUpPhrases.Any(p => input.Contains(p)))
            {
                if (!string.IsNullOrEmpty(_lastTopic) && _keywordResponses.ContainsKey(_lastTopic))
                {
                    string followUp = GetRandomResponse(_lastTopic);
                    return $"Sure! Here's more about {_lastTopic}: {followUp}";
                }
                return "I'd love to tell you more! What topic would you like to explore? You can ask about passwords, phishing, scams, privacy, malware, and more.";
            }

            // ── Sentiment Detection ───────────────────────────
            string sentimentPrefix = DetectSentiment(input, userName);

            // ── Memory — detect topic interest ───────────────
            foreach (var keyword in _keywordResponses.Keys)
            {
                if (input.Contains(keyword))
                {
                    // Remember their favourite topic if they express interest
                    if (input.Contains("interest") || input.Contains("like") || input.Contains("love") || input.Contains("favourite"))
                    {
                        Remember("favourite_topic", keyword);
                        return $"Great! I'll remember that you're interested in {keyword}. It's a crucial part of staying safe online.{GetRandomResponse(keyword)}";
                    }

                    _lastTopic = keyword;
                    string response = GetRandomResponse(keyword);
                    return sentimentPrefix + response;
                }
            }

            // ── General conversation ──────────────────────────────
            if (input.Contains("how are you"))
                return "I'm running smoothly and ready to help you stay safe online! How are you doing today?";

            if (input.Contains("purpose") || input.Contains("what can you do") || input.Contains("help"))
                return $"Hi {userName}! I'm your Cybersecurity Awareness Bot. You can ask me about:\n • Passwords\n  • Phishing\n  • Scams\n  • Privacy\n  • Malware\n  • Safe Browsing\n • Two-Factor Authentication\n  • Social Engineering\n ";

            if (input.Contains("name"))
            {
                string remembered = Recall("name");
                if (remembered != null)
                    return $"I remember you — you're {remembered}! How can I help you today?";
            }

            // ── Memory recall — refer back to favourite topic ─────
            string favTopic = Recall("favourite topic");
            if (favTopic != null && (input.Contains("remember") || input.Contains("what do you know about me")))
                return $"I remember that you're interested in {favTopic}! As someone focused on {favTopic}, you might want to {GetQuickTip(favTopic)}";

            // ── Error Handling / Fallback ─────────────────────
            return "I'm not sure I understand that. Could you try rephrasing? You can ask me about passwords, phishing, scams, privacy, malware, or safe browsing.";
        }

        /// <summary>
        /// Selects a random response for a given keyword topic.
        /// </summary>
        private string GetRandomResponse(string keyword)
        {
            var responses = _keywordResponses[keyword];
            return responses[random.Next(responses.Count)];
        }

        /// <summary>
        /// Detects the user's sentiment and returns an empathetic prefix.
        /// </summary>
        private string DetectSentiment(string input, string userName)
        {
            if (_worriedWords.Any(w => input.Contains(w)))
                return "It's completely understandable to feel that way — cybersecurity can feel overwhelming. You're taking the right step by learning about it! Here's what you need to know:  ";

            if (_frustratedWords.Any(w => input.Contains(w)))
                return "Don't worry, this stuff can be confusing at first. Let me break it down simply for you:  ";

            if (_curiousWords.Any(w => input.Contains(w)))
                return "Great question! I love your curiosity. Here's the answer:  ";

            return "";
        }

        /// <summary>
        /// Returns a short personalised tip for a topic (used in memory recall).
        /// </summary>
        private string GetQuickTip(string topic)
        {
            switch (topic)
            {
                case "password": return "review your passwords and enable a password manager if you haven't already.";
                case "privacy": return "check your social media privacy settings this week.";
                case "phishing": return "double-check any suspicious emails before clicking links.";
                case "scam": return "verify any unexpected contacts through official channels before responding.";
                case "malware": return "run a full antivirus scan regularly to stay protected.";
                case "two-factor": return "enable 2FA on your most important accounts today.";
                case "social engineering": return "stay alert to unsolicited requests for personal information.";
                default: return "keep learning and stay safe online!";
            }
        }
    }
}
