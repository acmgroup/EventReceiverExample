/**
 * Universal Event Json Receiver example over RabbitMQ.
 *
 * Required packages:
 * https://www.nuget.org/packages/RabbitMQ.Client (v12.0.1)
 * https://www.nuget.org/packages/Newtonsoft.Json/ (v5.1.0)
 *
 * Tested Using:
 * Microsoft Visual Studio Community 2017: v15.9.6
 * NuGet Package Manager: v4.6.0
 * Microsoft .NET Framework: v4.7.03056 (projected targeted at 4.6.1)
 *
 */

#define DEBUG

// Enable the directive below if you want the example to load an event
// json message from file.
// #define SAMPLE_FROM_FILE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using System.IO;

namespace EventReceiverExample
{
    class Program
    {
        #if SAMPLE_FROM_FILE
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please specify the name of the json file to decode.");
                return 1;
            }

            Console.WriteLine("-> Starting up...");

            var json = File.ReadAllText(args[0]);

            // Deserialize the JSON message into a basic class
            // called EventMsg so we can extract the primary
            // details before continuing.
            var msg = JsonConvert.DeserializeObject<UniversalMsg>(json);
            if ((msg.message_type == "event") && msg.message_ver == 1 && msg.valid)
            {
                // Now we can Deserialize the complete message and handle
                // it.
                Console.WriteLine("-> Received new message:");
                var eventMsg = JsonConvert.DeserializeObject<EventMessage>(json);
                handleEvent(eventMsg);
            } else {
                Console.WriteLine("-> Unable to decode message");
            }

            #if DEBUG
                Console.WriteLine("Press enter to close...");
                Console.ReadLine();
            #endif

            return 0;
        }

        #else

        static void Main(string[] args)
        {
            // Step 1: Create a connection factory, this will allow us to connect
            // to the remote RabbitMQ server using the credentials below.
            var factory = new ConnectionFactory()
            {
                HostName = Properties.Settings.Default.ServerHostname,
                UserName = Properties.Settings.Default.ServerUsername,
                Password = Properties.Settings.Default.ServerPassword,
                Port = Properties.Settings.Default.ServerPort,
                VirtualHost = Properties.Settings.Default.VirtualHost
            };

            Console.WriteLine("-> Connecting to Server: {0}:{1} (Virtualhost: {2})",
                Properties.Settings.Default.ServerHostname,
                Properties.Settings.Default.ServerPort,
                Properties.Settings.Default.VirtualHost);

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // Step 2: We declare a new queue called "events.receiver.test", the name is up to you.
                // If the queue does not exist yet, it will automatically be created on the
                // RabbitMQ server. If it already exists it will be used as is. Queues is
                // what keeps our messages until we read it.
                // See: https://www.rabbitmq.com/queues.html
                //
                // Note that the queue we create is "durable", meaning that data within the
                // queue will be persistent and is stored on disk and not just in memory. Data will
                // also remain in the queue if our client is not reading it or if the server reboots.
                Console.WriteLine("-> Declaring Queue: {0}",
                    Properties.Settings.Default.QueueName);

                channel.QueueDeclare(queue: Properties.Settings.Default.QueueName,
                                     durable: Properties.Settings.Default.FlagsDurable,
                                     exclusive: Properties.Settings.Default.FlagsExclusive,
                                     autoDelete: Properties.Settings.Default.FlagsAutoDelete,
                                     arguments: null);

                // Step 3: Bind our queue to an exchange. Exchanges routes messages to one or more queues. 
                // We need to bind our "events.receiver.test" queue to an existing exchange called "acm.leps". 
                // Furthermore, we specify a routing/binding key called "events.#" which
                // specifies to route *all* events to our queue.
                Console.WriteLine("-> Binding Queue: {0} to Exchange: {1} using Routing Key: {2}",
                    Properties.Settings.Default.QueueName,
                    Properties.Settings.Default.ExchangeName,
                    Properties.Settings.Default.RoutingKey);

                channel.QueueBind(
                    queue: Properties.Settings.Default.QueueName, 
                    exchange: Properties.Settings.Default.ExchangeName, 
                    routingKey: Properties.Settings.Default.RoutingKey);

                // Step 4: Setup a basic message consumer event handler.
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    // This callback is called whenever a new message becomes
                    // available.
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine("-> Received new message:");
                    Console.WriteLine(message);

                    // Deserialize the JSON message into a basic class
                    // called UniversalMsg so we can extract the primary
                    // details before continuing.
                    var msg = JsonConvert.DeserializeObject<UniversalMsg>(message);
                    if (msg.message_type == "event" && msg.message_ver == 1 && msg.valid)
                    {
                        // Now we can Deserialize the complete message and handle
                        // it.
                        var eventMsg = JsonConvert.DeserializeObject<EventMessage>(message);
                        handleEvent(eventMsg);
                    }

                    // Send an ACK back to the server that we have processed the
                    // message. We will automatically receive a new message afterwards
                    // if there is one available.
                    //
                    // IMPORTANT: If no ACK is returned to the server it will keep the
                    // message in the queue, potentially causing duplicates and queues
                    // becoming full.
                    Console.WriteLine("<- Sending Acknowledgement.");
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    Console.WriteLine("");
                };

                // Step 5: Start our message consumer
                Console.WriteLine("-> Starting consumer.");
                channel.BasicConsume(queue: Properties.Settings.Default.QueueName,
                                     autoAck: false,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
        #endif

        public static void handleEvent(EventMessage msg)
        {
            Console.WriteLine("==============================================================================");
            Console.WriteLine("imei:           {0}", msg.device.imei);
            Console.WriteLine("serial_no:      {0}", msg.device.serial_no);
            Console.WriteLine("message_type:   {0}", msg.message_type);
            Console.WriteLine("timestamp:      {0}", msg.timestamp.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
            Console.WriteLine("gateway:        {0}", msg.gateway);
            Console.WriteLine("code:           {0}", msg.code);
            Console.WriteLine("message:        {0}", msg.message);

            Console.WriteLine("source:         {0}: {1} [url: {2}]", msg.source.label, msg.source.value, msg.source.url);
            if (msg.location != null)
            {
                Console.WriteLine("location:       lat: {0} lng: {1} address: {2}", msg.location.latitude, msg.location.longitude, msg.location.address);
            }

            Console.WriteLine("importance:     {0}", msg.importance);
            Console.WriteLine("alert_level:    {0}", msg.alert_level);
            Console.WriteLine("color:          {0}", msg.color);
            Console.WriteLine("state:          {0}", msg.state);
            Console.WriteLine("ticket:         {0}", msg.ticket);
            Console.WriteLine("device:         imei: {0} serialno: {1} [url: {2}]", msg.device.imei, msg.device.serial_no, msg.device.url);

            foreach (var field in msg.data)
            {
                Console.WriteLine("data:           {0}: {1} [url: {2}]", field.label, field.value, field.url);
            }

            Console.WriteLine("==============================================================================");
        }

        public class UniversalMsg
        {
            /// <value>The message version, currently there is only a version 1 message.</value>
            public int message_ver { get; set; }

            /// <value>Message type, can be "register", "gps", "history", "status", "event", "heartbeat".</value>
            public string message_type { get; set; }

            /// <value>
            /// Whether the Gateway validated this as a "valid" message, i.e. anyone reading the data can use
            /// it as a valid position. It is recommended that if valid is false, that the message is either
            /// discarded, or stored but not used in any calculations/processing.
            /// </value>
            public bool valid { get; set; }
        }

        public class EventMessage
        {
            /// <value>The JSON message structure version, currently there is only a version 1 message.</value>
            public int message_ver { get; set; }

            /// <value>Message type, can be "event".</value>
            public string message_type { get; set; }

            /// <value>
            /// Whether the Gateway validated this as a "valid" message, i.e. anyone reading the data can use
            /// it as a valid event. It is recommended that if valid is false, that the message is either
            /// discarded, or stored but not used in any calculations/processing.
            /// </value>
            public bool valid { get; set; }

            /// <value>The ISO 8601 timestamp of the event</value>
            public DateTime timestamp { get; set; }

            /// <value>The domain which received the message from the unit</value>
            public string gateway { get; set; }

            /// <value>The event code</value>
            public string code { get; set; }

            /// <value>The event message/text</value>
            public string message { get; set; }

            /// <value>The details of the origin/creator of the event</value>
            public EventSource source { get; set; }

            /// <value>The location details of the event</value>
            public EventLocation location { get; set; }

            /// <value>The TCP/UDP port that unit connected to the gateway</value>
            public int port { get; set; }

            /// <value>Transmission type: tcp, udp, http, https, sms</value>
            public string transmission { get; set; }

            /// <value>The importance of the event, can by: high, medium or lo.</value>
            public string importance { get; set; }

            /// <value>A value from 0 to 10 that can be used to "score" overall critical status of the source</value>
            public int alert_level { get; set; }

            /// <value>An HTML color for the event, e.g. #FF8000</value>
            public string color { get; set; }

            /// <value>Some events may have a "start" and "end" state, most are standalone and will hence be empty</value>
            public string state { get; set; }

            /// <value>Whether this event should generate a ticket on a remote system or not</value>
            public bool ticket { get; set; }

            /// <value>The details of the device that generated the event</value>
            public EventDevice device { get; set; }

            /// <value>Array of additional data fields</value>
            public IList<EventData> data { get; set; }

            /// <value>Array of event pools where the event will be stored, internal use only</value>
            public IList<string> pools { get; set; }
        }

        public class EventSource
        {
            /// <value>A fieldname/key for the source, e.g. "veh_reg"</value>
            public string key { get; set; }

            /// <value>A label for the source, e.g. "Vehicle"</value>
            public string label { get; set; }

            /// <value>The actual source, e.g. a vehicle registration no. "ABC 123 GP"</value>
            public string value { get; set; }

            /// <value>An optional url that referse back to the source, such as an administrative website</value>
            public string url { get; set; }
        }

        public class EventLocation
        {
            /// <value>Latitude in decimal degrees (-90.0 to +90.0)</value>
            public float latitude { get; set; }

            /// <value>Longitude in decimal degrees (-180.0 to +180.0)</value>
            public float longitude { get; set; }

            /// <value>The address where the event took place (if applicable and available)</value>
            public string address { get; set; }
        }

        public class EventDevice
        {
            /// <value>Either "imei" or "code"</value>
            public string identifier { get; set; }

            /// <value>The device's IMEI (if identifier field is "imei")</value>
            public string imei { get; set; }

            /// <value>The device's serial no or identification code (if identifier field is "code")</value>
            public string serial_no { get; set; }

            /// <value>The device's firmware version (e.g. "1.04")</value>
            public string firm_ver { get; set; }

            /// <value>The device type, e.g. "teltonika"</value>
            public string type { get; set; }

            /// <value>The device model, only available on some units</value>
            public string model { get; set; }

            /// <value>A optional url that links back to where the device can be accessed</value>
            public string url { get; set; }
        }

        public class EventData
        {
            /// <value>A name/key for the data field</value>
            public string key { get; set; }

            /// <value>A short label for the data field</value>
            public string label { get; set; }

            /// <value>The actual value of the data field</value>
            public dynamic value { get; set; }

            /// <value>An optional URL that links back to the data</value>
            public string url { get; set; }
        }
    }
}
