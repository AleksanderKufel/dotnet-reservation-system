namespace ReservationSystem.IntegrationTests;

// Every test starts with empty tables; the schema and migration history are kept.
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected IntegrationTestBase(ReservationApiFactory factory)
    {
        Factory = factory;
    }

    protected ReservationApiFactory Factory { get; }

    public Task InitializeAsync() => Factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;
}
