:::writing{variant="document" id="63182"}
# QMS Companion API

An open-source developer API for accessing QMS blockchain data.

Built with **ASP.NET Core 8** and **C#**.

## Features

- QMS network information
- Latest block information
- Wallet balance lookup
- Transaction lookup by hash
- Transaction receipt lookup

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/v1/network` | Get network information |
| GET | `/api/v1/network/block/latest` | Get the latest block |
| GET | `/api/v1/network/wallet/{address}/balance` | Get wallet balance |
| GET | `/api/v1/transactions/{hash}` | Get transaction details |
| GET | `/api/v1/transactions/{hash}/receipt` | Get transaction receipt |

## Technology Stack

- C#
- ASP.NET Core 8
- Swagger / OpenAPI
- JSON-RPC
- QMS Testnet

## Getting Started

### Requirements

- .NET 8 SDK
- Visual Studio 2022 or compatible IDE

### Run the project

1. Clone the repository.
2. Open `QMS.Companion.Api.sln` in Visual Studio.
3. Restore NuGet packages.
4. Run the application.
5. Open the Swagger UI in your browser.

## Network

The project currently connects to the QMS Testnet RPC.

RPC endpoint:

`https://rpc.testnet.qms.finance`

## Project Status

Currently under development.

More features, documentation, and improvements are planned.

## License

To be determined.
:::