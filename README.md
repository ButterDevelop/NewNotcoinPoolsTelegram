# NewNotcoinPoolsTelegram

**NewNotcoinPoolsTelegram** is a quick-and-dirty C# application that monitors the Notcoin mini-app in Telegram for new or disappearing pools. When changes are detected, the tool sends notifications to a designated Telegram channel. It is designed to help users stay up-to-date with pool activity in real time.

---

## Overview

This project continuously polls a remote API endpoint to retrieve pool data and compares it with a locally stored database (in JSON format). When a new pool is detected or an existing pool’s status changes (e.g., from inactive to active or vice versa), the application sends a formatted Telegram message to alert users. Key functionalities include:

- **Monitoring New Pools:** Detects and notifies when a new pool is added.
- **Monitoring Status Changes:** Alerts when a pool becomes active or is no longer active.
- **Database Persistence:** Stores the current state of pools in a local JSON file (`db.json`) for comparison on subsequent checks.
- **Automatic Re-Authentication:** Re-logs in automatically if the authorization token expires.
- **Logging:** Uses Serilog for robust logging of activity and errors.
- **HTTP Requests & Proxy Support:** Uses HTTP requests (with optional proxy support) to fetch data from the API.

---

## Features

- **Real-Time Monitoring:** Checks for pool updates every minute.
- **Telegram Notifications:** Sends rich formatted messages (using HTML) to a Telegram channel when pools are added or change status.
- **Automatic Re-Login:** Monitors authorization status and performs a re-login if needed.
- **Local Persistence:** Maintains a local database (`db.json`) to track known pools.
- **Error Handling & Logging:** Detailed logging via Serilog to both console and file for easy troubleshooting.
- **Configurable Headers & Proxy:** Supports custom HTTP headers and proxy settings for robust network requests.

---

## Technologies Used

- **.NET (C#)**
- **Telegram.Bot** – For interacting with the Telegram API.
- **Newtonsoft.Json** – For JSON serialization/deserialization.
- **Serilog** – For structured logging.
- **xNet** – For making HTTP requests with advanced proxy support.
- **System.Timers/Threading.Timer** – For scheduling periodic tasks.

---

## Getting Started

### Prerequisites

- [.NET 8 SDK (or later)](https://dotnet.microsoft.com/download)
- A valid Telegram bot token and a target Telegram chat/channel ID.
- Internet access to reach the API endpoints and Telegram servers.
- (Optional) Proxy configuration if required by your network.

### Running the Application

1. **Clone the Repository:**

   ```bash
   git clone https://github.com/ButterDevelop/NewNotcoinPoolsTelegram.git
   cd NewNotcoinPoolsTelegram
   ```

2. **Restore Dependencies and Build:**
   Open the project in Visual Studio or run the following commands:
   ```bash
   dotnet restore
   dotnet build
   ```

3. **Configure Settings:**
   The application currently contains hard-coded configuration values for the Telegram bot token, chat ID, and API URLs. To customize these settings, edit the constants at the top of `Program.cs`:
   - `_telegramBotToken`
   - `_telegramChatId`
   - `_dataUrl` (pool data endpoint)
   - `_webappSessionUrl` (for re-login)
   - `_originHeader` and `_refererHeader`
   - `_proxy` (if needed)

4. **Run the Application:**
   You can start the application from Visual Studio or via the command line:
   ```bash
   dotnet run
   ```
   The application will start the Telegram bot, load the local database, perform an initial re-login, and then begin checking for pool updates every minute.

## How It Works

- **Initialization**:
The application initializes the HTTP controller with a list of user agents, starts the Telegram bot, and loads a local JSON database (`db.json`) containing previous pool data.
- **Re-Authentication:**
A function checks and refreshes the authorization header if it is expired. If re-login fails, a Telegram message is sent to alert the administrator.
- **Periodic Checking:**
Using a timer, the application polls the _dataUrl endpoint every minute. The JSON response is parsed, and each pool is compared to the locally stored data. New pools or status changes trigger a notification message.
- **Database Update:**
When a new pool is detected or an existing pool changes status, the pool data is updated in the local database and saved back to `db.json`.
- **Telegram Bot Integration:**
The built-in Telegram bot listens for specific admin commands and sends notifications to the designated Telegram channel. Admin commands (such as reloading web app data or restarting the app) are handled via the bot.

## Project Structure

```bash
NewNotcoinPoolsTelegram/
├── Program.cs              # Main entry point of the application
├── HTTPController.cs       # Handles HTTP requests and proxy settings
├── TelegramBot.cs          # Telegram bot implementation and command handling
├── DBModel.cs              # Data models for persisting pool data and web app data
├── Notcoin Models          # Models (e.g., Pool, WebParseModel) for JSON deserialization
├── Properties/             # Project properties and resources
├── logs/                   # Log files generated by Serilog
├── db.json                 # Local JSON database for pool data persistence
└── (Other supporting files)
```

## Logging

The application uses Serilog to log messages to both the console and a rolling file. Log messages include timestamps and detailed information about operations and errors, aiding in monitoring and debugging.

## License

This project is licensed under the **MIT License.**

## Contributing

Since this project was written quickly as a proof-of-concept, contributions are welcome! Whether you have improvements, fixes, or new features to add, feel free to fork the repository and open a pull request.

## Contact

For any questions, suggestions, or feedback, please reach out to me via GitHub.

---

**Stay updated on the latest Notcoin pool changes and happy monitoring!**