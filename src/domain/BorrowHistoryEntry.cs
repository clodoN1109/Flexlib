namespace Flexlib.Domain;

public class BorrowHistoryEntry
{
    public string     UserId     { get; set; }
    public DateTime   BorrowedAt { get; set; }
    public DateTime?  ReturnedAt { get; set; }

    public bool WasReturned => ReturnedAt.HasValue;

    public BorrowHistoryEntry(string userId)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        BorrowedAt = DateTime.UtcNow;
    }

    public void MarkReturned()
    {
        ReturnedAt = DateTime.UtcNow;
    }
}

