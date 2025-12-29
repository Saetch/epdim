using NATS.Net;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EpidemicSimulation
{
    class SimulationService
    {
        static async Task Main(string[] args)
        {
            // Connect to local NATS server (use NATS_URL env var or default to localhost:4222)
            var url = Environment.GetEnvironmentVariable("NATS_URL") 
                        ?? "nats://127.0.0.1:4222";
            await using var nc = new NatsClient(url);

            int infectedCount = 1;
            Console.WriteLine($"Starting si mulation, initial infected: {infectedCount}");

            var cts = new CancellationTokenSource();
            // Subscribe asynchronously to the "sim.time.tick" subject, expecting an int payload
            await foreach (var msg in nc.SubscribeAsync<int>("sim.time.tick", cancellationToken: cts.Token))
            {
                int tick = msg.Data;
                // Update simulation state: simple growth (e.g., add the tick count to infected)
                infectedCount += tick;
                Console.WriteLine($"Tick {tick}: Infected count = {infectedCount}");
            }
        }
    }
}
