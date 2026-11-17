namespace QMS.Companion.Api.Models;

public class TransactionReceiptResponse
{
    public string TransactionHash { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string From { get; set; } = string.Empty;

    public string? To { get; set; }

    public string BlockNumber { get; set; } = string.Empty;

    public string GasUsed { get; set; } = string.Empty;

    public string BlockHash { get; set; } = string.Empty;

    public string? ContractAddress { get; set; }
}
