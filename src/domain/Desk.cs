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

    public Result SortDesk(string sortSequence)
    {
        return Result.Success("");
    }
}
