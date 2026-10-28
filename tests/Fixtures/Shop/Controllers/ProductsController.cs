using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers;

public class ProductsController
{
    private readonly AppDbContext _db;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(AppDbContext db , ILogger<ProductsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    public List<Product> GetAll()
    {
        _logger.LogInformation("Listing all products");
        return _db.Products;
    }
}
