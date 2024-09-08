using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using GenerativeAI.Models;
using StudyAI.QuizLogic;


namespace StudyAI
{
    /// <summary>
    /// Interaction logic for NumericalQuestionControl.xaml
    /// </summary>
    public partial class NumericalQuestionControl : UserControl
    {

        private GenerativeModel _model;
        private QuestionManager _questionManager;

        string _answer;

        bool _useAIAnswerCheck;

        public NumericalQuestionControl()
        {
            InitializeComponent();
            SetStatus("Unanswered");
            InitializeAIModel();
            _questionManager = new QuestionManager();
        }

        private void InitializeAIModel()
        {
            string apiKey = "AIzaSyBQOHQfRJh6JZhlP_PEMa26ilNYWUQcSQc";
            _model = new GenerativeModel(apiKey);
        }


        public void SetQuestion(string question, string answer, bool useAIAnswerCheck)
        {
            _answer = answer;
            QuestionText.Text = question;
            _useAIAnswerCheck = useAIAnswerCheck;
        }


        public void SetStatus(string status)
        {
            // Update border color based on status
            switch (status)
            {
                case "Correct":
                    BorderB.BorderBrush = new SolidColorBrush(Colors.Green);
                    AnswerBox.IsEnabled = false; // Disable the TextBox
                    break;
                case "Incorrect":
                    BorderB.BorderBrush = new SolidColorBrush(Colors.Red);
                    AnswerBox.IsEnabled = true; // Enable the TextBox
                    break;
                case "Unanswered":
                    BorderB.BorderBrush = new SolidColorBrush(Colors.White); // Changed to Gray for unanswered
                    AnswerBox.IsEnabled = true; // Enable the TextBox
                    break;
                default:
                    BorderB.BorderBrush = new SolidColorBrush(Colors.White); // Default border color
                    AnswerBox.IsEnabled = true; // Ensure TextBox is enabled by default
                    break;
            }
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

        private async void Get_Answer(object sender, RoutedEventArgs e)
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

        private async void Check_Answer(object sender, RoutedEventArgs e)
        {
            _useAIAnswerCheck = true;
            if (_useAIAnswerCheck)
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
            else
            {
                if (AnswerBox.Text == _answer)
                {
                    SetStatus("Correct"); // Call the SetStatus method when the button is clicked
                }
                else
                {
                    SetStatus("Incorrect"); // Call the SetStatus method when the button is clicked
                }
            }
        }

        private void Try_Again(object sender, RoutedEventArgs e)
        {
            SetStatus("Unanswered");
        }



    }
}
