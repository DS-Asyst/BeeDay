namespace LevelUp.Web.Components.Features.Experience.Feedback;

public sealed class LevelUpFeedbackStore
{
    private readonly HashSet<Guid> _processedEntries = [];
    private readonly List<LevelUpFeedback> _history = [];

    public event Action? Changed;

    public LevelUpFeedback? Current { get; private set; }

    public IReadOnlyList<LevelUpFeedback> History => _history;

    public void Add(LevelUpFeedback feedback)
    {
        ArgumentNullException.ThrowIfNull(feedback);

        if (!_processedEntries.Add(feedback.ExperienceEntryId))
        {
            return;
        }

        Current = feedback;
        _history.Insert(0, feedback);
        if (_history.Count > 3)
        {
            _history.RemoveAt(_history.Count - 1);
        }

        Changed?.Invoke();
    }

    public void Consume()
    {
        if (Current is null)
        {
            return;
        }

        Current = null;
        Changed?.Invoke();
    }
}
