using System.Collections.Generic;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers;

public class ProductsController
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    public List<Product> GetAll()
    {
        return _db.Products;
    }
}
