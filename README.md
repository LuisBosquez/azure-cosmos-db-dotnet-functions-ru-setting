# Update Cosmos DB container offer using Azure functions
Sample Azure Functions to maximize or minimize RUs depending on hour of the day. There are also sample functions to increase or decrease using `DocumentClient` calls.

## How to use
Replace the placeholder values with your own credentials:
```csharp
static string CONNECTION_STRING = "YOUR_FULL_CONNECTION_STRING_HERE";
static string DATABASE_NAME = "YOUR_DATABASE_NAME";
static string CONTAINER_NAME = "YOUR_CONTAINER_OR_COLLECTION_NAME_HERE";
```
Set the maximum and minimum request units for your container:
```csharp
int MAX_THROUGHPUT_CAPACITY = 10000;
int MIN_THROUGHPUT_CAPACITY = 400;
```
Set the hours for the maximum and minimum capacity:
```csharp
static int MAX_CAPACITY_START_HOUR = 9;
static int MIN_CAPACITY_START_HOUR = 18;
```
On line 22, you can change the frequency in which this function runs. By default, this is running every 5 minutes:
```csharp
public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
```

