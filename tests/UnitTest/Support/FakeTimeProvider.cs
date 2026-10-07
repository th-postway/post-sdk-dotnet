namespace Postway.UnitTest.Support;

/// <summary>A clock the test sets by hand.</summary>
public sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => Now;
}
