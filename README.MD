# LoggingServer

LoggingServer is a centralized logging service implemented in C# and .NET Core to manage your application logs effectively. It helps you to log, retrieve and monitor different types of logs conveniently.

## Features

1. Supports different types of log messages (Debug, Info, Warning, Error, Fatal).
2. The server provides API endpoints to fetch log files based on the Host and Date.
3. Ability to receive log messages from clients and store them into log files.

## API Endpoints

1. GET `/{host}` : Fetches a list of log files for a given `host`.
2. GET `/{host}/{type}` : Fetches logs of a specific type for a given `host`.
3. POST `/{host}/{type}` : Appends a new log message of a given `type` for a specified `host`.

## Change the Port

You can change the default port in the appsettings.json file.

```json
{
	"LoggingServer": {
		"Port": 5000
		// Change the port here
	}
}
```

## Client usage

A client-side JavaScript logging API is available. It logs different types of messages to the console, and also sends them to the LoggingServer.

Example usage:   
You can include the logger using our CDN link in your HTML file or using ES6 Module import.
Html:

```html

<script src="https://cdn.jsdelivr.net/gh/drew-chase/LoggingServer@{version}/LoggingServer/wwwroot/js/Logger.js"></script>
```

or ES6 Module:

```javascript
import Logger from 'https://cdn.jsdelivr.net/gh/drew-chase/LoggingServer@{version}/LoggingServer/wwwroot/js/Logger.js';
```

> ## Note:
> Make sure to replace `{version}` with the version of the logger that you are using.

```javascript
const logger = new Logger('https://your-loggingservice-host.com');
logger.info('This is an information message');
logger.error('An error occurred');
```

## How to Use

You can make HTTP requests to the provided endpoint including the host and type of log as parameters in the URL and the log message in the request body.

```http request
POST /your-host/INFO
Content-Type: application/json

{
  "message": "Your log message"
}
```

You will receive a confirmation response upon the successful logging of the message.

> ## Note:
> - The host in the context of this server refers to the hostname of the user of the logging server where the logs originate from. The date is optional in the queryParams. If the date is not provided, the current date will be used to search the logs.
> - Make sure to replace 'your-host' and 'INFO' with your host name and desired log type.


