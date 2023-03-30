using System.Net.Http;
using System;
using System.Windows;
using System.Windows.Controls;
using Server.Models;
using System.IO;
using System.Net.Sockets;
using System.Net;
using static System.Net.Mime.MediaTypeNames;
using System.Text.Json;
using Client.Views;

namespace Client;

public partial class MainWindow : Window
{
    public Car Car { get; set; }
    BinaryWriter bw;
    BinaryReader br;
    public MainWindow()
    {
        InitializeComponent();
        cBox_Methods.ItemsSource = Enum.GetValues(typeof(HttpMethods));
        var client = new TcpClient("127.0.0.1", 45678);
        var serverStream = client.GetStream();
        bw = new BinaryWriter(serverStream);
        br = new BinaryReader(serverStream);
    }

    private void cBox_Methods_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox cb)
        {
            if (cb.SelectedItem is HttpMethods method && (method == HttpMethods.GET || method == HttpMethods.DELETE || method == HttpMethods.PUT))
                tBox.IsEnabled = true;
        }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (cBox_Methods.SelectedIndex != -1 && cBox_Methods.SelectedItem is HttpMethods method)
            ExecuteCommand(method);
    }

    private async void ExecuteCommand(HttpMethods method)
    {
        Command command = new Command();
        switch(method)
        {
            case HttpMethods.GET:
                if (tBox.Text is not null)
                {

                    var Car = new Car();
                    Car.Id = int.Parse(tBox.Text);

                    command.Method = HttpMethods.GET;
                    command.Car = Car;


                    string jsonString = JsonSerializer.Serialize(command);
                    bw.Write(jsonString);

                    var jsonResponse = br.ReadString();

                    var car = JsonSerializer.Deserialize<Car>(jsonResponse);
                    if (car is not null && car.Make is not null && car.Make.Length > 0)
                    {
                        PutCar putCar = new PutCar(car, false);
                        putCar.ShowDialog();
                    }
                    else
                        MessageBox.Show("Car couldn't find");

                    tBox.IsEnabled = false;
                    tBox.Text = string.Empty;
                }
                break;
            case HttpMethods.DELETE:
                if (tBox.Text is not null)
                {
                    var Car = new Car();
                    Car.Id = int.Parse(tBox.Text);

                    command.Method = HttpMethods.DELETE;
                    command.Car = Car;

                    string jsonString = JsonSerializer.Serialize(command);
                    bw.Write(jsonString);
                    bool isDeleted = br.ReadBoolean();
                    MessageBox.Show(isDeleted ? "Deleted Successfully" : "Unexpected Error");

                    tBox.IsEnabled = false;
                    tBox.Text = string.Empty;
                }
                break;
            case HttpMethods.PUT:
                if (!string.IsNullOrEmpty(tBox.Text))
                {
                    var Car = new Car();
                    Car.Id = int.Parse(tBox.Text);
                    command.Method = HttpMethods.GET;
                    command.Car = Car;
                    string jsonString = JsonSerializer.Serialize(command);
                    bw.Write(jsonString);

                    var jsonResponse = br.ReadString();
                    var car = JsonSerializer.Deserialize<Car>(jsonResponse);

                    PutCar putCar = new PutCar(car, true);
                    putCar.ShowDialog();

                    command.Method = HttpMethods.PUT;
                    command.Car = putCar.Car;

                    string jsonStringPut = JsonSerializer.Serialize(command);
                    bw.Write(jsonStringPut);

                    MessageBox.Show(br.ReadBoolean() ? "Put Successfully" : "Unexpected Error");
                    bw.Flush();
                }
                break;
            case HttpMethods.POST:
                AddCar addCar = new AddCar();
                addCar.ShowDialog();
                
                if (addCar.Car is not null && !string.IsNullOrEmpty(addCar.Car.Make) && !string.IsNullOrEmpty(addCar.Car.Model))
                {
                    command.Method = HttpMethods.POST;
                    command.Car = addCar.Car;

                    string jsonString = JsonSerializer.Serialize(command);
                    bw.Write(jsonString);

                    MessageBox.Show(br.ReadBoolean() ? "Added Successfully" : "Unexpected Error");
                }
                break;
        }
    }   
}
