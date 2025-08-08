namespace Flexlib.Domain;

public class LoanHistoryEntry
{
    public string     UserId     { get; set; }
    public DateTime   BorrowedAt { get; set; }
    public DateTime?  ReturnedAt { get; set; }

    public bool WasReturned => ReturnedAt.HasValue;

    public LoanHistoryEntry(string userId)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        BorrowedAt = DateTime.UtcNow;
    }

    public void MarkReturned()
    {
        ReturnedAt = DateTime.UtcNow;
    }
}

