using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using MyWeather;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");

            var cts = new CancellationTokenSource();

            var weatherClient = new WeatherService.WeatherServiceClient(channel);
            var streamingData = weatherClient.RequestStreamData(new WeatherRequest {Location = "27.9506,-82.4572"});
            try
            {
                //Get the data as the server pushes it to us
                await foreach (var weatherData in streamingData.ResponseStream.ReadAllAsync(cancellationToken: cts.Token))
                {
                    Console.WriteLine($"{weatherData.Temperature}");
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine("Stream cancelled.");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

    }
}
