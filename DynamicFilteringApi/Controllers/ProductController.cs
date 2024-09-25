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

        if(productSearch.Categories is not null && productSearch.Categories.Any() )
        {
            MemberExpression memberExpression = Expression.Property(parameter, nameof(Product.Category)); // x.Category
            Expression orExpression = Expression.Constant(false); // x => false

            foreach (var category in productSearch.Categories )
            {
                var constExpression = Expression.Constant(category.Name); // "Electronics"
                BinaryExpression binaryExpression = Expression.Equal(memberExpression, constExpression); // x.Category == "Electronics"

                orExpression = Expression.OrElse(orExpression, binaryExpression); // x => false || x.Category == "Electronics"
            }

            predicate = Expression.AndAlso(predicate, orExpression); // x => true && (x.IsActive == true && (x.Category == "Electronics" || x.Category == "Food"))
        }

        if(productSearch.Names is not null && productSearch.Names.Any() )
        {
            MemberExpression memberExpression = Expression.Property(parameter, nameof(Product.Name)); // x.Name
            Expression orExpression = Expression.Constant(false); // x => false

            foreach(var name in productSearch.Names )
            {
                var constExpression = Expression.Constant(name.Name); // "Keyboard"
                BinaryExpression binaryExpression = Expression.Equal(memberExpression, constExpression); // x.Name == "Keyboard"

                orExpression = Expression.OrElse(orExpression, binaryExpression); // x => false || x.Name == "Keyboard"
            }

            predicate = Expression.AndAlso(predicate, orExpression); // x => true && (x.IsActive == true && (x.Category == "Electronics" || x.Category == "Food") && (x.Name == "Keyboard" || x.Name == "Mouse"))
        }

        if ( productSearch.Price is not null )
        {
            // target : x.Price >= 10 && x.Price <= 20

            Expression left = Expression.Property(parameter, nameof(Product.Price)); // x.Price
            if ( productSearch.Price.Min is not null )
            {
                Expression right = Expression.Constant(productSearch.Price.Min.Value); // 10
                BinaryExpression binaryExpression = Expression.GreaterThanOrEqual(left, right); // x.Price >= 10

                predicate = Expression.AndAlso(predicate, binaryExpression); // x => true && (x.IsActive == true && (x.Category == "Electronics" || x.Category == "Food") && (x.Name == "Keyboard" || x.Name == "Mouse") && x.Price >= 10)
            }

            if ( productSearch.Price.Max is not null )
            {
                Expression right = Expression.Constant(productSearch.Price.Max.Value); // 20
                BinaryExpression binaryExpression = Expression.LessThanOrEqual(left, right); // x.Price <= 20

                predicate = Expression.AndAlso(predicate, binaryExpression); // x => true && (x.IsActive == true && (x.Category == "Electronics" || x.Category == "Food") && (x.Name == "Keyboard" || x.Name == "Mouse") && x.Price >= 10 && x.Price <= 20)
            }
        }

        var lambdaExpression = Expression.Lambda<Func<Product, bool>>(predicate, parameter); // x => true && x.IsActive == true
        var data = await productDbContext.Products.Where(lambdaExpression).ToListAsync();

        return Ok(data);
    }
}
