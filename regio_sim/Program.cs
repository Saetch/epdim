using NATS.Net;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EpidemicSimulation
{
    class SimulationService
    {

        static CancellationTokenSource cts = new CancellationTokenSource();
        static UInt64 infectedCount = 1;
        
        //give unique UUID to each simulation service
        static string serviceId = Guid.NewGuid().ToString();


        static async Task Main(string[] args)
        {
            // Connect to local NATS server (use NATS_URL env var or default to localhost:4222)
            var url = Environment.GetEnvironmentVariable("NATS_URL") 
                        ?? "nats://127.0.0.1:4222";
            await using var nc = new NatsClient(url);

            Console.WriteLine($"Starting si mulation, initial infected: {infectedCount}");

            // Subscribe asynchronously to the "sim.time.tick" subject, expecting an int payload
            await Task.WhenAll(
                new SimulationService().TickHandler(nc),
                new SimulationService().ResetHandler(nc),
                new SimulationService().ShutdownHandler(nc),
                new SimulationService().EchoQueueHandler(nc)
            );
        }


        async Task TickHandler(NatsClient nc)
        {

            await foreach (var msg in nc.SubscribeAsync<int>("sim.time.tick", cancellationToken: cts.Token))
            {
                infectedCount += (UInt64)msg.Data;
                Console.WriteLine($"Service {serviceId} received Tick {msg.Data}: infected = {infectedCount}");
            }

        }

        async Task ResetHandler(NatsClient nc)
        {

            await foreach (var msg in nc.SubscribeAsync<int>("sim.reset", cancellationToken: cts.Token))
            {
                infectedCount = (UInt64)msg.Data;
                Console.WriteLine($"Service {serviceId} received Reset {msg.Data}: infected = {infectedCount}");
            }

        }

        async Task EchoQueueHandler(NatsClient nc)
        {

            await foreach (var msg in nc.SubscribeAsync<string>("sim.echo", queueGroup: "echo_workers", cancellationToken: cts.Token))
            {
                Console.WriteLine($"Service {serviceId} received Echo: {msg.Data}");
            }

        }

        async Task ShutdownHandler(NatsClient nc)
        {

            await foreach (var msg in nc.SubscribeAsync<string>("sim.shutdown", cancellationToken: cts.Token))
            {
                Console.WriteLine($"Service {serviceId} received Shutdown command. Shutting down... : {msg.Data}");
                cts.Cancel();
            }

        }

    }
}
