
namespace QMS.Companion.Api.Models
{
    public class LatestBlockResponse
    {
        public long BlockNumber { get; set; }

        public string? BlockHash { get; set; }

        public DateTime Timestamp { get; set; }

        public int TransactionCount { get; set; }

        public string? GasUsedHex { get; set; }
    }
}

