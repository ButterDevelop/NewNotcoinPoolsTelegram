namespace NewNotcoinPoolsTelegram
{
    public class DBModel
    {
        public string WebAppData { get; set; } = string.Empty;
        public List<Pool> Pools { get; set; } = new List<Pool>();
    }

    public class Calc
    {
        public string total { get; set; } = string.Empty;
        public string reserved { get; set; } = string.Empty;
        public string paid { get; set; } = string.Empty;
        public string available { get; set; } = string.Empty;
    }

    public class Data
    {
        public List<Pool> pools { get; set; } = [];
    }

    public class Identity
    {
        public Calc calc { get; set; } = new();
    }

    public class WebParseModel
    {
        public bool ok { get; set; }
        public Data data { get; set; } = new();
    }

    public class Pool
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string image { get; set; } = string.Empty;
        public string? reward { get; set; } = string.Empty;
        public int mined { get; set; }
        public int claimed { get; set; }
        public int challengeId { get; set; }
        public bool isJoined { get; set; }
        public bool isRisky { get; set; }
        public bool isActive { get; set; }
        public Identity identity { get; set; } = new();

        public string MarkdownV2()
        {
            string rewardString = reward ?? "0";

            double rewardDouble = double.Parse(rewardString);

            double bronze   = rewardDouble / 1000000000;
            double silver   = bronze * 10;
            double gold     = silver * 100;
            double platinum = gold * 5;

            return $"<b>Description:</b> " + (string.IsNullOrEmpty(description) ? "<b>no description</b>\n\n" : $"\n<i>{description}</i>\n\n") +
                   $"<b>Reward:</b> {reward} $NOT\n" +
                   $"<b>Is Risky:</b> " + (isRisky   ? "risky ❌" : "not risky ✅") + "\n" +
                   $"<b>Is Active:</b> " + (isActive ? "active ✅"   : "not active ❌") + "\n\n" + 
                   $"<b>Loot per hour ($NOT):</b>\n" +
                   $" 💎 Platinum: {platinum}\n" +
                   $" 🥇 Gold: {gold}\n" +
                   $" 🥈 Silver: {silver}\n" +
                   $" 🥉 Bronze: {bronze}";
        }
    }
}
