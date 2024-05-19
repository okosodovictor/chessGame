using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Company.Function
{
    public class CreateRequestFunction
    {
        private readonly ILogger<CreateRequestFunction> _logger;
        private static readonly string EndpointUri = "";
        private static readonly string PrimaryKey = "";

        private static readonly string DatabaseId = "ChessGameDB";
        private static readonly string ContainerId = "KnightMoveRequests";

        private static CosmosClient cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
        private static Database database = cosmosClient.GetDatabase(DatabaseId);
        private static Container container = database.GetContainer(ContainerId);

        public CreateRequestFunction(ILogger<CreateRequestFunction> logger)
        {
            _logger = logger;
        }

        [Function("CreateRequestFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "knightpath")] HttpRequest req)
        {
            _logger.LogInformation("Processing a KnightMoveRequest.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            KnightMoveRequest data = JsonConvert.DeserializeObject<KnightMoveRequest>(requestBody) ?? throw new InvalidOperationException("Deserialization resulted in null");

            if (string.IsNullOrEmpty(data.StartPosition) || string.IsNullOrEmpty(data.EndPosition))
            {
                return new BadRequestObjectResult("Please pass a valid startPosition and endPosition in the request body.");
            }

            data.RequestId = Guid.NewGuid().ToString();
            data.Id = data.RequestId;
            data.Status = "Pending";
            data.CreatedAt = DateTime.UtcNow;
            data.UpdatedAt = DateTime.UtcNow;

            try
            {
                ItemResponse<KnightMoveRequest> response = await container.CreateItemAsync(data, new PartitionKey(data.RequestId));
                return new OkObjectResult(new { trackingId = data.RequestId });
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Cosmos DB error: {ex.Message}");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
