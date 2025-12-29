namespace Main.Application.Abstractions.Data;

public interface IMainDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
