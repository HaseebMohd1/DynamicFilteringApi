using DynamicFilteringApi.Database;
using DynamicFilteringApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DynamicFilteringApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly ProductDbContext productDbContext;

    public ProductController(ProductDbContext productDbContext)
    {
        this.productDbContext = productDbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        await this.productDbContext.Database.EnsureCreatedAsync();
        return Ok(productDbContext.Products);
    }

    [HttpPost("search-product")]
    public async Task<IActionResult> SearchProducts([FromBody]ProductSearchCriteria productSearch)
    {
        await this.productDbContext.Database.EnsureCreatedAsync();

        ParameterExpression parameter = Expression.Parameter(typeof(Product), "x"); // x =>
        Expression predicate = Expression.Constant(true); // x => true

        if ( productSearch.IsActive.HasValue )
        {
            MemberExpression memberExpression = Expression.Property(parameter, nameof(Product.IsActive)); // x.IsActive
            ConstantExpression constantExpression = Expression.Constant(productSearch.IsActive.Value); // true

            BinaryExpression binaryExpression = Expression.Equal(memberExpression, constantExpression); // x.IsActive == true
            predicate = Expression.AndAlso(predicate, binaryExpression); // x => true && x.IsActive == true
        }

        var lambdaExpression = Expression.Lambda<Func<Product, bool>>(predicate, parameter); // x => true && x.IsActive == true
        var data = await productDbContext.Products.Where(lambdaExpression).ToListAsync();

        return Ok(data);
    }
}
