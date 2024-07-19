using NewNotcoinPoolsTelegram;
using Newtonsoft.Json;
using Serilog;
using System.Text.RegularExpressions;
using Telegram.Bot;

class Program
{
    private static Timer? _timer = null;

    private const string _telegramBotToken    = "7248246786:AAGNZk94aNHjdGbfSpG3VQKp6kVTFVVnIPo";
    private const string _telegramChatId      = "@NewNotcoinPools";
    private const string _dataUrl             = "https://clicker-api.joincommunity.xyz/pool/my",
                                   _webappSessionUrl    = "https://clicker-api.joincommunity.xyz/auth/webapp-session";
    private const string _originHeader        = "https://farm.joincommunity.xyz",
                         _refererHeader       = _originHeader;
    private const string _proxy               = "";


    private static readonly string MASK_DATE_LOG_FILE_PATH = "%DateTime%", LOG_FILE_PATH = $"logs/newNotcoinPoolsTG-{MASK_DATE_LOG_FILE_PATH}.log";
    internal static string GetLogFilePath()
    {
        return LOG_FILE_PATH.Replace(MASK_DATE_LOG_FILE_PATH, DateTime.Now.ToString("yyyyMMddHH"));
    }

    private static DBModel _dbModel = new DBModel();
    private static readonly string _dbFilePath = "db.json";

    private static string _authorizationHeader = "";
    private static string _refreshToken        = "";

    private readonly static TelegramBot adminTelegramBot = new(_telegramBotToken);

    public static string WebAppData = "{\"webAppData\":\"query_id=AAHY8gsXAAAAANjyCxc1WP2F&user=%7B%22id%22%3A386659032%2C%22first_name%22%3A%22%D0%9C%D0%B0%D0%BA%D1%81%D0%B8%D0%BC%22%2C%22last_name%22%3A%22%22%2C%22username%22%3A%22max_kt%22%2C%22language_code%22%3A%22ru%22%2C%22allows_write_to_pm%22%3Atrue%7D&auth_date=1720984346&hash=359d0b4d5704fe061764f4c6c31592633bfc64fd98ed24e99ba48650b8d2b2c9\"}";

    private static DateTime _lastMessageToMaximTimestamp = new(2024, 1, 1);

    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
                         .MinimumLevel.Debug()
                         .WriteTo.Console()
                         .WriteTo.File(LOG_FILE_PATH.Replace(MASK_DATE_LOG_FILE_PATH, ""), rollingInterval: RollingInterval.Hour)
                         .CreateLogger();

        HTTPController.Initialize(NewNotcoinPoolsTelegram.Properties.Resources.UserAgents);

        adminTelegramBot.Start();
        Log.Information("Started Telegram bot.");

        LoadDatabase();

        WebAppData = _dbModel.WebAppData;

        await DoReLoginAsync();

        _timer = new Timer(async _ => await CheckForUpdates(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

        Log.Information("Started. Press any key to exit...");
        
        while (true)
        {
            Log.Information("I'm working.");

            Thread.Sleep(10000);
        }
    }

    private static void LoadDatabase()
    {
        if (File.Exists(_dbFilePath))
        {
            var json = File.ReadAllText(_dbFilePath);
            _dbModel = JsonConvert.DeserializeObject<DBModel>(json) ?? new DBModel();
        }
        else
        {
            _dbModel = new DBModel();
        }
    }
    private static void SaveDatabase()
    {
        var json = JsonConvert.SerializeObject(_dbModel, Formatting.Indented);
        File.WriteAllText(_dbFilePath, json);
    }

    public static void SaveWebAppDataFromTelegram(string webAppData)
    {
        WebAppData          = webAppData;
        _dbModel.WebAppData = webAppData;
        SaveDatabase();
        _lastMessageToMaximTimestamp = new DateTime(2024, 1, 1);
    }

    private static async Task DoReLoginAsync()
    {
        _authorizationHeader = GetAccessAuthToken() ?? string.Empty;
        if (string.IsNullOrEmpty(_authorizationHeader))
        {
            if ((DateTime.Now - _lastMessageToMaximTimestamp).TotalSeconds > 3600) // At least 1 hour to avoid spam
            {
                await adminTelegramBot.SendMessageToMaxim("<b>The auth has expired!</b>");
                _lastMessageToMaximTimestamp = DateTime.Now;
            }

            Log.Error("New Authorization header is NULL after relogin! Sending message to Telegram.");
        }
        else
        {
            Log.Information($"New Authorization header after relogin: {_authorizationHeader}");
        }
    }

    private static async Task CheckForUpdates()
    {
        try
        {
            var headers = new Dictionary<string, string>
            {
                { "accept",             "application/json" },
                { "sec-ch-ua",          "\"Microsoft Edge\";v=\"125\", \"Chromium\";v=\"125\", \"Not.A/Brand\";v=\"24\", \"Microsoft Edge WebView2\";v=\"125\"" },
                { "sec-ch-ua-mobile",   "?0" },
                { "origin",             _originHeader },
                { "Authorization",      _authorizationHeader },
                { "sec-fetch-site",     "cross-site" },
                { "sec-fetch-mode",     "cors" },
                { "sec-fetch-dest",     "empty" }
            };

            string? json_answer = HTTPController.ExecuteFunctionUntilSuccess(() =>
                                     HTTPController.SendRequest(_dataUrl, RequestType.GET, _proxy, headers, null, _refererHeader)
                                 );

            if (json_answer == null) 
            {
                Log.Error("JsonAnswer is null! Trying to get new auth.");

                await DoReLoginAsync();

                return;
            }

            Log.Information($"CheckForUpdates Server Answer: {json_answer}");

            if (json_answer.Contains("InvalidAuthorizationException"))
            {
                Log.Warning("Detected expired auth. Doing relogin now.");

                await DoReLoginAsync();

                return;
            }

            var parsed   = JsonConvert.DeserializeObject<WebParseModel>(json_answer);
            if (parsed == null) return;

            var newPools = parsed.data;
            if (newPools == null) return;

            bool hasChanges = false;

            // Check for new active pools
            foreach (var newPool in newPools)
            {
                var existingPool = _dbModel.Pools.Find(p => p.id == newPool.id);

                if (existingPool == null)
                {
                    _dbModel.Pools.Add(newPool);
                    await SendMessageToTelegram("<b>New " + (newPool.isActive ? "ACTIVE" : "upcoming") + $" pool added:</b> {newPool.name}\n\n" + 
                                                $"{newPool.MarkdownV2()}\n\n" +
                                                (newPool.isActive ? $"<b>Easy enter to Notcoin Bot</b> 👉 @notcoin_bot\n\n" : "") +
                                                $"Stay with us and turn on notifications! {_telegramChatId}",
                                                isSilent: !newPool.isActive);
                    hasChanges = true;
                }
                else if (existingPool.isActive != newPool.isActive)
                {
                    existingPool.isActive = newPool.isActive;
                    if (newPool.isActive)
                    {
                        await SendMessageToTelegram($"Pool <b>{newPool.name}</b> is <b>now active ✅</b>\n\n" +
                                                    $"{newPool.MarkdownV2()}\n\n" +
                                                    $"<b>Easy enter to Notcoin Bot</b> 👉 @notcoin_bot\n\n" +
                                                    $"Stay with us and turn on notifications! {_telegramChatId}");
                    }
                    else
                    {
                        await SendMessageToTelegram($"Pool <b>{newPool.name}</b> is <b>no longer active ❌</b> \n\nStay with us and earn $NOT | {_telegramChatId}");
                    }
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                SaveDatabase();
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error: {ex.Message}");
        }
    }

    private static string? GetAccessAuthToken()
    {
        var headers = new Dictionary<string, string>()
        {
            { "Authorization", "Bearer " + _refreshToken }
        };

        string paramsString = $"{{\"webAppData\":\"{WebAppData}\"}}";

        string? json_answer = HTTPController.ExecuteFunctionUntilSuccess(() =>
                                     HTTPController.SendRequest(_webappSessionUrl, RequestType.POST, _proxy, headers, null, 
                                     WebAppData.Contains("\"webAppData\"") ? WebAppData : paramsString,
                                     "application/json", _refererHeader)
                                 );

        Log.Information($"Getting the auth info: {json_answer}");
        if (json_answer == null) return null;

        Regex regex = new Regex(",\"accessToken\":\"(.*)\",\"refreshToken\":\"(.*)\"");
        var match = regex.Match(json_answer);

        if (!match.Success) return null;

        _refreshToken = match.Groups[2].Value;

        return "Bearer " + match.Groups[1].Value;
    }

    private static async Task SendMessageToTelegram(string message, bool isSilent = false)
    {
        var url = $"https://api.telegram.org/bot{_telegramBotToken}/sendMessage";
        var content = new StringContent(JsonConvert.SerializeObject(new
        {
            chat_id              = _telegramChatId,
            text                 = message,
            parse_mode           = "HTML",
            disable_notification = isSilent
        }), System.Text.Encoding.UTF8, "application/json");

        /*WebProxy webProxy = new WebProxy(_proxy);
        HttpClientHandler httpClientHandler = new HttpClientHandler
        {
            Proxy = webProxy
        };*/

        HttpClient httpClient = new();
        var response = await httpClient.PostAsync(url, content);
        if (response.IsSuccessStatusCode)
        {
            Log.Information($"Message sent to Telegram. Message: \n\n{message}\n");
        }
        else
        {
            Log.Error($"Failed to send message. Status code: {response.StatusCode}. \n\nMessage: {message}\n");
        }
    }
}
