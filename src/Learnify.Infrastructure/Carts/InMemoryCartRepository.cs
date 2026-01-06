using Learnify.Application.Common.Interfaces;
using Learnify.Domain.Entities;
using System.Collections.Concurrent;

namespace Learnify.Infrastructure.Carts
{
    /// <summary>
    /// In-memory cart repository for development when Redis is not available.
    /// Note: Cart data will be lost when the application restarts.
    /// </summary>
    public class InMemoryCartRepository : ICartRepository
    {
        private readonly ConcurrentDictionary<string, Cart> _carts = new();

        public Task<bool> DeleteCartAsync(string userId)
        {
            var removed = _carts.TryRemove(userId, out _);
            return Task.FromResult(removed);
        }

        public Task<Cart?> GetCartAsync(string userId)
        {
            _carts.TryGetValue(userId, out var cart);
            return Task.FromResult(cart);
        }

        public Task<Cart> UpdateCartAsync(Cart cart)
        {
            _carts.AddOrUpdate(cart.UserId, cart, (_, _) => cart);
            return Task.FromResult(cart);
        }
    }
}
