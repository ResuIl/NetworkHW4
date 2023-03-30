using Server.Models;
using System.Windows;
using System.Windows.Input;

namespace Client.Views;

public partial class AddCar : Window
{
    public Car Car { get; set; }
    public AddCar()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(tBox_Model.Text) && !string.IsNullOrEmpty(tBox_Make.Text) && !string.IsNullOrEmpty(tBox_Year.Text) && !string.IsNullOrEmpty(tBox_VIN.Text) && !string.IsNullOrEmpty(tBox_Color.Text))
        {
            Car = new Car
            {
                Model = tBox_Model.Text,
                Make = tBox_Make.Text,
                Year = ushort.Parse(tBox_Year.Text),
                VIN = tBox_VIN.Text,
                Color = tBox_Color.Text
            };

            DialogResult = true;
        } else
            MessageBox.Show("Please Fill All Blanks.");
    }

    private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (!char.IsDigit(e.Text, e.Text.Length - 1))
            e.Handled = true;
    }
}
