using QuickCheckr;

namespace QuickWebr;

public record Observation<TReader>(Func<TReader, CheckrOf<Case>> Invariant);