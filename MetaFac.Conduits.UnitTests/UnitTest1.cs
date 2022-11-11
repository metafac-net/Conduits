using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MetaFac.Conduits.Testing;

namespace MetaFac.Conduits.UnitTests
{
    public class UnitTest1
    {
        [Fact]
        public async Task DisposedServiceShouldThrow()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            FakeConduitServer conduitServer;
            using var server = new ConduitServer(new WeatherService());
            using (conduitServer = new FakeConduitServer(server))
            {
                using var conduitClient = new FakeConduitClient(conduitServer);
                using var client = new WeatherClient(conduitClient, true);
                var weather = await client.GetWeather("Brisbane", cts.Token);
                weather.Should().NotBeNull();
                weather.Tag.Should().Be(WeatherTag.NotFound);
            }
            // repeat
            {
                using var client = new WeatherClient(new FakeConduitClient(conduitServer));
                var ex = await Assert.ThrowsAsync<ObjectDisposedException>(
                             async () =>
                             {
                                 var weather = await client.GetWeather("Brisbane", cts.Token);
                             });
                ex.Message.Should().StartWith("Cannot access a disposed object.");
            }
        }

        [Fact]
        public async Task ServiceCallsAreRepeatable()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var server = new ConduitServer(new WeatherService());
            using var conduitServer = new FakeConduitServer(server);
            {
                using var conduitClient = new FakeConduitClient(conduitServer);
                using var client = new WeatherClient(conduitClient);
                await client.UpdateWeather(new WeatherData(WeatherTag.WeatherData, "Brisbane", 31, DateTime.UtcNow), cts.Token);
                var weather = await client.GetWeather("Brisbane", cts.Token);
                weather.TemperatureC.Should().Be(31.0D);
            }
            // repeat
            {
                using var client = new WeatherClient(new FakeConduitClient(conduitServer));
                var weather = await client.GetWeather("Brisbane", cts.Token);
                weather.TemperatureC.Should().Be(31.0D);
            }
        }

        [Fact]
        public async Task StreamingServiceCalls()
        {
            var cts1 = Debugger.IsAttached
                ? new CancellationTokenSource(TimeSpan.FromSeconds(30))
                : new CancellationTokenSource(TimeSpan.FromSeconds(5));

            using var server = new ConduitServer(new WeatherService());
            using var conduitServer = new FakeConduitServer(server);
            {
                using var client = new WeatherClient(new FakeConduitClient(conduitServer));
                List<WeatherData> responses = new List<WeatherData>();
                Exception? fault = null;
                var subscriber = Task.Run(async () =>
                {
                    try
                    {
                        await foreach (var response in client.GetWeatherStream("Brisbane", cts1.Token))
                        {
                            responses.Add(response);
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        // expected
                        fault = null;
                    }
                    catch (OperationCanceledException)
                    {
                        // expected
                        fault = null;
                    }
                    catch (Exception ex)
                    {
                        fault = ex;
                    }
                });
                var publisher = Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    await client.UpdateWeather(new WeatherData(WeatherTag.WeatherData, "Brisbane", 31, DateTime.UtcNow), cts1.Token);
                    await client.UpdateWeather(new WeatherData(WeatherTag.WeatherData, "Brisbane", 32, DateTime.UtcNow), cts1.Token);
                });
                await Task.WhenAll(subscriber, publisher);
                fault.Should().BeNull();
                responses.Count.Should().Be(2);
                responses[0].TemperatureC.Should().Be(31.0D);
                responses[1].TemperatureC.Should().Be(32.0D);
            }
            // repeat
            {
                var cts2 = Debugger.IsAttached
                    ? new CancellationTokenSource(TimeSpan.FromSeconds(30))
                    : new CancellationTokenSource(TimeSpan.FromSeconds(5));

                using var client = new WeatherClient(new FakeConduitClient(conduitServer));
                List<WeatherData> responses = new List<WeatherData>();
                Exception? fault = null;
                try
                {
                    await foreach (var response in client.GetWeatherStream("Brisbane", cts2.Token))
                    {
                        responses.Add(response);
                    }
                }
                catch (TaskCanceledException)
                {
                    // expected
                    fault = null;
                }
                catch (OperationCanceledException)
                {
                    // expected
                    fault = null;
                }
                catch (Exception ex)
                {
                    fault = ex;
                }
                fault.Should().BeNull();
                responses.Count.Should().Be(1);
                responses[0].TemperatureC.Should().Be(32.0D);
            }
        }
    }
}