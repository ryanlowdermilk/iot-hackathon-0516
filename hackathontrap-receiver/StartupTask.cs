using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using System.Threading;
using Windows.Devices.Gpio;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace hackathontrap_receiver
{

    public sealed class StartupTask : IBackgroundTask
    {

        private BackgroundTaskDeferral _deferral;
        private static string _deviceId = "receiver";
        private static string _deviceKey = "/7pZKSWEiSNI/2RYmzqzlEndQQGDB9Zh6QnWTvHvyuY=";
        private static string _ioTHubHostName = "iothubhackathon.azure-devices.net";
        private const int _buttonPin = 4;
        private GpioPin _button;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var cts = new CancellationTokenSource();
            await ReceiveCommandLoop(cts.Token);
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

        static DeviceClient GetDeviceClient()
        {
            // Generate the symmetric key
            var authMethod = new DeviceAuthenticationWithRegistrySymmetricKey(_deviceId, _deviceKey);

            // Build the connection string
            var connecitonString = IotHubConnectionStringBuilder.Create(_ioTHubHostName, authMethod).ToString();
            var deviceClient = DeviceClient.CreateFromConnectionString("HostName=iothubhackathon.azure-devices.net;DeviceId=receiver;SharedAccessKey=/7pZKSWEiSNI/2RYmzqzlEndQQGDB9Zh6QnWTvHvyuY=", TransportType.Http1);
            return deviceClient;
        }
    }
}
