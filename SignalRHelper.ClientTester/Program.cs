using System;
using System.Diagnostics;
using SignalRHelper.Client;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalRHelper.ClientTester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var signalRClient = new SignalRClient("consoleTester", "http://localhost:52903/thorhub");

            signalRClient.Connected += () => Console.WriteLine($"{DateTimeOffset.UtcNow} : Connected");
            signalRClient.Disconnected += () => Console.WriteLine($"{DateTimeOffset.UtcNow} : Disconnected");
            signalRClient.ConnectionStatusChanged += (status) => Console.WriteLine($"{DateTimeOffset.UtcNow} : connection status changed to {status}");
            signalRClient.ExceptionOccured += (exception) => Console.WriteLine($"{DateTimeOffset.UtcNow} : exception occured : {exception}");

            signalRClient.Connect();
            signalRClient.HubConnection.On<Trace>("TraceReceived", (trace) => Console.WriteLine(true));
            Console.Read();
        }
    }
}
