using Microsoft.EntityFrameworkCore.Storage;
using WordInverser.DAL.Context;
using WordInverser.DAL.Interfaces;

namespace WordInverser.DAL.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly WordInverserDbContext _context;
    private IDbContextTransaction? _transaction;

    public IWordCacheRepository WordCacheRepository { get; }
    public IRequestResponseRepository RequestResponseRepository { get; }

    public UnitOfWork(
        WordInverserDbContext context,
        IWordCacheRepository wordCacheRepository,
        IRequestResponseRepository requestResponseRepository)
    {
        _context = context;
        WordCacheRepository = wordCacheRepository;
        RequestResponseRepository = requestResponseRepository;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
