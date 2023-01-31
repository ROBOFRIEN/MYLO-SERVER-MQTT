
using MQTTnet;
using MQTTnet.Server;
using Serilog;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MYLO_SERVER_MQTT
{
    class Program
    {

        private static int MessageCounter = 0;

        const int serverPort = 707;

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            MqttServerOptionsBuilder options = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithDefaultEndpointPort(serverPort)
                .WithConnectionValidator(OnNewConnection)
                .WithApplicationMessageInterceptor(OnNewMessage);


            IMqttServer mqttServer = new MqttFactory().CreateMqttServer();

            mqttServer.StartAsync(options.Build()).GetAwaiter().GetResult();

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            string serverIP = string.Empty;

            for(int i = 0; i < host.AddressList.Length; i++)
            {
                if(host.AddressList[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    serverIP = host.AddressList[i].ToString();
                    break;
                }
            }
            Log.Logger.Information(
                "Server IP: {ip}, PORT: {port}",
                serverIP,
                serverPort
                );

            Console.ReadLine();
        }

        public static void OnNewConnection(MqttConnectionValidatorContext context)
        {
            Log.Logger.Information(
                    "New connection: ClientId = {clientId}, Endpoint = {endpoint}, CleanSession = {cleanSession}",
                    context.ClientId,
                    context.Endpoint,
                    context.CleanSession);
        }

        public static void OnNewMessage(MqttApplicationMessageInterceptorContext context)
        {
            var payload = context.ApplicationMessage?.Payload == null ? null : Encoding.UTF8.GetString(context.ApplicationMessage?.Payload);

            MessageCounter++;

            Log.Logger.Information(
                "MessageId: {MessageCounter} - TimeStamp: {TimeStamp} -- Message: ClientId = {clientId}, Topic = {topic}, Payload = {payload}, QoS = {qos}, Retain-Flag = {retainFlag}",
                MessageCounter,
                DateTime.Now,
                context.ClientId,
                context.ApplicationMessage?.Topic,
                payload,
                context.ApplicationMessage?.QualityOfServiceLevel,
                context.ApplicationMessage?.Retain);
        }



    }
}
