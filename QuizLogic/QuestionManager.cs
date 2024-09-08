using GenerativeAI.Models;
using GenerativeAI.Requests;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Google.Cloud.AIPlatform.V1;


namespace StudyAI.QuizLogic
{
    public class QuestionManager
    {
        private int questionNumber = 1;
        private readonly Panel _quizContainer;

        private GenerativeModel _model;

        string _referenceMaterial;

        public QuestionManager(Panel quizContainer)
        {
            _quizContainer = quizContainer;
            InitializeAIModel();
        }

        public QuestionManager()
        {
            InitializeAIModel();
        }

        public void ClearQuestions()
        {
            _quizContainer.Children.Clear();
        }

        public void ResetQuestionNumber()
        {
            questionNumber = 1;
        }

        public int GetQuestionNumber()
        {
            return questionNumber - 1; // Return number of questions added
        }


        public void LogToFile(string message)
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
            // Replace with your actual API key
            string apiKey = "AIzaSyBQOHQfRJh6JZhlP_PEMa26ilNYWUQcSQc";
            _model = new GenerativeModel(apiKey);
        }

        private Label CreateQuestionLabel()
        {
            var questionLabel = new Label
            {
                Content = $"Question {questionNumber}",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 10, 0, 5)
            };

            return questionLabel;
        }

        public async Task<string> GetAnswer(string question)
        {
            string prompt = "Write a simple explanation to the following question, try to avoid complicated words, if irrelevant: " + question + " in norwegian";

            var response = await _model.GenerateContentAsync(prompt);

            return response;
        }

        public async Task<bool> CheckAnswerAsync(string question, string answer)
        {
            // Construct the prompt
            string prompt = $"For this question: {question}, would you define this answer correct: '{answer}'? It has to be factually correct. Answer only with one word: Yes or No";

            try
            {
                // Generate content asynchronously
                var response = await _model.GenerateContentAsync(prompt);

                // Validate response
                response = response.Trim();
                if (response.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else if (response.Equals("No", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                else
                {
                    // Log or handle invalid response
                    // Retry with a limit to avoid infinite recursion
                    int maxRetries = 3;
                    for (int i = 0; i < maxRetries; i++)
                    {
                        // Wait for a short time before retrying
                        await Task.Delay(1000); // 1 second delay

                        response = await _model.GenerateContentAsync(prompt);
                        response = response.Trim();

                        if (response.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                        else if (response.Equals("No", StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                    }

                    // Log an error or handle the case where the response is still invalid after retries
                    throw new Exception("Invalid response from AI after multiple attempts.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it
                MessageBox.Show($"Error checking answer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> CheckAnswerAsyncCustom(string prompt)
        {
            // Construct the prompt
            string _prompt = prompt;

            try
            {
                // Generate content asynchronously
                var response = await _model.GenerateContentAsync(_prompt + "Context: " + _referenceMaterial);

                // Validate response
                response = response.Trim();
                if (response.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else if (response.Equals("No", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                else
                {
                    // Log or handle invalid response
                    // Retry with a limit to avoid infinite recursion
                    int maxRetries = 3;
                    for (int i = 0; i < maxRetries; i++)
                    {
                        // Wait for a short time before retrying
                        await Task.Delay(1000); // 1 second delay

                        response = await _model.GenerateContentAsync(prompt);
                        response = response.Trim();

                        if (response.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                        else if (response.Equals("No", StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                    }

                    // Log an error or handle the case where the response is still invalid after retries
                    throw new Exception("Invalid response from AI after multiple attempts.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it
                MessageBox.Show($"Error checking answer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public void AddSimpleQuestion(string question)
        {
            var questionLabel = CreateQuestionLabel();
            _quizContainer.Children.Add(questionLabel);

            var simpleQuestion = new SimpleQuestionControl();
            simpleQuestion.SetQuestion(question);
            _quizContainer.Children.Add(simpleQuestion);

            questionNumber++;
        }

        public void AddMultipleChoiceQuestion(string question, List<string> options, List<string> correctAnswerIds)
        {
            var questionLabel = CreateQuestionLabel();
            _quizContainer.Children.Add(questionLabel);

            var multipleChoiceQuestion = new MultipleChoiceQuestionControl();
            multipleChoiceQuestion.SetQuestion(question, options, correctAnswerIds);
            _quizContainer.Children.Add(multipleChoiceQuestion);

            questionNumber++;
        }

        public void AddTrueOrFalseQuestion(string question, bool answer)
        {
            var questionLabel = CreateQuestionLabel();
            _quizContainer.Children.Add(questionLabel);

            var trueOrFalseQuestion = new TrueOrFalseControl();
            trueOrFalseQuestion.SetQuestion(question, answer);
            _quizContainer.Children.Add(trueOrFalseQuestion);

            questionNumber++;
        }

        public void AddNumericalQuestion(string question, string answer, bool useAIAnswerCheck)
        {
            var questionLabel = CreateQuestionLabel();
            _quizContainer.Children.Add(questionLabel);

            var numericalQuestion = new NumericalQuestionControl();
            numericalQuestion.SetQuestion(question, answer, useAIAnswerCheck);
            _quizContainer.Children.Add(numericalQuestion);

            questionNumber++;
        }

        public void AddCategorizationQuestion(List<string> items, List<string> categories)
        {
            var questionLabel = CreateQuestionLabel();
            _quizContainer.Children.Add(questionLabel);

            var categorizationQuestion = new CategorizeQuestionControl();
            categorizationQuestion.SetQuestion(items, categories);
            _quizContainer.Children.Add(categorizationQuestion);

            questionNumber++;
        }
    }
}
