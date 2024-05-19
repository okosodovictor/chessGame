using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.Function
{
    public class GetResultFunction
    {
        private readonly ILogger<GetResultFunction> _logger;
        private static readonly string EndpointUri = "";
        private static readonly string PrimaryKey = "";
        private static readonly string DatabaseId = "ChessGameDB";
        private static readonly string ResultContainerId = "KnightMoveResults";


        public GetResultFunction(ILogger<GetResultFunction> logger)
        {
            _logger = logger;
        }

        [Function("GetResultFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "knightpath")] HttpRequest req, string trackingId)
        {
            _logger.LogInformation("Processing request to retrieve knight move result.");

            // Extract the query parameter
            string requestId = req.Query["requestId"];

            if (string.IsNullOrEmpty(requestId))
            {
                _logger.LogWarning("Operation ID not provided in query string.");
                return new BadRequestObjectResult("Please provide a valid operationId in the query string.");
            }

            try
            {
                var cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
                var database = cosmosClient.GetDatabase(DatabaseId);
                var container = database.GetContainer(ResultContainerId);

                var query = new QueryDefinition("SELECT * FROM c WHERE c.RequestId = @RequestId")
                                .WithParameter("@RequestId", requestId);
                var iterator = container.GetItemQueryIterator<KnightMoveResult>(query);

                if (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    var knightMoveResult = response.FirstOrDefault();

                    if (knightMoveResult != null)
                    {
                        _logger.LogInformation($"Result found for operation ID: {requestId}");
                        return new OkObjectResult(knightMoveResult);
                    }
                }

                _logger.LogWarning($"No result found for operation ID: {requestId}");
                return new NotFoundObjectResult($"No result found for operation ID: {requestId}");
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Cosmos DB error: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Internal server error: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
