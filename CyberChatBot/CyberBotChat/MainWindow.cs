using System;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CyberChatBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// Handles all UI events, chat bubble rendering, ASCII art display,
    /// voice greeting playback, and wires user input into ChatbotEngine.
    /// </summary>
    public partial class MainWindow : Window
    {
        // ─────────────────────────────────────────────
        //  Fields
        // ─────────────────────────────────────────────

        private readonly ChatbotEngine _engine = new ChatbotEngine();
        private string _userName = "";

        // Update this path if your WAV file moves
        private readonly string _wavPath = @"C:\Users\mngun\source\repos\CyberChatBot\CyberBotChat\Voice message.wav";

              // ASCII art banner matching the Cybersecurity Awareness ChatBot logo
        private readonly string _asciiArt =
" ██████╗██╗   ██╗██████╗ ███████╗██████╗ ███████╗███████╗ ██████╗██╗   ██╗██████╗ ██╗████████╗██╗   ██╗     █████╗ ██╗    ██╗ █████╗ ██████╗ ███████╗███╗   ██╗███████╗███████╗███████╗    ██████╗  ██████╗ ████████╗ \n"+
"██╔════╝╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗██╔════╝██╔════╝██╔════╝██║   ██║██╔══██╗██║╚══██╔══╝╚██╗ ██╔╝    ██╔══██╗██║    ██║██╔══██╗██╔══██╗██╔════╝████╗  ██║██╔════╝██╔════╝██╔════╝    ██╔══██╗██╔═══██╗╚══██╔══╝ \n"+
"██║      ╚████╔╝ ██████╔╝█████╗  ██████╔╝███████╗█████╗  ██║     ██║   ██║██████╔╝██║   ██║    ╚████╔╝     ███████║██║ █╗ ██║███████║██████╔╝█████╗  ██╔██╗ ██║█████╗  ███████╗███████╗    ██████╔╝██║   ██║   ██║   \n"+
"██║       ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗╚════██║██╔══╝  ██║     ██║   ██║██╔══██╗██║   ██║     ╚██╔╝      ██╔══██║██║███╗██║██╔══██║██╔══██╗██╔══╝  ██║╚██╗██║██╔══╝  ╚════██║╚════██║    ██╔══██╗██║   ██║   ██║   \n"+
"╚██████╗   ██║   ██████╔╝███████╗██║  ██║███████║███████╗╚██████╗╚██████╔╝██║  ██║██║   ██║      ██║       ██║  ██║╚███╔███╔╝██║  ██║██║  ██║███████╗██║ ╚████║███████╗███████║███████║    ██████╔╝╚██████╔╝   ██║  \n\n"+
 "╚═════╝   ╚═╝   ╚═════╝ ╚══════╝╚═╝  ╚═╝╚══════╝╚══════╝ ╚═════╝ ╚═════╝ ╚═╝  ╚═╝╚═╝   ╚═╝      ╚═╝       ╚═╝  ╚═╝ ╚══╝╚══╝ ╚═╝  ╚═╝╚═╝  ╚═╝╚══════╝╚═╝  ╚═══╝╚══════╝╚══════╝╚══════╝    ╚═════╝  ╚═════╝    ╚═╝   ";

        // ─────────────────────────────────────────────
        //  Constructor
        // ─────────────────────────────────────────────

        /// <summary>
        /// Initialises the window components and plays the voice greeting on startup.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            PlayVoiceGreeting();
        }

        // ─────────────────────────────────────────────
        //  Voice Greeting
        // ─────────────────────────────────────────────

        /// <summary>
        /// Plays the WAV voice greeting asynchronously when the application launches.
        /// Silently skips playback if the file is missing or unplayable.
        /// </summary>
        public void PlayVoiceGreeting()
        {
            try
            {
                SoundPlayer player = new SoundPlayer(_wavPath);
                player.Play();
            }
            catch
            {
                // Silently skip — no crash if file is missing
            }
        }

        // ─────────────────────────────────────────────
        //  Name Entry
        // ─────────────────────────────────────────────

        /// <summary>
        /// Handles the Enter key press on the name input field,
        /// triggering the same action as clicking the Start Chat button.
        /// </summary>
        public void NameInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                StartChat_Click(sender, e);
        }

        /// <summary>
        /// Validates the user's name, stores it in the chatbot's memory,
        /// switches the UI from the name panel to the chat panel,
        /// and displays the ASCII art banner followed by a personalised welcome message.
        /// </summary>
        public void StartChat_Click(object sender, RoutedEventArgs e)
        {
            string name = NameInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                NameError.Visibility = Visibility.Visible;
                return;
            }

            _userName = name;
            _engine.Remember("name", name);

            // Switch from name entry panel to chat panel
            NamePanel.Visibility = Visibility.Collapsed;
            ChatContainer.Visibility = Visibility.Visible;

            // Display ASCII art banner as the first message
            AddAsciiMessage(_asciiArt);

            // Personalised welcome message
            AddBotMessage(
                "Hi " + _userName + "! Welcome to the Cybersecurity Awareness Bot." +
                "I'm here to help you stay safe online. You can ask me about:\n\n" +
                "Passwords\n" + "Phishing\n" + "Scams & Privacy\n" + "Malware\n" + "Safe Browsing\n" + "Two-Factor Authentication\n" + "Social Engineering\n" + "What would you like to know?");

            UserInput.Focus();
            StatusText.Text = "Chatting as " + _userName;
        }

        // ─────────────────────────────────────────────
        //  Message Sending
        // ─────────────────────────────────────────────

        /// <summary>
        /// Handles the Enter key press in the message input box,
        /// sending the message if the input is not empty.
        /// </summary>
        public void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(UserInput.Text))
                SendMessage();
        }

        /// <summary>
        /// Handles the Send button click event.
        /// </summary>
        public void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        /// <summary>
        /// Reads the user's input, displays it as a user bubble,
        /// retrieves the bot's response from ChatbotEngine,
        /// displays the response as a bot bubble,
        /// and updates the status bar with any remembered topic.
        /// </summary>
        public void SendMessage()
        {
            string userText = UserInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(userText)) return;

            AddUserMessage(userText);
            UserInput.Clear();

            string response = _engine.GetResponse(userText);
            AddBotMessage(response);

            // Update status bar if a favourite topic has been remembered
            string favTopic = _engine.Recall("favourite topic");
            if (favTopic != null)
                StatusText.Text = "Remembered: You're interested in " + favTopic;

            ScrollToBottom();
        }

        // ─────────────────────────────────────────────
        //  Chat Bubble Rendering
        // ─────────────────────────────────────────────

        /// <summary>
        /// Adds a right-aligned chat bubble displaying the user's message.
        /// </summary>
        /// <param name="text">The message text typed by the user.</param>
        public void AddUserMessage(string text)
        {
            UIElement bubble = CreateBubble(
                text,
                new SolidColorBrush(Color.FromRgb(0, 92, 128)),
                Brushes.White,
                isUser: true,
                label: _userName);

            ChatPanel.Children.Add(bubble);
            ScrollToBottom();
        }

        /// <summary>
        /// Adds a left-aligned chat bubble displaying the bot's response.
        /// </summary>
        /// <param name="text">The response text generated by ChatbotEngine.</param>
        public void AddBotMessage(string text)
        {
            UIElement bubble = CreateBubble(
                text,
                new SolidColorBrush(Color.FromRgb(22, 27, 34)),
                new SolidColorBrush(Color.FromRgb(230, 237, 243)),
                isUser: false,
                label: "CyberBot");

            ChatPanel.Children.Add(bubble);
            ScrollToBottom();
        }

        /// <summary>
        /// Adds a special ASCII art bubble using Courier New font so the
        /// block characters render correctly and the logo displays as intended.
        /// </summary>
        /// <param name="art">The ASCII art string to display.</param>
        public void AddAsciiMessage(string art)
        {
            TextBlock artBlock = new TextBlock
            {
                Text = art,
                FontFamily = new FontFamily("Courier New"),
                FontSize = 5.5,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 212, 255)),
                TextWrapping = TextWrapping.NoWrap,
                Margin = new Thickness(8, 10, 8, 6),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            ChatPanel.Children.Add(artBlock);
            ScrollToBottom();
        }

        /// <summary>
        /// Creates a styled chat bubble containing a sender label, message text,
        /// and timestamp. Bubbles are right-aligned for the user and left-aligned
        /// for the bot, with distinct corner rounding to indicate direction.
        /// </summary>
        /// <param name="text">The message content to display inside the bubble.</param>
        /// <param name="background">The background brush for the bubble.</param>
        /// <param name="foreground">The text colour brush for the message.</param>
        /// <param name="isUser">True if this bubble belongs to the user; false for the bot.</param>
        /// <param name="label">The sender name shown above the bubble.</param>
        /// <returns>A UIElement ready to be added to the chat panel.</returns>
        public UIElement CreateBubble(string text, Brush background, Brush foreground, bool isUser, string label)
        {
            // Sender name label
            TextBlock nameLabel = new TextBlock
            {
                Text = label,
                Foreground = isUser
                    ? new SolidColorBrush(Color.FromRgb(0, 180, 230))
                    : new SolidColorBrush(Color.FromRgb(63, 185, 80)),
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(isUser ? 0 : 4, 0, isUser ? 4 : 0, 3),
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left
            };

            // Message text block
            TextBlock messageText = new TextBlock
            {
                Text = text,
                Foreground = foreground,
                FontSize = 15,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 22
            };

            // Rounded bubble border — corners indicate message direction
            Border bubble = new Border
            {
                Background = background,
                CornerRadius = isUser
                    ? new CornerRadius(12, 2, 12, 12)
                    : new CornerRadius(2, 12, 12, 12),
                Padding = new Thickness(14, 10, 14, 10),
                BorderBrush = new SolidColorBrush(Color.FromRgb(48, 54, 61)),
                BorderThickness = new Thickness(1),
                Child = messageText
            };

            // Timestamp shown below the bubble
            TextBlock timestamp = new TextBlock
            {
                Text = DateTime.Now.ToString("HH:mm"),
                Foreground = new SolidColorBrush(Color.FromRgb(100, 110, 120)),
                FontSize = 15,
                Margin = new Thickness(4, 2, 4, 0),
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left
            };

            // Stack: label → bubble → timestamp
            StackPanel innerStack = new StackPanel
            {
                MaxWidth = 580,
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left
            };
            innerStack.Children.Add(nameLabel);
            innerStack.Children.Add(bubble);
            innerStack.Children.Add(timestamp);

            // Outer container with spacing
            Border container = new Border
            {
                Margin = new Thickness(8, 4, 8, 4),
                Child = innerStack
            };

            return container;
        }

        // ─────────────────────────────────────────────
        //  Utility
        // ─────────────────────────────────────────────

        /// <summary>
        /// Scrolls the chat scroll viewer to the bottom so the latest message is visible.
        /// </summary>
        public void ScrollToBottom()
        {
            ChatScrollViewer.UpdateLayout();
            ChatScrollViewer.ScrollToBottom();
        }

        /// <summary>
        /// Clears all messages from the chat panel and displays a fresh prompt.
        /// The user's name and memory are preserved between clears.
        /// </summary>
        public void ClearChat_Click(object sender, RoutedEventArgs e)
        {
            ChatPanel.Children.Clear();
            StatusText.Text = "Chat cleared — ready for new questions!";

            if (!string.IsNullOrEmpty(_userName))
                AddBotMessage("Chat cleared! What would you like to know next, " + _userName + "?");
        }
    }
}

