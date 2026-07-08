using System;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data.Repositories;

namespace Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IGenericRepository<Product>? _products;
        private IGenericRepository<Item>? _items;
        private IGenericRepository<User>? _users;
        private IGenericRepository<RefreshToken>? _refreshTokens;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<Product> Products => 
            _products ??= new GenericRepository<Product>(_context);

        public IGenericRepository<Item> Items => 
            _items ??= new GenericRepository<Item>(_context);

        public IGenericRepository<User> Users => 
            _users ??= new GenericRepository<User>(_context);

        public IGenericRepository<RefreshToken> RefreshTokens => 
            _refreshTokens ??= new GenericRepository<RefreshToken>(_context);

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
