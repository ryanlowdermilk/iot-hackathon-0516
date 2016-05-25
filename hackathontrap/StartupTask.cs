using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace hackathontrap
{
    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;
        private static string _deviceId = "hackathontrap";
        private static string _deviceKey = "BaBrZjE2nnpUIQ+QiG9rJVy4zWhdNmaqw9DrckrM05s=";
        private static string _ioTHubHostName = "iothubhackathon.azure-devices.net";

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            var cts = new CancellationTokenSource();
            var taskSending = TelemetrySendLoop(cts.Token);
            var taskReceive = ReceiveCommandLoop(cts.Token);
        }

        static async Task ReceiveCommandLoop(CancellationToken ct)
        {
            var deviceClient = GetDeviceClient();

            while (!ct.IsCancellationRequested)
            {
                Message receivedMessage = await deviceClient.ReceiveAsync();

                if (receivedMessage == null) continue;

                var messageText = Encoding.ASCII.GetString(receivedMessage.GetBytes());

                //TODO: perform some action based on message

                await deviceClient.CompleteAsync(receivedMessage);
            }
        }

        static async Task TelemetrySendLoop(CancellationToken ct)
        {
            try
            {
                var deviceClient = GetDeviceClient();

                while (!ct.IsCancellationRequested)
                {

                    var messageObject = new
                    {
                        DeviceID = _deviceId,
                        Temperature = 34.2,
                        ExternalTemperature = 38.7,
                        Humidity = 37.7,
                        Timestamp = DateTime.Now
                    };

                    // to Json
                    var messageJson = JsonConvert.SerializeObject(messageObject);
                    // to device message
                    var message = new Message(Encoding.UTF8.GetBytes(messageJson));

                    await deviceClient.SendEventAsync(message);
                    await Task.Delay(200);
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
        }

        static DeviceClient GetDeviceClient()
        {
            // Generate the symmetric key
            var authMethod = new DeviceAuthenticationWithRegistrySymmetricKey(_deviceId, _deviceKey);

            // Build the connection string
            var connecitonString = IotHubConnectionStringBuilder.Create(_ioTHubHostName, authMethod).ToString();
            var deviceClient = DeviceClient.CreateFromConnectionString("HostName=iothubhackathon.azure-devices.net;DeviceId=hackathontrap;SharedAccessKey=BaBrZjE2nnpUIQ+QiG9rJVy4zWhdNmaqw9DrckrM05s=", TransportType.Http1);

            return deviceClient;
        }
    }
}
