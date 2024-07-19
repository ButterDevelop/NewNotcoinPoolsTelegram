using Serilog;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NewNotcoinPoolsTelegram
{
    public class TelegramBot
    {
        private readonly ITelegramBotClient botClient;
        private readonly long[] adminIds = [386659032, 785861432, 1388367582];
        private const long maximTGIdIndex = 0;

        public TelegramBot(string token)
        {
            botClient = new TelegramBotClient(token);
        }

        public void Start()
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = [UpdateType.Message]
            };
            botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken: CancellationToken.None);
        }

        public async Task SendMessageToMaxim(string message)
        {
            await botClient.SendTextMessageAsync(adminIds[maximTGIdIndex], message, parseMode: ParseMode.Html);
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message != null)
            {
                if (update.Type == UpdateType.Message && update.Message.Type == MessageType.Text)
                {
                    var message = update.Message;
                    if (message.From is null) return;

                    if (adminIds.Contains(message.Chat.Id))
                    {
                        await HandleAdminMessage(message);
                    }
                }
            }
        }

        private async Task HandleAdminMessage(Message message)
        {
            if (message.Text is null || message.From is null) return;

            var parts   = message.Text.Split(' ');
            var command = parts[0].ToLower();

            Log.Information($"Command called by {message.From.Username}: {message.Text}");

            switch (command)
            {
                case "/start":
                    await botClient.SendTextMessageAsync(message.Chat, $"Hi! Your Telegram ID is <code>{message.Chat.Id}</code>", parseMode: ParseMode.Html);
                    break;
                case "/authwebappdata":
                    if (parts.Length == 2)
                    {
                        Program.SaveWebAppDataFromTelegram(parts[1]);
                        await botClient.SendTextMessageAsync(message.Chat, $"WebAppData was changed successfully!", parseMode: ParseMode.Html);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat, $"Usage: /authwebappdata <webAppData>\n" +
                                                                           $"Or: /authwebappdata <queryTGAuthString>");
                    }
                    break;
                case "/restartapp":
                    await botClient.SendTextMessageAsync(message.Chat, $"Restarting app.", parseMode: ParseMode.Html);
                    new Thread(() =>
                    {
                        Thread.Sleep(10000);
                        Environment.Exit(0);
                    }).Start();
                    break;
                default:
                    await botClient.SendTextMessageAsync(message.Chat, $"Unknown command.", parseMode: ParseMode.Html);
                    break;
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Log.Error($"Telegram Bot HandleErrorAsync: {exception}");
            return Task.CompletedTask;
        }
    }
}
