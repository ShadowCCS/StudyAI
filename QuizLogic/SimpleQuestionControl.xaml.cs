using GenerativeAI.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StudyAI.QuizLogic;
using System.Windows.Documents;

namespace StudyAI
{
    public partial class SimpleQuestionControl : UserControl
    {

        private GenerativeModel _model;
        private QuestionManager _questionManager;

        public SimpleQuestionControl()
        {
            InitializeComponent();
            SetStatus("Unanswered");
            _questionManager = new QuestionManager();
            InitializeAIModel();
        }

        public void SetQuestion(string question)
        {
            QuestionText.Text = question;
        }

        public void SetStatus(string status)
        {
            // Update border color based on status
            switch (status)
            {
                case "Correct":
                    BorderB.BorderBrush = new SolidColorBrush(Colors.Green);
                    AnswerBox.IsEnabled = false; // Disable the TextBox
                    tryAgain_Btn.IsEnabled = true;
                    tryAgain_Btn.Visibility = Visibility.Visible;
                    break;
                case "Incorrect":
                    BorderB.BorderBrush = new SolidColorBrush(Colors.Red);
                    AnswerBox.IsEnabled = true; // Enable the TextBox
                    tryAgain_Btn.IsEnabled = true;
                    tryAgain_Btn.Visibility = Visibility.Visible;
                    break;
                case "Unanswered":
                    BorderB.BorderBrush = new SolidColorBrush(Colors.White); // Changed to Gray for unanswered
                    AnswerBox.IsEnabled = true; // Enable the TextBox
                    tryAgain_Btn.IsEnabled = false;
                    tryAgain_Btn.Visibility = Visibility.Hidden;
                    break;
                case "Hold":
                    BorderB.BorderBrush = new SolidColorBrush(Colors.White); // Changed to Gray for unanswered
                    AnswerBox.IsEnabled = false; // Enable the TextBox
                    tryAgain_Btn.IsEnabled = false;
                    tryAgain_Btn.Visibility = Visibility.Hidden;
                    break;
                default:
                    BorderB.BorderBrush = new SolidColorBrush(Colors.White); // Default border color
                    AnswerBox.IsEnabled = true; // Ensure TextBox is enabled by default
                    tryAgain_Btn.IsEnabled = false;
                    tryAgain_Btn.Visibility = Visibility.Hidden;
                    break;
            }
        }

        private void InitializeAIModel()
        {
            string apiKey = "AIzaSyBQOHQfRJh6JZhlP_PEMa26ilNYWUQcSQc";
            _model = new GenerativeModel(apiKey);
        }

        bool HasWipedText = false;
        private void WipeTextFocused(object sender, RoutedEventArgs e)
        {
            if (!HasWipedText)
            {
                AnswerBox.Text = "";
                HasWipedText = true;
            }
        }

        private void AddtextUnFocused(object sender, RoutedEventArgs e)
        {
            if (AnswerBox.Text == string.Empty)
            {
                AnswerBox.Text = "Write your answer here...";
                HasWipedText = false;
            }
        }

        private async void Check_Answer(object sender, RoutedEventArgs e)
        {
            // Retrieve the suggested answer
            string answerSuggestion = await _questionManager.GetAnswer(QuestionText.Text);

            // Create a new FlowDocument for the RichTextBox
            var flowDoc = new FlowDocument();

            // Create a new Paragraph and add formatted text
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run(answerSuggestion) { Foreground = Brushes.DarkGray });
            flowDoc.Blocks.Add(paragraph);

            // Set the FlowDocument as the content of the RichTextBox
            FormattedTextBox.Document = flowDoc;
        }



        private void Try_Again(object sender, RoutedEventArgs e)
        {
            SetStatus("Unanswered");
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            SetStatus("Hold");
            bool correct = await _questionManager.CheckAnswerAsync(QuestionText.Text, AnswerBox.Text);

            if (correct)
            {
                SetStatus("Correct");
            }
            else
            {
                SetStatus("Incorrect");
            }
            
        }


    }
}