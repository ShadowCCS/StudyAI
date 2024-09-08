using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StudyAI.QuizLogic
{
    /// <summary>
    /// Interaction logic for TrueOrFalseControl.xaml
    /// </summary>
    public partial class TrueOrFalseControl : UserControl
    {

        bool _answer;

        public TrueOrFalseControl()
        {
            InitializeComponent();
            SetStatus("Unanswered");
        }


        public void SetStatus(string status)
        {
            // Update border color based on status
            switch (status)
            {
                case "Correct":
                    BorderB.BorderBrush = new SolidColorBrush(Colors.Green);
                    blueBtn.IsEnabled = false; 
                    redBtn.IsEnabled = false; 
                    break;
                case "Incorrect":
                    BorderB.BorderBrush = new SolidColorBrush(Colors.Red);
                    blueBtn.IsEnabled = true;
                    redBtn.IsEnabled = true;
                    break;
                case "Unanswered":
                    BorderB.BorderBrush = new SolidColorBrush(Colors.White); 
                    blueBtn.IsEnabled = true;
                    redBtn.IsEnabled = true;
                    break;
                default:
                    BorderB.BorderBrush = new SolidColorBrush(Colors.White);
                    blueBtn.IsEnabled = true;
                    redBtn.IsEnabled = true;
                    break;
            }

        }
        public void SetQuestion(string question, bool answer)
        {
            QuestionText.Text = question;
            _answer = answer;
        }

        private void CheckAnswer(bool answer)
        {
            if(answer == _answer)
            {
                SetStatus("Correct");
            }
            else
            {
                SetStatus("Incorrect");
            }
        }

        private void BlueButton_Click(object sender, RoutedEventArgs e)
        {
            CheckAnswer(true);
        }

        private void RedButton_Click(object sender, RoutedEventArgs e)
        {
            CheckAnswer(false);
        }


    }
}
