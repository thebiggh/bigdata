using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace iot_events
{
    class DisposableDecorator<T> : IDisposable 
    {
        private readonly T target;
        private readonly Action<T> cleanup;

        public DisposableDecorator(T target, Action<T> cleanup)
        {
            this.target = target;
            this.cleanup = cleanup;
        }

        ~DisposableDecorator()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static implicit operator T(DisposableDecorator<T> decorator) => decorator.target;

        public T Target => target;

        private void Dispose(bool disposing)
        {
            try
            {
                if (target != null)
                {
                    cleanup?.Invoke(target);
                }
            }
            catch (Exception) {}
        }
    }

    class DisposableDecorator
    {
        public static DisposableDecorator<TObj> Create<TObj>(TObj target, Action<TObj> action) => new DisposableDecorator<TObj>(target, action);
    }

    class Program
    {
        static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();

        static async Task MainAsync(string[] args)
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .AddCommandLine(args)
                    .Build();

                var receiveConnectionString = new EventHubsConnectionStringBuilder(config["RECEIVE_ENDPOINT"]);
                var sendConnectionString = new EventHubsConnectionStringBuilder(config["SEND_ENDPOINT"]);

                var offset = File.Exists("checkpoint.txt") ? File.ReadAllText("checkpoint.txt") : (Environment.GetEnvironmentVariable("CHECKPOINT") ?? PartitionReceiver.StartOfStream);

                using (var receiveClient = DisposableDecorator.Create(EventHubClient.CreateFromConnectionString(receiveConnectionString.ToString()), x => x.Close()))
                using (var sendClient = DisposableDecorator.Create(EventHubClient.CreateFromConnectionString(sendConnectionString.ToString()), x => x.Close()))
                using (var receiver = DisposableDecorator.Create(receiveClient.Target.CreateReceiver("$Default", "0", offset, new ReceiverOptions()), x => x.Close()))
                {
                    bool loop = true;
                    Console.CancelKeyPress += (_1, _2) => loop = false;

                    while (loop)
                    {
                        using (var batch = sendClient.Target.CreateBatch())
                        {
                            var events = await receiver.Target.ReceiveAsync(100);
                            
                            foreach (var e in events)
                            {
                                offset = e.SystemProperties.Offset;
                                var data = e.Properties.ToDictionary(p => p.Key, p => p.Value);
                                data["x-body"] = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(e.Body.Array));
                                data["x-enqueued"] = e.SystemProperties.EnqueuedTimeUtc;
                                data["x-offset"] = e.SystemProperties.Offset;
                                data["x-sequence"] = e.SystemProperties.SequenceNumber;
                                data["x-partition"] = e.SystemProperties.PartitionKey;
                                var json = JsonConvert.SerializeObject(data);
                                if (batch.TryAdd(new EventData(Encoding.UTF8.GetBytes(json))))
                                {
                                    Console.WriteLine(json);
                                }
                            }

                            if (batch.Count > 0)
                            {
                                await sendClient.Target.SendAsync(batch.ToEnumerable());
                                File.WriteAllText("checkpoint.txt", $"{offset}");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
