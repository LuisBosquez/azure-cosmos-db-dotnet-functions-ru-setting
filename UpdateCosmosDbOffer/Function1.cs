using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Net;

namespace UpdateCosmosDbOffer
{
    public static class Function1
    {
        // Default values. Use your own.
        static int MAX_THROUGHPUT_CAPACITY = 10000;
        static int MIN_THROUGHPUT_CAPACITY = 400;
        static int MAX_CAPACITY_START_HOUR = 9;
        static int MIN_CAPACITY_START_HOUR = 18;

        static string CONNECTION_STRING = "YOUR_FULL_CONNECTION_STRING_HERE";
        static string DATABASE_NAME = "YOUR_DATABASE_NAME";
        static string CONTAINER_NAME = "YOUR_CONTAINER_OR_COLLECTION_NAME_HERE";

        [FunctionName("Function1")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            log.LogInformation($"Current hour: {DateTime.Now.Hour}");

            CosmosClient cosmosClient = new CosmosClient(CONNECTION_STRING);

            // Peak hour RU maximization    
            if (DateTime.Now.Hour == MAX_CAPACITY_START_HOUR)
            {
                log.LogInformation($"Maximizing capacity for database: {0} and container: {1}.", DATABASE_NAME, CONTAINER_NAME);
                if (MaximizeContainerOffer(cosmosClient, DATABASE_NAME, CONTAINER_NAME))
                {
                    log.LogInformation($"Successfully set throughput to {3} RU's for database: {0} and container: {1}.", DATABASE_NAME, CONTAINER_NAME, MAX_THROUGHPUT_CAPACITY);
                }
            }

            // Slow hour minimization
            if (DateTime.Now.Hour == MIN_CAPACITY_START_HOUR)
            {
                log.LogInformation($"Minimizing capacity for database: {0} and container: {1}.", DATABASE_NAME, CONTAINER_NAME);
                if(MinimizeContainerOffer(cosmosClient, DATABASE_NAME, CONTAINER_NAME))
                {
                    log.LogInformation($"Successfully set throughput to {3} RU's for database: {0} and container: {1}.", DATABASE_NAME, CONTAINER_NAME, MIN_THROUGHPUT_CAPACITY);
                }
            }

            log.LogInformation("Current throughput {0}", GetContainerOffer(cosmosClient, DATABASE_NAME, CONTAINER_NAME));
        }

        public static int GetContainerOffer(CosmosClient client, string databaseName, string containerName)
        {
            Container container = client.GetContainer(databaseName, containerName);

            return (int) container.ReadThroughputAsync().Result;
        }

        public static bool UpdateContainerOffer(CosmosClient client, string databaseName, string containerName, int updatedContainerOffer)
        {
            Container container = client.GetContainer(databaseName, containerName);
            HttpStatusCode resultCode;

            try
            {
                resultCode = container.ReplaceThroughputAsync(updatedContainerOffer).Result.StatusCode;
            } catch(CosmosException ce)
            {
                Console.WriteLine(ce.Message);
                return false;
            }

            if (resultCode.Equals(HttpStatusCode.OK))
            {
                Console.WriteLine((int) container.ReadThroughputAsync().Result);
                return true;
            }

            return false;
        }

        public static bool IncreaseContainerOffer(CosmosClient client, string databaseName, string containerName, int increase)
        {
            Container container = client.GetContainer(databaseName, containerName);

            int currentOffer = (int)container.ReadThroughputAsync().Result;

            return UpdateContainerOffer(client, databaseName, containerName, currentOffer + increase);
        }

        public static bool MaximizeContainerOffer(CosmosClient client, string databaseName, string containerName)
        {
            Container container = client.GetContainer(databaseName, containerName);

            int currentOffer = (int)container.ReadThroughputAsync().Result;

            return UpdateContainerOffer(client, databaseName, containerName, MAX_THROUGHPUT_CAPACITY);
        }

        public static bool MinimizeContainerOffer(CosmosClient client, string databaseName, string containerName)
        {
            Container container = client.GetContainer(databaseName, containerName);

            int currentOffer = (int)container.ReadThroughputAsync().Result;

            return UpdateContainerOffer(client, databaseName, containerName, MIN_THROUGHPUT_CAPACITY);
        }
    }
}
