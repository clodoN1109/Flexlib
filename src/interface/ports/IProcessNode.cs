public interface IProcessNode
{
    object OriginalInput { get; }
    object GetNewValue();
}
