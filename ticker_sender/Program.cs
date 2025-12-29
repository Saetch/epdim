using NATS.Net;
using System;
using System.Threading.Tasks;

namespace EpidemicSimulation
{
    class TickPublisher
    {
        static async Task Main(string[] args)
        {
            // Connect to NATS (same URL strategy)
            var url = Environment.GetEnvironmentVariable("NATS_URL") 
                        ?? "nats://127.0.0.1:4222";
            await using var nc = new NatsClient(url);

            int tick = 0;
            while (true)
            {
                tick++;
                Console.WriteLine($"Publishing tick {tick}");
                await nc.PublishAsync<int>("sim.time.tick", tick);
                await Task.Delay(1000);  // wait 1 second
            }
        }
    }
}
