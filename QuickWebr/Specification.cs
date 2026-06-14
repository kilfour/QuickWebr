using QuickCheckr;

namespace QuickWebr;

public record Specification<TReader>(Func<HttpClient, TReader, CheckrOf<(Func<bool>, CheckrOf<Case>)>> Checkr);