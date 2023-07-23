using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Firebase.Database;

namespace Friend_Testing
{
    public partial class MainWindow : Window
    {
        public static FirebaseClient FirebaseClient { get; set; }
        private ObservableCollection<ChatMessage> messages = new ObservableCollection<ChatMessage>();

        private string Username;

        public class ChatMessage
        {
            public string SenderId { get; set; }
            public string Content { get; set; }
            public long Timestamp { get; set; }
            public string FixedTimestamp { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();

            Username = PromptUsername();
            if (string.IsNullOrWhiteSpace(Username))
            {
                Close();
                return;
            }

            FirebaseClient = new FirebaseClient("Place your link here"); // Your Realtime Database (make one on Google Firebase) from my tutorial

            ChatListView.ItemsSource = messages;

            LoadMessages();
            ScrollChatList();
        }

        private async Task LoadMessages()
        {
            try
            {
                var existingMessages = await FirebaseClient.Child("Messages").OnceAsync<ChatMessage>();
                messages.Clear();

                if (existingMessages != null && existingMessages.Any())
                {
                    foreach (var message in existingMessages)
                    {
                        if (message?.Object != null)
                        {
                            message.Object.FixedTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(message.Object.Timestamp).ToLocalTime().ToString("H:mm tt");
                            messages.Add(message.Object);
                        }
                    }
                }

                FirebaseClient.Child("Messages").AsObservable<ChatMessage>().Subscribe(message =>
                {
                    if (message?.Object != null)
                    {
                        message.Object.FixedTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(message.Object.Timestamp).ToLocalTime().ToString("H:mm tt");

                        if (!messages.Any(m => m.Timestamp == message.Object.Timestamp && m.Content == message.Object.Content && m.SenderId == message.Object.SenderId))
                        {
                            Application.Current.Dispatcher.Invoke(() => messages.Add(message.Object));
                            ScrollChatList();
                        }
                    }
                });

                ScrollChatList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load messages. Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string PromptUsername()
        {
            var dialog = new InputDialog("Enter your username:");
            if (dialog.ShowDialog() == true)
            {
                string username = dialog.UserInput;
                if (string.IsNullOrWhiteSpace(username))
                {
                    var result = MessageBox.Show("You must enter a valid username. Do you want to try again?", "Invalid Username", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        return PromptUsername();
                    }
                    else
                    {
                        return null;
                    }
                }
                return username;
            }
            else
            {
                return null;
            }
        }

        private async void OnSendMessage(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MessageTextBox.Text))
                return;

            var message = new ChatMessage
            {
                SenderId = Username,
                Content = MessageTextBox.Text,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            await FirebaseClient.Child("Messages").PostAsync(message);
            MessageTextBox.Clear();

            ScrollChatList();
        }

        private void ScrollChatList()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (ChatListView.Items.Count > 0)
                {
                    var lastItem = ChatListView.Items[ChatListView.Items.Count - 1];
                    ChatListView.ScrollIntoView(lastItem);
                }
            });
        }

        public class InputDialog : Window
        {
            private TextBox inputTextBox;
            private Button okButton;

            public InputDialog(string prompt)
            {
                Title = "Username Prompt";
                Width = 300;
                Height = 150;
                WindowStartupLocation = WindowStartupLocation.CenterScreen;

                var promptLabel = new Label
                {
                    Content = prompt,
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                inputTextBox = new TextBox
                {
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                okButton = new Button
                {
                    Content = "OK",
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                okButton.Click += OkButton_Click;

                var stackPanel = new StackPanel();
                stackPanel.Children.Add(promptLabel);
                stackPanel.Children.Add(inputTextBox);
                stackPanel.Children.Add(okButton);

                Content = stackPanel;
            }

            private void OkButton_Click(object sender, RoutedEventArgs e)
            {
                DialogResult = true;
            }

            public string UserInput => inputTextBox.Text;
        }
    }
}
