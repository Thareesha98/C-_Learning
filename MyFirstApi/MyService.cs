using System;

interface IMyService
{
    void LogCreation(string message);
}


public class MyService : IMyService
{
    private readonly int _serviceId;
    public MyService()
    {
        _serviceId = new Random().Next(100000, 999999);
    }

    public void LogCreation(string message)
    {
        Console.WriteLine($"Service {_serviceId} created with message: {message}");
    }
}


