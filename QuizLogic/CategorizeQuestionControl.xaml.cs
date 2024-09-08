using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GenerativeAI.Models;
using GenerativeAI.Types;
using StudyAI.QuizLogic;

namespace StudyAI
{
    public partial class CategorizeQuestionControl : UserControl
    {
        private readonly SolidColorBrush grayBrush;
        private Border draggedItemBorder;
        private bool isDraggingFromCategory;

        List<string> _items;
        List<string> _categories;

        private GenerativeModel _model;
        private QuestionManager _questionManager;

        public CategorizeQuestionControl()
        {
            InitializeComponent();
            grayBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#EDEDED"));
            _questionManager = new QuestionManager();
        }

        public void SetQuestion(List<string> items, List<string> categories)
        {
            _items = items;
            _categories = categories;

            ClearPanels();

            foreach (var item in items)
            {
                AddItemToItemsPanel(item);
            }

            foreach (var category in categories)
            {
                AddCategoryToCategoriesPanel(category);
            }
        }

        private void ClearPanels()
        {
            ItemsPanel.Children.Clear();
            CategoriesPanel.Children.Clear();
        }

        private void AddItemToItemsPanel(string itemText)
        {
            var itemBorder = CreateBorder(grayBrush, 100, 5);
            var itemLabel = new Label
            {
                Content = itemText,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            itemBorder.Child = itemLabel;
            itemBorder.Tag = itemText;
            itemBorder.MouseMove += ItemBorder_MouseMove;
            itemBorder.MouseLeftButtonUp += ItemBorder_MouseLeftButtonUp;
            ItemsPanel.Children.Add(itemBorder);
        }

        private void AddCategoryToCategoriesPanel(string categoryText)
        {
            var categoryBorder = CreateBorder(grayBrush, 150, 5, 100);
            var categoryStackPanel = new StackPanel { Orientation = Orientation.Vertical };
            var categoryLabel = new Label
            {
                Content = categoryText,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var categoryContainer = new Border
            {
                BorderThickness = new Thickness(1),
                BorderBrush = null,
                Background = grayBrush,
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(5),
                Margin = new Thickness(5),
                MinWidth = 150,
                MinHeight = 100
            };

            categoryContainer.Child = categoryStackPanel;
            categoryContainer.Tag = categoryStackPanel;

            // Enable drop on the category border
            categoryContainer.AllowDrop = true;
            categoryContainer.DragOver += CategoryContainer_DragOver;
            categoryContainer.Drop += CategoryContainer_Drop;

            var categoryPanel = new StackPanel();
            categoryPanel.Children.Add(categoryLabel);
            categoryPanel.Children.Add(categoryContainer);

            CategoriesPanel.Children.Add(categoryPanel);
        }


        private Border CreateBorder(SolidColorBrush background, double minWidth, double margin, double minHeight = 0)
        {
            return new Border
            {
                BorderThickness = new Thickness(1),
                BorderBrush = null,
                Background = background,
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(5),
                Margin = new Thickness(margin),
                MinWidth = minWidth,
                MinHeight = minHeight
            };
        }

        private void ItemBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var border = sender as Border;
                if (border != null)
                {
                    draggedItemBorder = border;
                    isDraggingFromCategory = border.Parent is StackPanel; // Check if dragging from a category
                    DragDrop.DoDragDrop(border, border.Tag.ToString(), DragDropEffects.Move);
                }
            }
        }

        private void ItemBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border != null)
            {
                border.BorderBrush = null;
            }
        }

        private void CategoryContainer_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        private void CategoryContainer_Drop(object sender, DragEventArgs e)
        {
            var itemText = e.Data.GetData(typeof(string)) as string;
            if (itemText != null)
            {
                var categoryContainer = sender as Border;
                var categoryStackPanel = categoryContainer?.Tag as StackPanel;

                if (categoryStackPanel != null)
                {
                    if (isDraggingFromCategory)
                    {
                        // Remove the item from its previous category
                        var previousCategoryPanel = draggedItemBorder.Parent as StackPanel;
                        if (previousCategoryPanel != null)
                        {
                            previousCategoryPanel.Children.Remove(draggedItemBorder);
                        }
                    }

                    // Create a new border for the item
                    var itemBorder = CreateBorder(grayBrush, 100, 5);
                    var itemLabel = new Label
                    {
                        Content = itemText,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    itemBorder.Child = itemLabel;
                    itemBorder.Tag = itemText;

                    // Attach MouseMove event to the new item border
                    itemBorder.MouseMove += ItemBorder_MouseMove;

                    // Add the item to the new category
                    categoryStackPanel.Children.Add(itemBorder);

                    if (draggedItemBorder != null)
                    {
                        ItemsPanel.Children.Remove(draggedItemBorder);
                        draggedItemBorder = null;
                    }
                }
            }
            e.Handled = true;
        }

        private void ItemsPanel_Drop(object sender, DragEventArgs e)
        {
            var itemText = e.Data.GetData(typeof(string)) as string;
            if (itemText != null && isDraggingFromCategory)
            {
                AddItemToItemsPanel(itemText);

                // Remove the item from its category
                if (draggedItemBorder != null)
                {
                    var categoryStackPanel = draggedItemBorder.Parent as StackPanel;
                    if (categoryStackPanel != null)
                    {
                        categoryStackPanel.Children.Remove(draggedItemBorder);
                    }
                }
            }
            e.Handled = true;
        }


        public void SetStatus(string status)
        {
            // Update border color based on status
            switch (status)
            {
                case "Correct":
                    BorderB.BorderBrush = new SolidColorBrush(Colors.Green);
                    tryAgain_Btn.IsEnabled = true;
                    tryAgain_Btn.Visibility = Visibility.Visible;
                    break;
                case "Incorrect":
                    BorderB.BorderBrush = new SolidColorBrush(Colors.Red);
                    tryAgain_Btn.IsEnabled = true;
                    tryAgain_Btn.Visibility = Visibility.Visible;
                    break;
                case "Unanswered":
                    BorderB.BorderBrush = new SolidColorBrush(Colors.White); // Changed to Gray for unanswered
                    tryAgain_Btn.IsEnabled = false;
                    tryAgain_Btn.Visibility = Visibility.Hidden;
                    break;
                case "Hold":
                    BorderB.BorderBrush = new SolidColorBrush(Colors.White); // Changed to Gray for unanswered
                    tryAgain_Btn.IsEnabled = false;
                    tryAgain_Btn.Visibility = Visibility.Hidden;
                    break;
                default:
                    BorderB.BorderBrush = new SolidColorBrush(Colors.White); // Default border color
                    tryAgain_Btn.IsEnabled = false;
                    tryAgain_Btn.Visibility = Visibility.Hidden;
                    break;
            }
        }


        private void Try_Again(object sender, RoutedEventArgs e)
        {
            ClearPanels();
        }

        private async void Check_Answer(object sender, RoutedEventArgs e)
        {
            var categorizationString = BuildCategorizationString();
            // Assuming _questionManager.CheckAnswerAsync accepts the categorization string and returns a Task<bool>
            string _prompt = "Would you define this categorization correct:" + categorizationString + "Say no if the categories is empty nor there are any items not sorted" + "Answer only with one word: Yes or No"; ;

            _questionManager.LogToFile(categorizationString);

            SetStatus("Hold");
            bool correct = await _questionManager.CheckAnswerAsyncCustom(_prompt);

            if (correct)
            {
                SetStatus("Correct");
            }
            else
            {
                SetStatus("Incorrect");
            }
        }

        private string BuildCategorizationString()
        {
            var result = new StringBuilder();
            var uncategorizedItems = ItemsPanel.Children
                .OfType<Border>()
                .Select(border => border.Tag?.ToString())
                .Where(item => !string.IsNullOrEmpty(item));

            // Check if there are any uncategorized items
            if (uncategorizedItems.Any())
            {
                result.Append("Uncategorized Items: ");
                result.Append(string.Join(", ", uncategorizedItems));
                result.Append(". ");
            }

            // Iterate through categories
            foreach (StackPanel categoryPanel in CategoriesPanel.Children)
            {
                if (categoryPanel.Children.Count < 2)
                {
                    continue; // Skip if category structure is not as expected
                }

                var categoryLabel = categoryPanel.Children[0] as Label;
                var categoryContainer = categoryPanel.Children[1] as Border;
                var categoryStackPanel = categoryContainer?.Tag as StackPanel;

                // If any of the components are missing, skip this category
                if (categoryLabel == null || categoryContainer == null || categoryStackPanel == null)
                {
                    continue;
                }

                var categoryName = categoryLabel.Content?.ToString() ?? "Unknown Category";

                // Get items in the category
                var itemsInCategory = categoryStackPanel.Children
                    .OfType<Border>()
                    .Select(border => border.Tag?.ToString())
                    .Where(item => !string.IsNullOrEmpty(item))
                    .ToList();

                // Check if the category is empty
                if (!itemsInCategory.Any())
                {
                    result.Append($"{categoryName} is empty. ");
                }
                else
                {
                    foreach (var item in itemsInCategory)
                    {
                        result.Append($"{item} in Category {categoryName}, ");
                    }
                }
            }

            // Remove the last comma and space if the string is not empty
            if (result.Length > 2 && result.ToString().EndsWith(", "))
            {
                result.Length -= 2;
                result.Append(".");
            }

            // Show the constructed string for debugging purposes
            //MessageBox.Show($"Constructed String: {result}");

            return result.ToString();
        }

    }
}