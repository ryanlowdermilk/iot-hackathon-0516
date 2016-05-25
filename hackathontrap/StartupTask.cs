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
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            var cts = new CancellationTokenSource();
            var taskSending = TelemetrySendLoop(cts.Token);
        }

        static async Task TelemetrySendLoop(CancellationToken ct)
        {
            try
            {
                // Get DeviceId,DeviceKey & IoTHubHostName from config.
                var DeviceId = "hackathontrap";
                var DeviceKey = "BaBrZjE2nnpUIQ+QiG9rJVy4zWhdNmaqw9DrckrM05s=";
                var IoTHubHostName = "iothubhackathon.azure-devices.net";
                
                // Generate the symmetric key
                var authMethod = new DeviceAuthenticationWithRegistrySymmetricKey(DeviceId, DeviceKey);

                // Build the connection string
                var connecitonString = IotHubConnectionStringBuilder.Create(IoTHubHostName, authMethod).ToString();
                var Deviceclient = DeviceClient.CreateFromConnectionString("HostName=iothubhackathon.azure-devices.net;DeviceId=hackathontrap;SharedAccessKey=BaBrZjE2nnpUIQ+QiG9rJVy4zWhdNmaqw9DrckrM05s=", TransportType.Http1);

                while (!ct.IsCancellationRequested)
                {

                    var messageObject = new
                    {
                        DeviceID = DeviceId,
                        Temperature = 34.2,
                        ExternalTemperature = 38.7,
                        Humidity = 37.7
                    };

                    // to Json
                    var messageJson = JsonConvert.SerializeObject(messageObject);
                    // to device message
                    var message = new Message(Encoding.UTF8.GetBytes(messageJson));

                    await Deviceclient.SendEventAsync(message);
                    await Task.Delay(200);
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
        }
    }
}
