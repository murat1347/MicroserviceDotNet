using ESourcingProducts.Entities;
using MongoDB.Driver;

namespace ESourcingProducts.Data.Interfaces
{
    public interface IProductContext
    {
        IMongoCollection<Product> Products { get; }
    }

}
