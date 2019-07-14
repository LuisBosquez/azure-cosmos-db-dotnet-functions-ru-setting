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
        
        [FunctionName("Function1")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            string ConnectionString = "YOUR_FULL_CONNECTION_STRING_HERE";
            string DatabaseName = "YOUR_DATABASE_NAME";
            string ContainerName = "YOUR_CONTAINER_OR_COLLECTION_NAME_HERE";

            // Default values. Use your own.
            int MAX_THROUGHPUT_CAPACITY = 10000;
            int MIN_THROUGHPUT_CAPACITY = 400;

            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            log.LogInformation($"Current hour: {DateTime.Now.Hour}");

            CosmosClient cosmosClient = new CosmosClient(ConnectionString);

            // Peak hour RU maximization    
            if (DateTime.Now.Hour.ToString() == "9")
            {
                log.LogInformation($"Maximizing capacity for database: {0} and container: {1}.", DatabaseName, ContainerName);
                if (MaximizeContainerOffer(cosmosClient, DatabaseName, ContainerName))
                {
                    log.LogInformation($"Successfully set throughput to {3} RU's for database: {0} and container: {1}.", DatabaseName, ContainerName, MAX_THROUGHPUT_CAPACITY);
                }
            }

            // Slow hour minimization
            if (DateTime.Now.Hour.ToString() == "18")
            {
                log.LogInformation($"Minimizing capacity for database: {0} and container: {1}.", DatabaseName, ContainerName);
                if(MinimizeContainerOffer(cosmosClient, DatabaseName, ContainerName))
                {
                    log.LogInformation($"Successfully set throughput to {3} RU's for database: {0} and container: {1}.", DatabaseName, ContainerName, MIN_THROUGHPUT_CAPACITY);
                }
            }

            log.LogInformation("Current throughput {0}", GetContainerOffer(cosmosClient, DatabaseName, ContainerName));
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
