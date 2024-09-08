using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StudyAI
{
    public partial class MultipleChoiceQuestionControl : UserControl
    {
        private List<string> _correctAnswerIds;

        public MultipleChoiceQuestionControl()
        {
            InitializeComponent();
            SetStatus("Unanswered");
        }



        public void SetQuestion(string question, List<string> options, List<string> correctAnswerIds)
        {
            QuestionText.Text = question;
            OptionsContainer.Children.Clear();
            _correctAnswerIds = correctAnswerIds;

            bool useRadioButtons = correctAnswerIds.Count == 1; // Use RadioButton if only one correct answer

            for (int i = 0; i < options.Count; i++)
            {
                var option = options[i];
                if (useRadioButtons)
                {
                    var radioButton = new RadioButton
                    {
                        Content = option,
                        Margin = new Thickness(0, 5, 0, 5),
                        Tag = i.ToString(), // Set Tag property to use as the ID
                        GroupName = "OptionsGroup" // Ensure all radio buttons are in the same group
                    };
                    OptionsContainer.Children.Add(radioButton);
                }
                else
                {
                    var checkBox = new CheckBox
                    {
                        Content = option,
                        Margin = new Thickness(0, 5, 0, 5),
                        Tag = i.ToString() // Set Tag property to use as the ID
                    };
                    OptionsContainer.Children.Add(checkBox);
                }
            }
        }

        private List<string> GetSelectedAnswerIds()
        {
            var selectedAnswerIds = new List<string>();

            foreach (var child in OptionsContainer.Children)
            {
                if (child is RadioButton radioButton && radioButton.IsChecked == true)
                {
                    selectedAnswerIds.Add(radioButton.Tag.ToString());
                }
                else if (child is CheckBox checkBox && checkBox.IsChecked == true)
                {
                    selectedAnswerIds.Add(checkBox.Tag.ToString());
                }
            }

            return selectedAnswerIds;
        }

        public void SetStatus(string status)
        {
            switch (status)
            {
                case "Correct":
                    BorderB.BorderBrush = new SolidColorBrush(Colors.Green);
                    SetOptionsEnabled(false);
                    break;
                case "Incorrect":
                    BorderB.BorderBrush = new SolidColorBrush(Colors.Red);
                    SetOptionsEnabled(true);
                    break;
                case "Unanswered":
                    BorderB.BorderBrush = new SolidColorBrush(Colors.White);
                    SetOptionsEnabled(true);
                    break;
                default:
                    BorderB.BorderBrush = new SolidColorBrush(Colors.White);
                    SetOptionsEnabled(true);
                    break;
            }
        }

        private void SetOptionsEnabled(bool isEnabled)
        {
            foreach (var child in OptionsContainer.Children)
            {
                if (child is RadioButton || child is CheckBox)
                {
                    ((Control)child).IsEnabled = isEnabled;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var selectedAnswerIds = GetSelectedAnswerIds();


            if (_correctAnswerIds.Count == 1) // Single correct answer
            {
                if (selectedAnswerIds.Count == 1 && _correctAnswerIds.Contains(selectedAnswerIds.First()))
                {
                    SetStatus("Correct");
                }
                else
                {
                    SetStatus("Incorrect");
                }
            }
            else // Multiple correct answers
            {
                // Check that all correct answers are selected and no extra answers are selected
                if (_correctAnswerIds.All(id => selectedAnswerIds.Contains(id)) && selectedAnswerIds.Count == _correctAnswerIds.Count)
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
}
