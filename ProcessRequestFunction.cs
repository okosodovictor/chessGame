using System;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Company.Function
{
    public class ProcessRequestFunction
    {
        private readonly ILogger<ProcessRequestFunction> _logger;
        private static readonly string EndpointUri = "";
        private static readonly string PrimaryKey = "";
        private static readonly string DatabaseId = "ChessGameDB";
        private static readonly string RequestContainerId = "KnightMoveRequests";
        private static readonly string ResultContainerId = "KnightMoveResults";

        public ProcessRequestFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ProcessRequestFunction>();
        }

        [Function("ProcessRequestFunction")]
        public async Task Run([CosmosDBTrigger(
            databaseName: "ChessGameDB",
            containerName: "KnightMoveRequests",
            Connection = "chessdb_DOCUMENTDB",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true)] IReadOnlyList<KnightMoveRequest> requests)
        {
            if (requests != null && requests.Count > 0)
            {
                _logger.LogInformation("Documents modified: " + requests.Count);
                _logger.LogInformation("First document Id: " + requests[0].Id);

                var cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
                var database = cosmosClient.GetDatabase(DatabaseId);
                var resultContainer = database.GetContainer(ResultContainerId);
                var requestContainer = database.GetContainer(RequestContainerId);

                foreach (var request in requests)
                {
                    _logger.LogInformation($"Processing knight move request: {request.RequestId}");

                    // Calculate the shortest path for the knight
                    var shortestPath = CalculateShortestPath(request.StartPosition, request.EndPosition);

                    var result = new KnightMoveResult
                    {
                        Id = request.RequestId,
                        RequestId = request.RequestId,
                        ShortestPath = shortestPath,
                        NumberOfMoves = shortestPath.Count - 1,
                        StartPosition = request.StartPosition,
                        EndPosition = request.EndPosition
                    };

                    // Save the result
                    await resultContainer.CreateItemAsync(result, new PartitionKey(result.RequestId));

                    // Update the status of the request
                    request.Status = "Completed";
                    request.UpdatedAt = DateTime.UtcNow;
                    await requestContainer.ReplaceItemAsync(request, request.RequestId);
                }

            }
        }

        public static List<string> CalculateShortestPath(string startPosition, string endPosition)
        {
            // Validate start and end positions
            if (!IsValidChessPosition(startPosition) || !IsValidChessPosition(endPosition))
            {
                throw new ArgumentException("Invalid start or end position.");
            }

            // Convert positions to coordinates
            var startCoord = ConvertToCoordinates(startPosition);
            var endCoord = ConvertToCoordinates(endPosition);

            var queue = new Queue<(int x, int y)>();
            queue.Enqueue(startCoord);

            var visited = new HashSet<(int x, int y)>();
            visited.Add(startCoord);

            var parent = new Dictionary<(int x, int y), (int x, int y)>();

            // Perform BFS
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == endCoord)
                {
                    // Reconstruct the path
                    return ReconstructPath(parent, startCoord, endCoord);
                }

                // Generate next moves
                var moves = GenerateMoves(current);

                foreach (var move in moves)
                {
                    if (!visited.Contains(move))
                    {
                        visited.Add(move);
                        parent[move] = current;
                        queue.Enqueue(move);
                    }
                }
            }

            return new List<string>();
        }

        private static List<string> ReconstructPath(Dictionary<(int x, int y), (int x, int y)> parent, (int x, int y) start, (int x, int y) end)
        {
            var path = new List<string>();
            var current = end;

            while (current != start)
            {
                path.Add(ConvertToPosition(current));
                current = parent[current];
            }

            path.Add(ConvertToPosition(start));
            path.Reverse();
            return path;
        }

        private static List<(int x, int y)> GenerateMoves((int x, int y) position)
        {
            // Generate all possible moves for the knight
            var moves = new List<(int x, int y)>();
            int[] dx = { -2, -1, 1, 2, 2, 1, -1, -2 };
            int[] dy = { 1, 2, 2, 1, -1, -2, -2, -1 };

            for (int i = 0; i < dx.Length; i++)
            {
                int nx = position.x + dx[i];
                int ny = position.y + dy[i];

                if (IsValidMove(nx, ny))
                {
                    moves.Add((nx, ny));
                }
            }

            return moves;
        }

        private static bool IsValidMove(int x, int y)
        {
            return x >= 0 && x < 8 && y >= 0 && y < 8;
        }

        private static (int x, int y) ConvertToCoordinates(string position)
        {
            int x = position[0] - 'A';
            int y = position[1] - '1';
            return (x, y);
        }

        private static string ConvertToPosition((int x, int y) coordinates)
        {
            char file = (char)('A' + coordinates.x);
            char rank = (char)('1' + coordinates.y);
            return $"{file}{rank}";
        }

        private static bool IsValidChessPosition(string position)
        {
            if (position.Length != 2)
            {
                return false;
            }
            char file = position[0];
            char rank = position[1];
            return file >= 'A' && file <= 'H' && rank >= '1' && rank <= '8';
        }
    }
}