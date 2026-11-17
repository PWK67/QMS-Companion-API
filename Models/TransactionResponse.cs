namespace QMS.Companion.Api.Models;

public class TransactionResponse
{
    public string Hash { get; set; } = string.Empty;

    public string From { get; set; } = string.Empty;

    public string? To { get; set; }

    public string ValueQMS { get; set; } = "0";

    public string Gas { get; set; } = "0";

    public string? BlockNumber { get; set; }
}
