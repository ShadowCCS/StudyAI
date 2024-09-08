using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using GenerativeAI.Models;
using StudyAI.QuizLogic;
using System.IO;

namespace StudyAI
{
    /// <summary>
    /// Interaction logic for QuizTemplate.xaml
    /// </summary>
    public partial class QuizTemplate : Window
    {
        private QuestionManager _questionManager;
        private GenerativeModel _model;

        string _language = "Norsk Bokmål";
        string _length = "Medium";
        string _difficulty = "Medium";
        string _referencematerial = "";
        List<string> _commands = new List<string>
{
    "_questionManager.AddSimpleQuestion(\"Question\"); // Adds a simple question",
    "_questionManager.AddMultipleChoiceQuestion(\"Question\", new List<string> { \"Option1\", \"Option2\", \"Option3\", \"Option4\" },new List<string> { \"0\" }); // Requires a list of options + a list of answers ids. It can have both 1 answer or more.",
    "_questionManager.AddTrueOrFalseQuestion(\"Statement\", true); // Requires a statement and if it is true or false",
    "_questionManager.AddNumericalQuestion(\"Question\", 8, false); // Requires a question, a answer and a true or false if it should use AI to check the answer. It should be false with simple answers like 7 or 10, but true if else.",
    "\"_questionManager.AddCategorizationQuestion(new List<string> {\\\"Item 1\\\", \\\"Item 2\\\"},new List<string> { \\\"Category 1\\\", \\\"Category 2\\\"}); //Requires minimum 2 categories and 2 items. It can handle as many as desired, but more than 4 categories is not advisable. DO NOT CREATE DUPLICATE CATEGORIES OR ITEMS WITH THE SAME NAME\""
};



        public QuizTemplate()
        {
            InitializeComponent();
            _questionManager = new QuestionManager(QuizContainer);
            InitializeAIModel();
        }


        private void LogToFile(string message)
        {
            // Ensure the log file directory exists
            string logFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");

            try
            {
                // Append the message to the log file
                System.IO.File.AppendAllText(logFilePath, $"{DateTime.Now}: {message}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing to log file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeAIModel()
        {
            string _apiKey = "AIzaSyBQOHQfRJh6JZhlP_PEMa26ilNYWUQcSQc";
            _model = new GenerativeModel(_apiKey);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public async Task SetParameterStrings(string language, string length, string difficulty, string referencematerial)
        {
            _language = language;
            _length = length;

            if(_length == "Short")
            {
                _length = "Short " + "This means about 3-5 questions";
            }
            else if (_length == "Medium")
            {
                _length = "Medium " + "This means about 6-10 questions";
            }
            else if (_length == "Long ")
            {
                _length = "Long " + "This means about 11-15 questions";
            }
                

            _difficulty = difficulty;
            _referencematerial = referencematerial;


            await GenerateAndExecuteQuizQuestionsAsync(_language, _length, _difficulty, _referencematerial);
        }
        private async Task GenerateAndExecuteQuizQuestionsAsync(string language, string length, string difficulty, string referencematerial)
        {
            const int minQuestionsRequired = 3; // Minimum number of questions required
            bool successfulGeneration = false;

            while (!successfulGeneration)
            {
                try
                {
                    string prompt = $"From now on you are a part of a quiz generation system. You can create quiz questions using these commands: {string.Join(", ", _commands)} " +
                                    $"The quiz you will be generating questions for should be based on these notes/topic/prompt: {referencematerial} " +
                                    $"the questions should be in the language: {language} and should have a length of: {length} and difficulty of: {difficulty}. " +
                                    $"You should only write down the questions and nothing more, this includes titles etc. The quiz should be as good as you can and match the selected difficulty. All the answers need to be factually correct. The questions should be stocked and DO NOT CREATE MULTIPLE OF THE SAME QUESTION IN A ROW. DO NOT CREATE ONLY ONE TYPE OF QUESTION TYPE"+
                                    $"DO NOT CREATE DUPLICATE QUESTIONS OR ANSWERS, DO NOT CREATE QUESTIONS THAT ARE THE SAME AS THE ANSWER";

                    LogToFile($"Prompt sent: {prompt}");

                    var response = await _model.GenerateContentAsync(prompt);

                    LogToFile($"AI response: {response}");

                    standbyText.Visibility = Visibility.Collapsed;

                    FinishedBtn.Visibility = Visibility.Visible;
                    FinishedBtn.IsEnabled = true;

                    // Clear previous questions and reset question number
                    _questionManager.ClearQuestions(); // Ensure this method clears all previous UI elements
                    _questionManager.ResetQuestionNumber();

                    // Execute the generated function calls
                    ExecuteGeneratedCode(response);

                    // Check the number of questions added
                    if (_questionManager.GetQuestionNumber() >= minQuestionsRequired)
                    {
                        successfulGeneration = true;
                    }
                    else
                    {
                        MessageBox.Show($"Problem Encountered, Trying Again", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
                        _questionManager.ClearQuestions(); // Clear previous questions
                    }
                }
                catch (Exception ex)
                {
                    LogToFile($"Error fetching AI response: {ex.Message}");
                    MessageBox.Show($"Error fetching AI response: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break; // Exit loop if an error occurs
                }
            }
        }



        private void ExecuteGeneratedCode(string code)
        {
            if (_questionManager == null)
            {
                LogError("QuestionManager is not initialized.");
                return;
            }

            var lines = code.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var commandLines = new List<string>();
            var currentCommand = string.Empty;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine))
                    continue;

                var commandWithoutComment = trimmedLine.Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();

                if (commandWithoutComment.EndsWith(");"))
                {
                    currentCommand += " " + commandWithoutComment;
                    commandLines.Add(currentCommand);
                    currentCommand = string.Empty;
                }
                else
                {
                    currentCommand += " " + commandWithoutComment;
                }
            }

            foreach (var command in commandLines)
            {
                try
                {
                    if (command.Contains("_questionManager.AddSimpleQuestion"))
                    {
                        var question = ExtractParameter(command);
                        _questionManager.AddSimpleQuestion(question);
                    }
                    else if (command.Contains("_questionManager.AddMultipleChoiceQuestion"))
                    {
                        var (question, options, correctIndices) = ExtractMultipleChoiceParameters(command);
                        _questionManager.AddMultipleChoiceQuestion(question, options, correctIndices);
                    }
                    else if (command.Contains("_questionManager.AddTrueOrFalseQuestion"))
                    {
                        var (statement, isTrue) = ExtractTrueOrFalseParameters(command);
                        _questionManager.AddTrueOrFalseQuestion(statement, isTrue);
                    }
                    else if (command.Contains("_questionManager.AddNumericalQuestion"))
                    {
                        var (question, answer, useAIAnswerCheck) = ExtractNumericalParameters(command);
                        _questionManager.AddNumericalQuestion(question, answer, useAIAnswerCheck);
                    }
                    else if (command.Contains("_questionManager.AddCategorizationQuestion"))
                    {
                        var (items, categories) = ExtractCategorizationParameters(command);
                        _questionManager.AddCategorizationQuestion(items, categories);
                    }
                    else
                    {
                        LogError($"Unknown command: {command}");
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Error executing command: {command}. Exception: {ex.Message}");
                }
            }
        }


        private (List<string> items, List<string> categories) ExtractCategorizationParameters(string line)
        {
            try
            {
                int start = line.IndexOf('(') + 1;
                int end = line.LastIndexOf(')');
                string paramsStr = line.Substring(start, end - start);

                // Split parameters using a regex that handles commas outside of quotes
                string[] parts = SplitParameters(paramsStr);

                if (parts.Length != 2)
                    throw new FormatException("Expected 2 parameters in the categorization question command.");

                var items = ParseItems(parts[0].Trim());
                var categories = ParseCategories(parts[1].Trim());

                return (items, categories);
            }
            catch (Exception ex)
            {
                LogError($"Error extracting categorization parameters: {ex.Message}");
                throw;
            }
        }


        private List<string> ParseItems(string itemsStr)
        {
            if (itemsStr.StartsWith("new List<string>"))
            {
                int start = itemsStr.IndexOf('{') + 1;
                int end = itemsStr.IndexOf('}');
                var itemsString = itemsStr.Substring(start, end - start).Trim();

                var items = itemsString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(item => item.Trim().Trim('"'))
                                        .ToList();

                // Debugging output
                Console.WriteLine($"Parsed Items: {string.Join(", ", items)}");

                return items;
            }
            else
            {
                throw new FormatException("Invalid format for items.");
            }
        }

        private List<string> ParseCategories(string categoriesStr)
        {
            if (categoriesStr.StartsWith("new List<string>"))
            {
                int start = categoriesStr.IndexOf('{') + 1;
                int end = categoriesStr.IndexOf('}');
                var categoriesString = categoriesStr.Substring(start, end - start).Trim();

                var categories = categoriesString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                  .Select(category => category.Trim().Trim('"'))
                                                  .ToList();

                // Debugging output

                return categories;
            }
            else
            {
                throw new FormatException("Invalid format for categories.");
            }
        }

        private void LogError(string message)
        {
            try
            {
                // Assuming the log file is in the application resources folder
                string logPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "log.txt");
                using (var writer = new System.IO.StreamWriter(logPath, true))
                {
                    writer.WriteLine($"{DateTime.Now}: {message}");
                }
            }
            catch
            {
                // Handle any logging errors if necessary
            }
        }



        private void ProcessCommandBlock(string commandBlock)
        {
            // Check which function the command block corresponds to and execute accordingly
            if (commandBlock.Contains("_questionManager.AddSimpleQuestion"))
            {
                var question = ExtractParameter(commandBlock);
                _questionManager.AddSimpleQuestion(question);
            }
            else if (commandBlock.Contains("_questionManager.AddMultipleChoiceQuestion"))
            {
                var (question, options, correctIndex) = ExtractMultipleChoiceParameters(commandBlock);
                _questionManager.AddMultipleChoiceQuestion(question, options, correctIndex);
            }
            else if (commandBlock.Contains("_questionManager.AddTrueOrFalseQuestion"))
            {
                var (statement, isTrue) = ExtractTrueOrFalseParameters(commandBlock);
                _questionManager.AddTrueOrFalseQuestion(statement, isTrue);
            }
            else if (commandBlock.Contains("_questionManager.AddNumericalQuestion"))
            {
                var (question, answer, useAIAnswerCheck) = ExtractNumericalParameters(commandBlock);
                _questionManager.AddNumericalQuestion(question, answer, useAIAnswerCheck);
            }
            else if (commandBlock.Contains("_questionManager.AddCategorizationQuestion"))
            {
                var (item, category) = ExtractCategorizationParameters(commandBlock);
                _questionManager.AddCategorizationQuestion(item, category);
            }

        }

        private string ExtractParameter(string line)
        {
            try
            {
                int start = line.IndexOf('(') + 1;
                int end = line.LastIndexOf(')');
                if (start < 0 || end < 0 || end <= start)
                {
                    throw new InvalidOperationException("Invalid parameter format.");
                }
                return line.Substring(start, end - start).Trim('"');
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error extracting parameter: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }
        }

        private (string, List<string>, List<string>) ExtractMultipleChoiceParameters(string line)
        {
            try
            {
                int start = line.IndexOf('(') + 1;
                int end = line.LastIndexOf(')');
                string paramsStr = line.Substring(start, end - start);

                string[] parts = SplitParameters(paramsStr);

                if (parts.Length != 3)
                    throw new FormatException("Expected 3 parameters in the multiple choice question command.");

                // Extract question text
                string question = parts[0].Trim().Trim('"');

                // Extract options
                var options = ParseOptions(parts[1].Trim());

                // Extract the correct answer indices
                var correctIndicesString = parts[2].Trim();
                var correctIndices = ParseCorrectIndices(correctIndicesString);

                return (question, options, correctIndices);
            }
            catch (Exception ex)
            {
                LogError($"Error extracting multiple choice parameters: {ex.Message}");
                throw;
            }
        }

        private List<string> ParseCorrectIndices(string correctIndicesString)
        {
            if (correctIndicesString.StartsWith("new List<string>"))
            {
                int start = correctIndicesString.IndexOf('{') + 1;
                int end = correctIndicesString.IndexOf('}');
                var indicesString = correctIndicesString.Substring(start, end - start).Trim();

                var indices = indicesString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(index => index.Trim().Trim('"'))
                                            .ToList();


                return indices;
            }
            else
            {
                throw new FormatException("Invalid format for correct indices.");
            }
        }




        private string[] SplitParameters(string parameters)
        {
            // Split parameters using a regex that handles commas outside of quotes
            var regex = new Regex(@",(?=(?:[^{}]*{[^{}]*})*[^{}]*$)");
            return regex.Split(parameters);
        }

        private List<string> ParseOptions(string optionsStr)
        {
            var options = new List<string>();

            // Extract options between curly braces
            int optionsStart = optionsStr.IndexOf('{') + 1;
            int optionsEnd = optionsStr.LastIndexOf('}');
            if (optionsStart < 0 || optionsEnd < 0 || optionsStart > optionsEnd)
                throw new FormatException("Invalid options format.");

            string[] optionsArr = optionsStr.Substring(optionsStart, optionsEnd - optionsStart).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var option in optionsArr)
            {
                options.Add(option.Trim().Trim('"'));
            }

            return options;
        }

        private (string, bool) ExtractTrueOrFalseParameters(string line)
        {
            int start = line.IndexOf('(') + 1;
            int end = line.LastIndexOf(')');
            string paramsStr = line.Substring(start, end - start);
            string[] parts = paramsStr.Split(',');

            string statement = parts[0].Trim().Trim('"');
            bool isTrue = bool.Parse(parts[1].Trim());

            return (statement, isTrue);
        }


        private (string, string, bool) ExtractNumericalParameters(string line)
        {
            int start = line.IndexOf('(') + 1;
            int end = line.LastIndexOf(')');
            string paramsStr = line.Substring(start, end - start);
            string[] parts = paramsStr.Split(',');

            string question = parts[0].Trim().Trim('"');
            string answer = parts[1].Trim().Trim('"');
            bool useAIAnswerCheck = bool.Parse(parts[2].Trim());

            return (question, answer, useAIAnswerCheck);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LeftMouseDownDrag(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

    }
}

