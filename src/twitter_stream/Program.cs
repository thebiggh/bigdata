using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Streaming.Parameters;

namespace twitter_stream
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
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            using (var sendClient = DisposableDecorator.Create(EventHubClient.CreateFromConnectionString(config["SEND_CLIENT"]), x => x.Close()))
            {
                var keyword = config["KEYWORD"];

                var credentials = Auth.CreateCredentials(
                    config["CLIENT_ID"],
                    config["CLIENT_SECRET"],
                    config["ACCESS_TOKEN"],
                    config["ACCESS_SECRET"]
                );

                var http = new HttpClient();
                http.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", config["SUBSCRIPTION_KEY"]);

                var stream = Stream.CreateFilteredStream(credentials, TweetMode.Extended);
                stream.AddTweetLanguageFilter(LanguageFilter.English);
                stream.FilterLevel = StreamFilterLevel.Low;
                stream.AddTrack(keyword);

                stream.MatchingTweetReceived += async (s, e) => 
                {
                    try
                    {
                        if (!e.Tweet.IsRetweet)
                        {
                            var text = e.Tweet.Entities.Urls.Aggregate(e.Tweet.FullText, (t, u) => t.Replace(u.DisplayedURL, string.Empty));
                            text = e.Tweet.Entities.Hashtags.Aggregate(text, (t, h) => t.Replace($"#{h.Text}", h.Text));
                            text = e.Tweet.Entities.UserMentions.Aggregate(text, (t, u) => t.Replace($"@{u.ScreenName}", u.Name));
                            Console.WriteLine(text);

                            var sentiment = default(decimal?);
                            using (var response = await http.PostAsync(
                                $"{config["ENDPOINT"]}/text/analytics/v2.0/sentiment", 
                                new StringContent(JsonConvert.SerializeObject(new 
                                    {
                                        documents = new []
                                        {
                                            new 
                                            {
                                                language = "en",
                                                id = 1,
                                                text
                                            }
                                        }
                                    }), 
                                    Encoding.UTF8, 
                                    "application/json")))
                            {
                                if (response.IsSuccessStatusCode)
                                {
                                    var json = JObject.Parse(await response.Content.ReadAsStringAsync());
                                    sentiment = json.SelectToken("documents[0].score").Value<decimal>();
                                }
                            }

                            var payload = new 
                            {
                                keyword,
                                text,
                                sentiment,
                                hash_tags = e.Tweet.Entities.Hashtags.Select(t => t.Text),
                                created_by = $"@{e.Tweet.CreatedBy.ScreenName}",
                                created_at = e.Tweet.CreatedAt,
                                twitter_data = JsonConvert.DeserializeObject(e.Json)
                            };

                            await sendClient.Target.SendAsync(new EventData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))));
                        }
                    }
                    catch (Exception x)
                    {
                        Console.WriteLine(x);
                    }
                };

                stream.StartStreamMatchingAllConditions();
                
                using (var cts = new CancellationTokenSource())
                {
                    Console.CancelKeyPress += (_1, _2) => cts.Cancel();
                    stream.StreamStopped += (_1, _2) => cts.Cancel();
                    cts.Token.WaitHandle.WaitOne();
                }

                stream.StopStream();
            }
        }
    }
}
