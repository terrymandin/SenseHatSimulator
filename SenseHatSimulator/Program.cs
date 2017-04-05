using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace SenseHatSimulator
{
    class Program
    {
        static DeviceClient raspberryPi;
        static string iotHubUri = "tm-iot-hub-free.azure-devices.net";

        static void Main(string[] args)
        {
            Console.WriteLine("Simulated device\n");
            raspberryPi = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("<Device Name>", "<Device Key>"), TransportType.Amqp);
            DeviceContext raspberryPiContext = new DeviceContext() { temperature = 0 };
            while (true)
            {
                SendDeviceToCloudMessagesAsync("<Device Name>", raspberryPi, 49.171402, -124.005349, raspberryPiContext);
                Task.Delay(1000).Wait();
            }
        }
        private static async void SendDeviceToCloudMessagesAsync(string name, DeviceClient deviceClient, double latitude, double longitude, DeviceContext deviceContext)
        {
            var telemetryDataPoint = new
            {
                DeviceId = name,
                TimeStamp = DateTime.UtcNow,
                Temperature = GetRandom(-10, 20, deviceContext.temperature, deviceContext.temperatureSeed),
                ExternalTemperature = GetRandom(0, 5, deviceContext.temperature, deviceContext.externaltemperatureSeed),
                Humidity = GetRandom(0, 5, deviceContext.temperature, deviceContext.humiditySeed),
                pitch = GetRandom(-10, 10, deviceContext.pitch, deviceContext.pitchSeed),
                yaw = GetRandom(-10, 10, deviceContext.yaw, deviceContext.yawSeed),
                roll = GetRandom(-10, 10, deviceContext.roll, deviceContext.rollSeed),
            };

            var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            try
            {
                await deviceClient.SendEventAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine(ex.InnerException.Message);
            }

            deviceContext.IncrementSeed();

            Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

        }

        private static double GetRandom(double minVal, double maxVal, double incdec, double seed)
        {
            Random rand = new Random();
            return Math.Abs(rand.NextDouble() * (maxVal - minVal) + minVal + (maxVal - minVal) * Math.Sin(seed) + incdec);
        }

        private class DeviceContext
        {
            private const double INCREMENT = 0.1;
            public DeviceContext()
            {
                Random rand = new Random();
                pitchSeed = rand.NextDouble() * 360;
                yawSeed = rand.NextDouble() * 360;
                rollSeed = rand.NextDouble() * 360;
                temperatureSeed = rand.NextDouble() * 360;
                externaltemperatureSeed = rand.NextDouble() * 360;
                humidity = rand.NextDouble() * 360;
            }

            public double pitch = 0;
            public double yaw = 0;
            public double roll = 0;
            public double temperature = 0;
            public double externaltemperature = 0;
            public double humidity = 0;

            public double pitchSeed = 0;
            public double yawSeed = 0;
            public double rollSeed = 0;
            public double temperatureSeed = 0;
            public double externaltemperatureSeed = 0;
            public double humiditySeed = 0;

            public void IncrementSeed()
            {
                pitchSeed += INCREMENT;
                yawSeed += INCREMENT;
                rollSeed += INCREMENT;
                temperatureSeed += INCREMENT;
                externaltemperature += INCREMENT;
                humidity += INCREMENT;
            }
        }
    }
}

