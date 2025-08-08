using System.Text.Json;
using Flexlib.Infrastructure.Authentication;
using Flexlib.Infrastructure.Interop;

namespace Flexlib.Domain;


public class Desk
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public List<BorrowedItem> BorrowedItems { get; set; } = new();

    public Desk(string name, int id)
    {
        Id = id;
        Name = name;
    }

    public Desk(){}

    public void AddBorrowedItem(BorrowedItem item)
    {
        BorrowedItems.Add(item);
    }

    public void RemoveBorrowedItem(string itemId)
    {
        BorrowedItems.RemoveAll(i => i.Id == itemId);
    }

    public Result Sort(List<string>? sortSequence)
    {
        if (sortSequence == null || sortSequence.Count == 0)
            return Result.Success("No sort sequence provided. List left unmodified.");

        // Supported sort keys mapped to selectors
        var sortSelectors = new Dictionary<string, Func<BorrowedItem, object?>>(StringComparer.OrdinalIgnoreCase)
        {
            { "id",                 item => item.Id },
            { "name",               item => item.Name },
            { "borrowedat",         item => item.BorrowedAt },
            { "appetite",           item => item.Appetite },
            { "priority",           item => item.Priority },
            {
                "progress", item =>
                {
                    if (double.TryParse(item.Progress.CurrentValue, out var current) &&
                        double.TryParse(item.Progress.CompletionValue, out var total) &&
                        total != 0)
                    {
                        return current / total;
                    }

                    return item.Progress.CurrentValue; // or `null` or `double.NaN`, depending on what makes more sense in your context
                }
            },
            { "completion-value",   item => item.Progress.CompletionValue }
        };

        var invalidKeys = new List<string>();
        IOrderedEnumerable<BorrowedItem>? orderedItems = null;

        foreach (string rawKey in sortSequence)
        {
            string[] parts = rawKey.Split(':', 2, StringSplitOptions.TrimEntries);
            string key = parts[0].ToLowerInvariant();
            bool descending = parts.Length == 2 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            if (!sortSelectors.TryGetValue(key, out var selector))
            {
                invalidKeys.Add(rawKey);
                continue;
            }

            if (orderedItems == null)
            {
                orderedItems = descending
                    ? BorrowedItems.OrderByDescending(selector)
                    : BorrowedItems.OrderBy(selector);
            }
            else
            {
                orderedItems = descending
                    ? orderedItems.ThenByDescending(selector)
                    : orderedItems.ThenBy(selector);
            }
        }

        if (orderedItems != null)
            BorrowedItems = orderedItems.ToList();

        return invalidKeys.Count == 0
            ? Result.Success("Sorted successfully.")
            : Result.Warn($"Some sort keys were ignored: {string.Join(", ", invalidKeys)}");
    }
}
