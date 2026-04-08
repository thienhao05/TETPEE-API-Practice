using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TetPee.Repository;

namespace TetPee.Service.Cart;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }

    public async Task CreateCart()
    {
        // Tạo cart thì cần UserId
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        
        var userIdGuid =  Guid.Parse(userId!);
        
        var query = _dbContext.Carts.Where(x => x.UserId == userIdGuid);

        var isExist = await query.AnyAsync();

        if (isExist)
        {
            throw new Exception("Cart already exist");
        }

        var cart = new Repository.Entity.Cart()
        {
            UserId = userIdGuid,
        };
        
        _dbContext.Carts.Add(cart);
        await _dbContext.SaveChangesAsync();
    }

    public Task AddProductToCart(Request.AddProductToCartRequest request)
    {
        throw new NotImplementedException();
    }

    public Task RemoveProductFromCart(Request.RemoveProductFromCartRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<List<Response.ProductResponse>> GetCart()
    {
        throw new NotImplementedException();
    }
}