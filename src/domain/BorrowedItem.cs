using System.Text.Json;
using Flexlib.Infrastructure.Interop;

namespace Flexlib.Domain;


public class BorrowedItem
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public DateTime? BorrowedAt { get; set; }
    public DateTime? Appetite { get; set; }
    public ProgressVariable Progress { get; set; } = new();
    public int Priority { get; set; }

    public BorrowedItem(string itemId, string itemName)
    {
        Id = itemId;
        Name = itemName;
        BorrowedAt = DateTime.UtcNow;
    }

    public BorrowedItem(){}

    public Result SetAppetite(DateTime appetite)
    {
        Appetite = appetite;
        return Result.Success("");
    }

    public Result SetPriority(int priority)
    {
        Priority = priority;
        return Result.Success("");
    }

    public class ProgressVariable
    {
        public string  VariableUnit    { get; set; } = "%";
        public string? CurrentValue    { get; set; }
        public string? CompletionValue { get; set; }

        public bool IsCompleted => CurrentValue == CompletionValue;
    }
}


