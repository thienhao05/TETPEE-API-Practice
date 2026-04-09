using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TetPee.Repository;
using TetPee.Repository.Entity;

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

    public async Task AddProductToCart(Request.AddProductToCartRequest request)
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        
        var userIdGuid =  Guid.Parse(userId!);
        
        var query = _dbContext.Carts.Where(x => 
            x.UserId == userIdGuid);
        
        var cart = await query.FirstOrDefaultAsync();

        if (cart == null)
        {
            cart = new Repository.Entity.Cart()
            {
                Id = Guid.NewGuid(),
                UserId = userIdGuid,
            };
            
            _dbContext.Add(cart);
            await _dbContext.SaveChangesAsync();
        }
        
        var productQuery = _dbContext.CartDetails.Where(x => 
            x.Cart.Id == cart.Id && 
            x.ProductId == request.ProductId);
        
        // Cộng số lượng
        var cartExist = await productQuery.FirstOrDefaultAsync();
        
        if (cartExist != null)
        {
            cartExist.Quantity +=  request.Quantity;
            _dbContext.Update(cartExist);
            await _dbContext.SaveChangesAsync();
            return;
        }

        //cart chưa tồn tại thì tạo mới
        var cartDetail = new CartDetail()
        {
            CartId = cart.Id,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
        };
        
        _dbContext.Add(cartDetail);
        await _dbContext.SaveChangesAsync();
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