# C# Event Receiver Example

Welcome to the Event Receiver example project for C# and .NET. This 
is an example project which allows you to receive and decode Event 
messages from our ACMGroup Live Event Processing Services (LEPS).

We use a popular opensource message queuing/brokering system called
[RabbitMQ](https://www.rabbitmq.com) for our own internal use and 
to push messages to 3'rd party clients. This project connects to
a RabbitMQ server and will consume event messages received from it.

# Setup Instructions

This project have been designed and tested in Microsoft Visual Studio 
Community 2017: v15.9.6 and the Microsoft .NET Framework: v4.7.03056 
(projected targeted at 4.6.1)

## Cloning the Repository

You will require a git client to clone the project to your development
environment. On Linux/Mac you can use the standard git client provided with
your distribution or on Windows you can use a console based client such as 
[get-scm](https://git-scm.com) or the 
[Github extension for Visual Studio](https://visualstudio.github.com).

Clone the project into a directory of your choice using:

`git clone https://github.com/acmgroup/EventReceiverExample.git`

or when using the Visual Studion Github plugin go to the `Team Explorer`
Tab and click on `Clone` under the Github connection and then paste
https://github.com/acmgroup/EventReceiverExample.git into the address bar.

Once you have cloned and opened the project in Visual Studion you can 
start installing the required NuGet packages, see the section below.

## Installing NuGet Packages

The project requires two NuGet packages to be installed:
https://www.nuget.org/packages/RabbitMQ.Client (v12.0.1)
https://www.nuget.org/packages/Newtonsoft.Json (v5.1.0)

To install them, navigate to `Project -> Manage NuGet Packages...` and
select the `Browse` tab. Then search for `RabbitMQ.Client` and click on
the `[Install]` button. Do the same for `Newtonsoft.Json`.

Alternatively you can navigate to 
`Tools -> NuGet Package Manager -> Package Manager Console` and enter
the following two commands:

```
Install-Package RabbitMQ.Client -ProjectName EventReceiverExample
Install-Package Newtonsoft.Json -ProjectName EventReceiverExample
```

Once the packages have been successfully installed you can start configuring
the application, see the section below:

## Project Settings

If you have been in contact with us, we would have supplied you with the credentials
to configure the project in order to connect to our services and to consume
messages from it.

To fill in these settings, navigate to: 
`Project -> EventReceiverExample Properties...` and select the `Settings` tab.

Change `ServerHostname`, `ServerUsername`, `ServerPassword` and any other
properties we may have supplied you with.

Once done, press `Ctrl+S` to save the settings.

## Running the Project

The project is a console based application and hence does not have a GUI. When you 
run the project (by clicking on Start in the toolbar) it should connect to the
server and setup everything you need in order to start receiving messages. Look at
the output of the console to see if there are any errors.

The project will wait for incomming JSON based Event messages, when received the Event
message will be decoded into various classes, the results will be printed and an
Acknowledgement will be sent back to the server.

See the details below for the type of messages you may receive. In this case all
other message types are ignored except for `event` message types.

# Event JSON Message Structure v1

The Event JSON Message Structure is a standard structure we have designed to transmit 
event related data between our own internal services and 3'rd party services.

**Event Example 1: GPS Related Event**

```json
{
  "message_ver": 1,
  "message_type": "event",
  "valid": true,
  "timestamp": "2018-08-15T06:37:23+00:00",
  "gateway": "gateway.site.net",
  "code": "PANIC",
  "message": "Panic button pressed by driver John Doe in vehicle ABC 123 GP",
  "source": {
    "key": "vehicle_reg",
    "label": "Vehicle Reg",
    "value": "ABC 123 GP",
    "url": null
  },
  "location": {
    "latitude": -27.343254,
    "longitude": 34.234565,
    "address": "15 Main Road, Jamestown, Extension 3"
  },
  "importance": "high",
  "alert_level": 10,
  "color": "#FF0000",
  "sound_id": "bell",
  "state": null,
  "ticket": true,
  "device": {
    "identifier": "imei",
    "imei": "3512345456788",
    "serial_no": "123456",
    "cell_no": "+2782123456",
    "firm_ver": null,
    "type": "calamp",
    "model": "lmu1100",
    "url": null
  },
  "data": [
    { "key": "imei", "label": "IMEI", "value": "350123456789", "url": "http://www.somesite.net/#!/units/4564" },
    { "key": "driver", "label": "Driver", "value": "Piet Poggenpoel", "url": "http://www.somesite.net/#!/drivers/4535" },
    { "key": "x", "label": "Accelerometer X Axis",  "value": -0.3432122 },
    { "key": "y", "label": "Accelerometer Y Axis", "value": 0.0034534 }
  ],
  "pools": [
    "acm.alarms",
    "abctrucking"
  ]
}
```

**Event Example 2: Alarm Panel Related Event**

```json
{
  "message_ver": 1,
  "message_type": "event",
  "valid": true,
  "timestamp": "2018-08-15T06:37:23.345+00:00",
  "gateway": "gateway.site.net",
  "code": "ZONE:1:ALARM",
  "message": "Alarm triggered in zone 1 at 15 Main Road, Jamestown, Extension 3",
  "source": {
    "key": "cell_no",
    "label": "Cell No.",
    "value": "+2755512345",
    "url": null
  },
  "location": {
    "latitude": null,
    "longitude":null,
    "address": "15 Main Road, Jamestown, Extension 3"
  },
  "importance": "high",
  "alert_level": 5,
  "color": "#FF0000",
  "state": null,
  "ticket": true,
  "device": {
    "identifier": "cell_no",
    "imei": null,
    "serial_no": null,
    "cell_no": "+2782123456",
    "type": "Eagle",
    "model": "V6",
    "url": null
  },
  "data": [
    { "key": "zone", "label": "Zone No",  "value": 1 },
    { "key": "address", "label": "Address",  "value": "15 Main Road, Jamestown, Extension 3" }
  ],
  "pools": [
    "acm.alarms",
    "acm.technical",
    "abctrucking"
  ]
}
```

## Event Field Descriptions

`message_ver`: A version number indicating the event's structure.

`message_type`: Message type will always be `event`.

`valid`: This indicates that a gateway parsed the message and saw it as valid (true).

`timestamp`: An ISO 8601 date/time of when the event occurred at the source, e.g.
a vehicle tracking unit detected a harsh braking event or an alarm panel
detected a Panic button press.

`gateway`: Which gateway processed or generated the event.

`code`: A short string that identifies the event.

`message`: A string that provides more details about the event. This is not
required and can also be `null`.

`source`: The various fields that identifies the source of the event.
    
* `key`: The source field identification key, e.g. vehicle_reg, cell_no, etc.
* `label`: The source field label, e.g. "Vehicle Reg.", "Cell no.", etc.
* `value`: The source identification value, e.g. "ABC 123 GP", "+27821234567"
* `url`: An optional url that points back to the original source.

`location`: An object that identifies the location of the event. If any of the
fields are not available it can be left `null`. `location` itself is not
required and can be set to `null`

`importance`: Can be "high", "medium", "low" or `null`

`alert_level`: A number from 0 to 10 that indicates by how much the importance
of the primary source of the message should be raised.

`color`: An event color in HTML format.

`state`: Can be one of the following: null, "start", "end". If set to start or
end it indicates the start or end of an event.

`ticket`: Can be true, false or null and indicates whether a ticket should be create
or not.

`device`: The details of the device that generated the event, this is separate
from the `source` field because the source could typically be a vehicle, where
the `device` fields indicates the details of the tracking device inside a vehicle.

* `identifier`: Can be imei, serial_no, cell_no
* `imei`: The device's imei no., can be `null`.
* `serial_no`: The device's serial no., can be `null`.
* `cell_no`: The device's cell no., can be `null`.
* `type`: The device type, can be `null`.
* `model`: The device model, can be `null`.
* `url`: An optional url that points back to the device.

`data`: Additional data that can be displayed for the event. Each data
field may consist of the following fields:

* `key`: A field name/key identifying the data field.
* `label`: A display label for the data field.
* `value`: The value of the data field, can be a `string`, `int`, `float`, 
  `boolean` or `null`.