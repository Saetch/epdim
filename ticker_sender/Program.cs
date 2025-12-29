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
                if (Environment.GetEnvironmentVariable("ECHO") != null) {
                    await nc.PublishAsync<string>("sim.echo", $"Single echo at tick {tick}");
                    Console.WriteLine($"Publishing tick {tick} to sim echo");
                }
                else
                {
                    await nc.PublishAsync<int>("sim.time.tick", tick);
                    Console.WriteLine($"Publishing tick {tick} to sim time tick");
                }
                await Task.Delay(1000);  // wait 1 second
                if (Environment.GetEnvironmentVariable("SPECIAL_COMMAND") != null &&tick >= 15) {
                    await nc.PublishAsync<string>("sim.shutdown", "Automatic shutdown after more than 15 ticks");
                    break;
                }

            }
        }
    }
}
