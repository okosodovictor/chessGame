# Knight Path Technical Take Home
## Knight Move Calculator

This project is an Azure Functions-based solution to calculate the shortest path for a knight's move in chess from a start position to an end position. It consists of three serverless functions:
1. **CreateKnightMoveRequest**: Accepts the initial request and stores it in Azure Cosmos DB.
2. **ProcessRequestFunction**: Triggers on the creation of a request, processes it to find the shortest path, and stores the result in a different Cosmos DB container.
3. **GetResultFunction**: Allows users to retrieve the result of a processed request by providing the tracking ID.

## Project Structure

KnightMoveFunctionApp
│
├── CreateKnightMoveRequest.cs
├── ProcessRequestFunction.cs
├── GetResultFunction.cs
├── KnightMoveCalculator.cs
├── local.settings.json
├── README.me


### Creating a Request

Send a POST request to 'https://knightpathfunction.azurewebsites.net/api/knightpath' with the following Curl:

curl --location 'https://knightpathfunction.azurewebsites.net/api/knightpath' \
--header 'Content-Type: text/plain' \
--data '{
  "startPosition": "A1",
  "endPosition": "D5"
}

'

### Getting the Result

Send a GET request to https://<your-functionapp-name>.azurewebsites.net/api/knightpath?requestId=<trackingId> to retrieve the result of the request.

curl --location 'https://knightpathfunction.azurewebsites.net/api/knightpath?requestId=48cd79c7-661c-4789-8be8-a1bb6506d1a9'



### Key Design Choices

Azure Functions: Chosen for their scalability and ease of use for serverless applications.

Cosmos DB: Used for its global distribution, scalability, and ease of integration with Azure Functions.

Breadth-First Search (BFS): Utilized for calculating the shortest path for the knight's move, ensuring optimal performance for this specific problem.


### Key Design Choices and Implementation

Azure Functions
The project leverages Azure Functions to create a serverless architecture that handles three primary operations: creating a knight move request, processing the request to determine the shortest path for a knight's move in chess, and returning the results of the processed request. Azure Functions were chosen due to their scalability, ease of deployment, and seamless integration with Azure services such as Cosmos DB.

Creating a Knight Move Request:

The first function, CreateKnightMoveRequest, is triggered by an HTTP POST request. It accepts the starting and ending positions of the knight as input, generates a unique tracking ID for the request, and stores the request in a Cosmos DB container (KnightMoveRequests).
Key considerations here include validation of input data, handling edge cases (such as missing or malformed inputs), and ensuring the request is correctly stored with a pending status.
Processing the Knight Move Request:

The second function, ProcessRequestFunction, is triggered by changes in the Cosmos DB container via the CosmosDBTrigger. This function listens for new requests, calculates the shortest path for the knight's move using a Breadth-First Search (BFS) algorithm, and updates the request status to "completed."
This function also stores the result, including the shortest path and the number of moves, in another Cosmos DB container (KnightMoveResults). Using BFS ensures that the shortest path is always found efficiently, even for complex chessboard configurations.
Returning the Results:

The third function, GetKnightMoveResult, is triggered by an HTTP GET request. It retrieves the result of the knight move calculation based on the provided tracking ID (operationId). The function ensures that the result is retrieved efficiently and handles cases where the result may not be found.
The route configuration allows for intuitive API design, making it easy for users to fetch results by appending the tracking ID as a query parameter.
Design and Implementation Notes
Cosmos DB Integration:

Cosmos DB was chosen for its scalability, low-latency data access, and seamless integration with Azure Functions. Two containers are used: one for storing the initial requests (KnightMoveRequests) and another for storing the results (KnightMoveResults).
Partition keys (requestId) ensure efficient querying and data organization. Unique keys prevent duplication and maintain data integrity.
Error Handling and Logging:

Comprehensive error handling ensures that users receive meaningful responses in case of errors, such as invalid inputs or missing results. Logging is implemented at critical points to facilitate debugging and monitoring.
Breadth-First Search Algorithm:

BFS was chosen for its suitability in finding the shortest path in an unweighted grid, such as a chessboard. This ensures optimal performance and correctness in determining the knight's path.
The algorithm accounts for all possible moves of the knight and ensures the shortest path is calculated efficiently.


### Think about ways you can leverage the stored data to optimize your solution.

Caching Frequently Accessed Data
Result Caching:

Strategy: Implement an in-memory cache (such as Azure Cache for Redis) to store frequently accessed knight move results.
Benefit: Reduces the number of read operations on Cosmos DB, leading to faster response times for common queries.

Query Optimization
Indexing:

Strategy: Ensure that the RequestId property is indexed in Cosmos DB.
Benefit: Significantly improves query performance when retrieving results based on the RequestId.
Partitioning Strategy:

Strategy: Use a partition key that optimizes data distribution and access patterns. For instance, using RequestId as the partition key if queries are mostly based on this field.
Benefit: Enhances read and write efficiency by distributing data evenly across partitions.
