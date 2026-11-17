
using Microsoft.AspNetCore.Mvc;
using QMS.Companion.Api.Services;
using QMS.Companion.Api.Models;
using System.Numerics;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net.Http;

namespace QMS.Companion.Api.Controllers;

[ApiController]
[Route("api/v1/network")]
public class NetworkController : ControllerBase
{
    private readonly QmsRpcService _qmsRpcService;

    public NetworkController(QmsRpcService qmsRpcService)
    {
        _qmsRpcService = qmsRpcService;
    }

    // 1. Get Network Information
    /// <summary>
    /// Gets information about the QMS network.
    /// </summary>
    /// <remarks>
    /// Returns the network name and chain ID.
    /// </remarks>
    /// <returns>QMS network information.</returns>
    [HttpGet]
    public async Task<IActionResult> GetNetwork()
    {
        try
        {
            var chainId = await _qmsRpcService.GetChainIdAsync();

            return Ok(new
            {
                network = "QMS Testnet",
                chainId = chainId,
                rpc = "https://rpc.testnet.qms.finance"
            });
        }
        catch (TaskCanceledException)
        {
            return StatusCode(504, new
            {
                error = "QMS RPC request timed out"
            });
        }
        catch (HttpRequestException)
        {
            return StatusCode(502, new
            {
                error = "Unable to connect to QMS RPC"
            });
        }
    }

    // 2. Get Latest Block
    /// <summary>
    /// Gets the latest block from the QMS network.
    /// </summary>
    /// <remarks>
    /// Returns block number, timestamp, gas usage and transaction count.
    /// </remarks>
    [HttpGet("block/latest")]
    public async Task<IActionResult> GetLatestBlock()
    {
        try
        {
            var block = await _qmsRpcService.GetLatestBlockAsync();

            if (block.ValueKind == JsonValueKind.Null)
            {
                return NotFound(new
                {
                    error = "Latest block not found"
                });
            }

            var blockNumber = HexToBigInteger(
                block.GetProperty("number").GetString()!
            );

            var timestamp = HexToBigInteger(
                block.GetProperty("timestamp").GetString()!
            );

            var gasUsed = HexToBigInteger(
                block.GetProperty("gasUsed").GetString()!
            );

            var transactions = block.GetProperty("transactions");

            return Ok(new
            {
                blockNumber = blockNumber.ToString(),
                timestamp = DateTimeOffset
                    .FromUnixTimeSeconds((long)timestamp)
                    .UtcDateTime,
                gasUsed = gasUsed.ToString(),
                transactionCount = transactions.GetArrayLength()
            });
        }
        catch (TaskCanceledException)
        {
            return StatusCode(504, new
            {
                error = "QMS RPC request timed out"
            });
        }
        catch (HttpRequestException)
        {
            return StatusCode(502, new
            {
                error = "Unable to connect to QMS RPC"
            });
        }
    }

    // 3. Get Wallet Balance
    /// <summary>
    /// Gets the balance of a QMS wallet.
    /// </summary>
    /// <remarks>
    /// Returns the wallet address and balance in QMS.
    /// </remarks>
    [HttpGet("wallet/{address}/balance")]
    public async Task<IActionResult> GetWalletBalance(string address)
    {
        if (!Regex.IsMatch(address, "^0x[a-fA-F0-9]{40}$"))
        {
            return BadRequest(new
            {
                error = "Invalid wallet address"
            });
        }

        try
        {
            var balanceHex =
                await _qmsRpcService.GetBalanceAsync(address);

            var balanceWei = HexToBigInteger(balanceHex);

            var balanceQms = WeiToQms(balanceWei);

            var response = new WalletBalanceResponse
            {
                Address = address,
                Balance = balanceQms,
                Symbol = "QMS"
            };

            return Ok(response);
        }
        catch (TaskCanceledException)
        {
            return StatusCode(504, new
            {
                error = "QMS RPC request timed out"
            });
        }
        catch (HttpRequestException)
        {
            return StatusCode(502, new
            {
                error = "Unable to connect to QMS RPC"
            });
        }
    }

    // 4. Get Transaction
    /// <summary>
    /// Gets transaction details by transaction hash.
    /// </summary>
    /// <remarks>
    /// Returns sender, receiver, transaction value and block information.
    /// </remarks>
    [HttpGet("/api/v1/transactions/{hash}")]
    public async Task<IActionResult> GetTransaction(string hash)
    {
        if (!Regex.IsMatch(hash, "^0x[a-fA-F0-9]{64}$"))
        {
            return BadRequest(new
            {
                error = "Invalid transaction hash"
            });
        }

        try
        {
            var tx = await _qmsRpcService.GetTransactionAsync(hash);

            if (tx.ValueKind == JsonValueKind.Null)
            {
                return NotFound(new
                {
                    error = "Transaction not found"
                });
            }

            var valueHex = tx.GetProperty("value").GetString()!;
            var valueWei = HexToBigInteger(valueHex);
            var valueQms = WeiToQms(valueWei);

            var blockNumberElement = tx.GetProperty("blockNumber");

            var response = new TransactionResponse
            {
                Hash = tx.GetProperty("hash").GetString()!,
                From = tx.GetProperty("from").GetString()!,
                To = tx.GetProperty("to").ValueKind == JsonValueKind.Null
                    ? null
                    : tx.GetProperty("to").GetString(),

                ValueQMS = valueQms,

                Gas = HexToBigInteger(
                    tx.GetProperty("gas").GetString()!
                ).ToString(),

                BlockNumber = blockNumberElement.ValueKind == JsonValueKind.Null
                    ? null
                    : HexToBigInteger(
                        blockNumberElement.GetString()!
                    ).ToString()
            };

            return Ok(response);
        }
        catch (TaskCanceledException)
        {
            return StatusCode(504, new
            {
                error = "QMS RPC request timed out"
            });
        }
        catch (HttpRequestException)
        {
            return StatusCode(502, new
            {
                error = "Unable to connect to QMS RPC"
            });
        }
    }

    // 5. Get Transaction Receipt
    /// <summary>
    /// Gets the receipt of a QMS transaction.
    /// </summary>
    /// <remarks>
    /// Returns transaction status, gas used, block hash and other receipt details.
    /// </remarks>
    [HttpGet("/api/v1/transactions/{hash}/receipt")]
    public async Task<IActionResult> GetTransactionReceipt(string hash)
    {
        if (!Regex.IsMatch(hash, "^0x[a-fA-F0-9]{64}$"))
        {
            return BadRequest(new
            {
                error = "Invalid transaction hash"
            });
        }

        try
        {
            var receipt =
                await _qmsRpcService.GetTransactionReceiptAsync(hash);

            if (receipt.ValueKind == JsonValueKind.Null)
            {
                return NotFound(new
                {
                    error = "Transaction receipt not found or transaction is pending"
                });
            }

            var statusHex = receipt.GetProperty("status").GetString();

            var response = new TransactionReceiptResponse
            {
                TransactionHash =
                    receipt.GetProperty("transactionHash").GetString()!,

                Status = statusHex == "0x1"
                    ? "Success"
                    : "Failed",

                From = receipt.GetProperty("from").GetString()!,

                To = receipt.GetProperty("to").ValueKind == JsonValueKind.Null
                    ? null
                    : receipt.GetProperty("to").GetString(),

                BlockNumber = HexToBigInteger(
                    receipt.GetProperty("blockNumber").GetString()!
                ).ToString(),

                GasUsed = HexToBigInteger(
                    receipt.GetProperty("gasUsed").GetString()!
                ).ToString(),

                BlockHash =
                    receipt.GetProperty("blockHash").GetString()!,

                ContractAddress =
                    receipt.GetProperty("contractAddress").ValueKind == JsonValueKind.Null
                        ? null
                        : receipt.GetProperty("contractAddress").GetString()
            };

            return Ok(response);
        }
        catch (TaskCanceledException)
        {
            return StatusCode(504, new
            {
                error = "QMS RPC request timed out"
            });
        }
        catch (HttpRequestException)
        {
            return StatusCode(502, new
            {
                error = "Unable to connect to QMS RPC"
            });
        }
    }

    // Helper: Convert Hexadecimal to BigInteger
    private static BigInteger HexToBigInteger(string hex)
    {
        return BigInteger.Parse(
            "0" + hex[2..],
            System.Globalization.NumberStyles.HexNumber
        );
    }

    // Helper: Convert Wei to QMS
    private static string WeiToQms(BigInteger wei)
    {
        var divisor = BigInteger.Pow(10, 18);

        var whole = wei / divisor;
        var fraction = (wei % divisor)
            .ToString()
            .PadLeft(18, '0')
            .TrimEnd('0');

        return fraction.Length == 0
            ? whole.ToString()
            : $"{whole}.{fraction}";
    }
}
