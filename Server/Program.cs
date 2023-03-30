using System.Net.Sockets;
using System.Net;
using System.Text.Json;
using Server.Models;
using System.Net.Http;

List<Car> cars = new List<Car>();

var id = cars.Count;

var ip = IPAddress.Parse("127.0.0.1");
var port = 45678;

var listener = new TcpListener(ip, port);
listener.Start();
Console.WriteLine("Listening...");

Car? GetCarById(int id)
{
    if (cars.Count > 0)
    {
        foreach (var car in cars)
        {
            if (car.Id == id)
                return car;
        }
    }
    return null;
}

bool Delete(int id)
{
    if (cars.Count > 0)
    {
        foreach (var car in cars)
        {
            if (car.Id == id)
            {
                cars.Remove(car);
                return true;
            }
        }
        return false;
    }
    return false;
}

while (true)
{
    var client = listener.AcceptTcpClient();

    var clientStream = client.GetStream();
    var br = new BinaryReader(clientStream);
    var bw = new BinaryWriter(clientStream);

    while (true)
    {
        var jsonStr = br.ReadString();
        var command = JsonSerializer.Deserialize<Command>(jsonStr);

        Console.WriteLine(command.Method);

        if (command is null)
            continue;


        switch (command.Method)
        {
            case HttpMethods.GET:
                {
                    if (command.Car is not null)
                    {
                        int idCar = command.Car.Id;
                        Console.WriteLine(idCar);
                        var gettedCar = GetCarById(idCar);
                        if (gettedCar != null)
                        {
                            var jsonResponse = JsonSerializer.Serialize(gettedCar);
                            Console.WriteLine(jsonResponse);
                            bw.Write(jsonResponse.ToString());
                            Console.WriteLine("SENT");
                        }
                        else bw.Write(JsonSerializer.Serialize(new Car()));
                    }
                    break;
                }
            case HttpMethods.POST:
                if (command.Car is not null)
                {
                    command.Car.Id = ++id;
                    cars.Add(command.Car);
                    bw.Write("true");
                    Console.WriteLine(cars.Count.ToString());
                }
                else bw.Write(false);
                break;
            case HttpMethods.PUT:
                Console.WriteLine(command.Car.Id);
                for (int i = 0; i < cars.Count; i++)
                {
                    if (cars[i].Id == command.Car.Id)
                    {

                        cars[i].Model = command.Car.Model;
                        cars[i].Make = command.Car.Make;
                        cars[i].VIN = command.Car.VIN;
                        cars[i].Color = command.Car.Color;
                        cars[i].Year = command.Car.Year;

                        bw.Write(true);
                        break;
                    }
                }
                bw.Write(false);

                Console.WriteLine(cars[0].Model);
                break;
            case HttpMethods.DELETE:
                Console.WriteLine(cars[0].Model);
                if (command.Car is not null)
                {
                    int idCar = command.Car.Id;
                    var jsonResponse = JsonSerializer.Serialize(Delete(idCar));
                    bw.Write(jsonResponse);
                }
                else bw.Write(false);
                break;
            default:
                break;
        }
    }
}