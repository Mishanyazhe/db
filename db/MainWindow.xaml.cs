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
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Threading;

namespace db
{
    public partial class MainWindow : Window
    {
        private SqlConnection sqlConnection = null;
        private DataRowView selectedRow;
        DataTable dataSet;
        private TextBox[] textBoxes;

        public MainWindow()
        {
            InitializeComponent();

            textBoxes = new TextBox[]
            {
               lastNameTextBox,
               firstNameTextBox,
               patronymicTextBox,
               birthDayTextBox,
               passageDayTextBox,
               placeOfWorkTextBox,
               placeOfResidenceTextBox
            };
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["Database"].ConnectionString);

            sqlConnection.Open();

            ClearTextBoxes();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SqlCommand command = new SqlCommand(
                $"INSERT INTO [Fluorography] (LastName, FirstName, Patronymic, birthDay, PassageDay, PlaceOfWork, PlaceOfResidence) " +
                $"VALUES (@LastName, @FirstName, @Patronymic, @birthDay, @PassageDay, @PlaceOfWork, @PlaceOfResidence)",
                sqlConnection);

            DateTime date = CheckingDate(birthDayTextBox.Text);

            if (date == DateTime.MinValue) return;

            command.Parameters.AddWithValue("LastName", lastNameTextBox.Text);
            command.Parameters.AddWithValue("FirstName", firstNameTextBox.Text);
            command.Parameters.AddWithValue("Patronymic", patronymicTextBox.Text);
            command.Parameters.AddWithValue("BirthDay", $"{date.Month}/{date.Day}/{date.Year}");

            date = CheckingDate(passageDayTextBox.Text);

            if (date == DateTime.MinValue) return;

            command.Parameters.AddWithValue("PassageDay", $"{date.Month}/{date.Day}/{date.Year}");
            command.Parameters.AddWithValue("PlaceOfWork", placeOfWorkTextBox.Text);
            command.Parameters.AddWithValue("PlaceOfResidence", placeOfResidenceTextBox.Text);
            //command.Parameters.AddWithValue("PlaceOfResidence", string.IsNullOrWhiteSpace(placeOfResidenceTextBox.Text) ? "" : "@" + placeOfResidenceTextBox.Text.Replace(" ", string.Empty));

            command.ExecuteNonQuery();

            //ClearTextBoxes();
            PopupWindow("Успешно сохранено", 1.5);
        }

        private void PopupWindow(string text, double sec)
        {
            myPopup.IsOpen = true;
            popupName.Text = text;

            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(sec)
            };
            timer.Tick += (s, args) =>
            {
                myPopup.IsOpen = false;
                timer.Stop();
            };
            timer.Start();
        }

        private DateTime CheckingDate(string dateText)
        {
            DateTime date;

            try
            {
                date = DateTime.Parse(dateText.Replace(" ", string.Empty));

                return date;
            }
            catch (Exception)
            {
                PopupWindow("Некорректно введена дата!", 2);

                return DateTime.MinValue;
            }
        }

        private void ClearTextBoxes()
        {
            foreach (var child in grid.Children)
            {
                if (child is TextBox textBox)
                {
                    textBox.Clear();
                }
            }

            lastNameTextBox.Focus();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var currentTextBox = sender as TextBox;

            if (currentTextBox == null) return;

            int index = Array.IndexOf(textBoxes, currentTextBox);

            if (index < 0) return;

            SwitchingToBoxing(e, currentTextBox, index);

            if (currentTextBox.Name == "birthDayTextBox" || currentTextBox.Name == "passageDayTextBox")
            {
                HandleDateTextBoxKeyDown(e, currentTextBox);
            }
        }

        private void SwitchingToBoxing(KeyEventArgs e, TextBox currentTextBox, int index)
        {
            if (e.Key == Key.Right && Keyboard.Modifiers == ModifierKeys.Shift && index < textBoxes.Length - 1)
            {
                textBoxes[index + 1].Focus();
            }
            else if (e.Key == Key.Left && Keyboard.Modifiers == ModifierKeys.Shift && index > 0)
            {
                textBoxes[index - 1].Focus();
            }
            else if (e.Key == Key.Enter)
            {
                Button_Click(null, null);
            }

            if (e.Key == Key.Right && Keyboard.Modifiers == ModifierKeys.Shift && index == textBoxes.Length - 1)
            {
                index = 0;
                textBoxes[index].Focus();
            }
            else if (e.Key == Key.Left && Keyboard.Modifiers == ModifierKeys.Shift && index == 0)
            {
                index = textBoxes.Length - 1;
                textBoxes[index].Focus();
            }            
        }

        private void HandleDateTextBoxKeyDown(KeyEventArgs e, TextBox currentTextBox)
        {
            if (!((e.Key >= Key.D0 && e.Key <= Key.D9) ||
          (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ||
          e.Key == Key.Back ||
          e.Key == Key.Left || e.Key == Key.Right))
            {
                e.Handled = true;
                return;
            }

            if (currentTextBox.Text.Length == 2 || currentTextBox.Text.Length == 5)
            {
                int caretPosition = currentTextBox.CaretIndex;

                currentTextBox.Text += '.';
                currentTextBox.CaretIndex = caretPosition + 1;
            }

            if (currentTextBox.Text.Length >= 10 &&
                e.Key != Key.Back && e.Key != Key.Left && e.Key != Key.Right)
            {
                e.Handled = true;
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            SqlCommand command = new SqlCommand($"DELETE FROM Fluorography WHERE LastName LIKE '%test%'", sqlConnection);

            command.ExecuteNonQuery();

            PopupWindow("Успешно удалено", 1.5);
        }

        private void LoadDataToGrid(string query)
        {
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query, sqlConnection);
            dataSet = new DataTable();
            sqlDataAdapter.Fill(dataSet);
            dataGrid.ItemsSource = dataSet.DefaultView;
        }

        private void LastNameSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dataSet != null)
            {
                string filterText = lastNameSearch.Text;
                dataSet.DefaultView.RowFilter = $"Фамилия LIKE '%{filterText}%' OR Имя LIKE '%{filterText}%' OR " +
                                                $"Отчество LIKE '%{filterText}%'";
            }
        }

        private void Grid_Loaded_1(object sender, RoutedEventArgs e)
        {
            LoadDataToGrid("SELECT Id, LastName AS 'Фамилия', FirstName AS 'Имя', " +
                        "Patronymic AS 'Отчество', FORMAT(BirthDay, 'dd-MM-yyyy') AS 'Дата рождения', FORMAT(PassageDay, 'dd-MM-yyyy') AS 'Дата прохождения', " +
                        "PlaceOfWork AS 'Место работы', PlaceOfResidence AS 'Место проживания' FROM Fluorography");

            UpdateTotalCount();
            UpdateOverdueCount();
            UpdateExpiringCount();
        }

        private void UpdateTotalCount()
        {
            SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Fluorography", sqlConnection);
            int total = (int)command.ExecuteScalar();

            totalCount.Content = $"Всего: {total}";
        }

        private void UpdateOverdueCount()
        {
            SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Fluorography WHERE DATEDIFF(DAY, PassageDay, GETDATE()) > 365", sqlConnection);
            int overdue = (int)command.ExecuteScalar();

            overdueCount.Content = $"Просроченные: {overdue}";
        }

        private void UpdateExpiringCount()
        {
            SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Fluorography WHERE DATEDIFF(DAY, PassageDay, GETDATE()) > 335 AND  DATEDIFF(DAY, PassageDay, GETDATE()) < 365", sqlConnection);
            int expiring = (int)command.ExecuteScalar();

            expiringCount.Content = $"Истекающие: {expiring}";
        }

        private void dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            DataRowView rowView = e.Row.Item as DataRowView;

            if (rowView != null && rowView["Дата прохождения"] != DBNull.Value)
            {
                try
                {
                    string passageDayStr = rowView["Дата прохождения"].ToString();

                    DateTime passageDay = DateTime.ParseExact(passageDayStr, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);

                    if ((DateTime.Now - passageDay).TotalDays > 365)
                    {
                        e.Row.Background = new SolidColorBrush(Colors.Gray);
                    }
                    else if ((DateTime.Now - passageDay).TotalDays > 335)
                    {
                        e.Row.Background = new SolidColorBrush(Colors.LightGray);
                    }
                }
                catch (FormatException)
                {
                    MessageBox.Show("Неверный формат даты в поле Дата");
                }
            }
        }

        private void overdueButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDataToGrid("SELECT Id, LastName AS 'Фамилия', FirstName AS 'Имя', " +
                      "Patronymic AS 'Отчество', FORMAT(BirthDay, 'dd-MM-yyyy') AS 'Дата рождения', FORMAT(PassageDay, 'dd-MM-yyyy') AS 'Дата прохождения', " +
                      "PlaceOfWork AS 'Место работы', PlaceOfResidence AS 'Место проживания' " +
                      "FROM Fluorography WHERE DATEDIFF(DAY, PassageDay, GETDATE()) > 365 OR DATEDIFF(DAY, PassageDay, GETDATE()) > 335");
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            lastNameSearch.Clear();
        }

        private void showAllButton_Click(object sender, RoutedEventArgs e)
        {
            Grid_Loaded_1(sender, e);
        }

        private void DataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGrid dg = (DataGrid)sender;
            selectedRow = (DataRowView)dg.SelectedItem;

            if (selectedRow != null) dataGridContextMenu.IsOpen = true;
        }

        private void EditDate_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRow != null)
            {
                editDatePopup.IsOpen = true;

                newPassageDatePicker.SelectedDate = DateTime.Parse(selectedRow["Дата прохождения"].ToString());
            }
        }

        private void UpdateDate_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRow != null && newPassageDatePicker.SelectedDate.HasValue)
            {
                DateTime newDate = newPassageDatePicker.SelectedDate.Value;

                SqlCommand command = new SqlCommand(
                    "UPDATE Fluorography SET PassageDay = @newDate WHERE Id = @id", sqlConnection);

                command.Parameters.AddWithValue("@newDate", newDate);
                command.Parameters.AddWithValue("@id", selectedRow["Id"]);

                command.ExecuteNonQuery();

                editDatePopup.IsOpen = false;

                Grid_Loaded_1(null, null);
                PopupWindow("Дата успешно обновлена", 1.5);
            }
        }

        private void CancelEdit_Click(object sender, RoutedEventArgs e)
        {
            editDatePopup.IsOpen = false;
        }

        private void DeleteRow_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem is DataRowView selectedRow)
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить эту запись?",
                                                          "Подтверждение удаления",
                                                          MessageBoxButton.YesNo,
                                                          MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    int id = Convert.ToInt32(selectedRow["Id"]);

                    SqlCommand command = new SqlCommand("DELETE FROM Fluorography WHERE Id = @Id", sqlConnection);

                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();

                    Grid_Loaded_1(sender, e);

                    PopupWindow("Запись успешно удалена", 1.5);
                }
            }
        }
    }
}