# LocationGuesser

![Logo](images/logo_small.jpg)

Web-Game inspired by Geoguessr. Try to guess the exact location and year of a given image.

Built with:

- Vue 3 on the frontend
- C# and ASP.Net Core on the backend
- CosmosDB NoSQL for persistence
- Azure Blob Storage to store the images
- Azure App Service to host the REST API
- Azure Static Web App to host the frontend
- Github Actions for CI/CD

## Try it

The application is currently hosted on free resources in Azure, thus it will take a few seconds to start the App Service instance. Maybe you will have to reload the page:

[Try it here!](https://blue-glacier-0010b8203.3.azurestaticapps.net/#/)

## Architecture

![Cloud Architecture](images/architecture.png)

## Code Structure
- `frontend`: This directory contains the frontend project written in Vue 3 and using Vite
- `src`: This directory contains the backend API projects:
  - `LocationGuesser.Api`: The entry point for the API, containing the endpoints and setup
  - `LocationGuesser.Core`: Core project that contains the business logic and data access for CosmosDB and Blob Storage
  - `LocationGuesser.Core.Contracts`: All contracts, that the API uses for the REST calls
  - `LocationGuesser.Core.Domain`: Domain models

- `test`: Contains the unit tests for the application
- `util`: Contains a dotnet console application to upload a load of images and imagesets from a local directory to the CosmosDB and Blob Storage

## Helpful Resources

- Blob Storage: [Managed Identity for Blob Storage](https://learn.microsoft.com/de-de/azure/storage/blobs/storage-quickstart-blobs-dotnet?tabs=net-cli%2Cmanaged-identity%2Croles-azure-cli%2Csign-in-azure-cli%2Cidentity-visual-studio)
- Cosmos DB:
  - [Managed Identity for Cosmos DB](https://learn.microsoft.com/en-us/azure/cosmos-db/managed-identity-based-authentication)
  - [Define Managed Identity on VM](https://learn.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/tutorial-vm-managed-identities-cosmos?tabs=azure-cli)
