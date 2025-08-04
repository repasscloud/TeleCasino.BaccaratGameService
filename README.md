# TeleCasino Baccarat Game API

[![Build and Test BaccaratGameService](https://github.com/repasscloud/TeleCasino.BaccaratGameService/actions/workflows/test-baccarat-api.yml/badge.svg)](https://github.com/repasscloud/TeleCasino.BaccaratGameService/actions/workflows/test-baccarat-api.yml)
[![🚀 Publish TeleCasino.KenoGameService (linux-x64)](https://github.com/repasscloud/TeleCasino.BaccaratGameService/actions/workflows/docker-image.yml/badge.svg)](https://github.com/repasscloud/TeleCasino.BaccaratGameService/actions/workflows/docker-image.yml)
![GitHub tag (latest SemVer)](https://img.shields.io/github/v/tag/repasscloud/TeleCasino.BaccaratGameService?label=version)

A REST API service for simulating Baccarat games, generating results, and preparing video animation files.  
Players can place a wager ($1, $2, or $5) on **Player**, **Banker**, or **Tie** and receive a JSON summary of the result with a video file path.

## Features

- **Baccarat gameplay simulation**:
  - Standard Baccarat rules for Player and Banker hands.
  - Handles tie conditions.
  - Payout rules applied automatically based on bet type.
- **Video result file generation** (future enhancement):  
  Animated dealing of cards, revealing results, and highlighting winning hands.
- **JSON output**: Contains wager, bet type, winning hand, payout, net gain, and video file path.

## Installation

1. Ensure [.NET 9.0 SDK](https://dotnet.microsoft.com/download) is installed.
2. Clone or download this repository.
3. Add dependencies:

   ```bash
   dotnet add package Microsoft.AspNetCore.OpenApi
   ```

4. Place your card images (e.g., `ace_of_spades.svg`) into `images/` for video rendering (future step).

## Build & Run

```bash
dotnet clean
dotnet restore
dotnet build
dotnet run
```

The API will start at:

```sh
http://localhost:5000
```

Swagger UI will be available at:

```sh
http://localhost:5000/swagger
```

## Usage

### Endpoint: `/api/baccarat/play`

**Method:** `POST`  
**Query Parameters:**

- `wager` (int) — Must be **1, 2, or 5**.  
- `bet` (string) — `"player"`, `"banker"`, or `"tie"`.  
- `gameSessionId` (int) — Game session identifier.

### Example Request

```bash
curl -X POST "http://localhost:5000/api/baccarat/play?wager=5&bet=player&gameSessionId=123"      -H "Content-Type: application/json"
```

### Example Response

```json
{
  "GameId": "abc123",
  "Wager": 5,
  "Bet": "player",
  "WinningHand": "player",
  "Payout": 10,
  "NetGain": 5,
  "VideoFile": "abc123.mp4"
}
```

## Rules & Payouts

- **Bet options**:
  - **Player** — Pays 1:1  
  - **Banker** — Pays 0.95:1 (5% commission applied)  
  - **Tie** — Pays 8:1
- **Standard Baccarat draw rules** are followed for both Player and Banker hands.

## License

This project is released under the MIT License.
