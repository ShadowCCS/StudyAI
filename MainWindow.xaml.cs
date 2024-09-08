using System.Text;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using StudyAI.QuizLogic;
using System.Threading;
using System.Diagnostics.Metrics;

namespace StudyAI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        bool mc = true;
        bool tf = true;
        bool sh = true;
        bool fi = true;

        int selectedTypes = 4;
        int minSelectedTypes = 1;

        SolidColorBrush greenBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#74B16A"));
        SolidColorBrush redBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DB7C5E"));

        public MainWindow()
        {
            InitializeComponent();
            PasteNotesTxtBox.Text = "Paste your notes here...";
        }

        bool HasWipedText = false;
        private void WipeTextFocused(object sender, RoutedEventArgs e)
        {
            if (!HasWipedText)
            {
                PasteNotesTxtBox.Text = "";
                HasWipedText = true;
            }
        }

        private void AddtextUnFocused(object sender, RoutedEventArgs e)
        {
            if (PasteNotesTxtBox.Text == string.Empty)
            {
                PasteNotesTxtBox.Text = "Write your answer here...";
                HasWipedText = false;
            }
        }

        private void SetToggleCheckbox(Border checkbox, bool currentState)
        {
            checkbox.Background = currentState ? greenBrush : redBrush;
        }

        private void ToggleCheckbox(ref bool state, Border checkbox)
        {
            // Allow toggling only if more than one checkbox is selected
            if (selectedTypes > minSelectedTypes || !state)
            {
                state = !state;
                selectedTypes += state ? 1 : -1;
                SetToggleCheckbox(checkbox, state);
            }
        }

        private void MC_Checkbox_MouseDown(object sender, MouseEventArgs e)
        {
            ToggleCheckbox(ref mc, MC_Checkbox);
        }

        private void TF_Checkbox_MouseDown(object sender, MouseEventArgs e)
        {
            ToggleCheckbox(ref tf, TF_Checkbox);
        }

        private void SH_Checkbox_MouseDown(object sender, MouseEventArgs e)
        {
            ToggleCheckbox(ref sh, SH_Checkbox);
        }

        private void FI_Checkbox_MouseDown(object sender, MouseEventArgs e)
        {
            ToggleCheckbox(ref fi, FI_Checkbox);
        }

        private void LeftMouseDown(object sender, MouseEventArgs e)
        {
            DragMove();
        }

        private void MinimizeEvent(object sender, EventArgs e)
        {
           this.WindowState = WindowState.Minimized;
        }


        private void CloseEvent(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create an instance of the QuizTemplate window
                QuizTemplate quizWindow = new QuizTemplate();

                // Show the new window
                quizWindow.Show();

                // Retrieve values from UI controls
                string length = (LengthComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "DefaultLength";
                string difficulty = (DifficultyComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "DefaultDifficulty";
                string referencematerial = PasteNotesTxtBox.Text;

                // Set parameters asynchronously
                await quizWindow.SetParameterStrings("Norwegian", length, difficulty, referencematerial);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}