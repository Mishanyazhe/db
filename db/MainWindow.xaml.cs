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

namespace db
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SqlConnection sqlConnection = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["Database"].ConnectionString);

            sqlConnection.Open();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {


            SqlCommand command = new SqlCommand(
                $"INSERT INTO [Student] (FirstName, LastName, Patronymic, birthDay, PlaceOfBirth, PhoneNumber, TgUserName) VALUES (@FirstName, @LastName, @Patronymic, @birthDay, @PlaceOfBirth, @PhoneNumber, @TgUserName)",
                sqlConnection);

            DateTime date = DateTime.Parse(birthDayTextBox.Text);

            command.Parameters.AddWithValue("FirstName", firstNameTextBox.Text);
            command.Parameters.AddWithValue("LastName", lastNameTextBox.Text);
            command.Parameters.AddWithValue("Patronymic", patronymicTextBox.Text);
            command.Parameters.AddWithValue("birthDay", $"{date.Month}/{date.Day}/{date.Year}");
            command.Parameters.AddWithValue("PlaceOfBirth", placeTextBox.Text);
            command.Parameters.AddWithValue("PhoneNumber", phoneNumberTextBox.Text);
            command.Parameters.AddWithValue("TgUserName", "@" + tgUsernameTextBox.Text);

            MessageBox.Show(command.ExecuteNonQuery().ToString());
        }
    }
}
