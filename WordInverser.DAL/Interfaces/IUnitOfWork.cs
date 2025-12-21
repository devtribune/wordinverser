namespace WordInverser.DAL.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IWordCacheRepository WordCacheRepository { get; }
    IRequestResponseRepository RequestResponseRepository { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
