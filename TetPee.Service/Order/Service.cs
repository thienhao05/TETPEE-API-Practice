using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TetPee.Repository;
using TetPee.Repository.Entity;

namespace TetPee.Service.Order;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }

    public async Task<Response.CreateOrderResponse> CreateOrder(Request.CreateOrderRequest request)
    {
        var userId = _httpContext.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;

        var userIdGuid = Guid.Parse(userId!);
        
        // List Object -> List Guid => Mapping List thì xài Select
        var productIds = request.Products.Select(x => x.ProductId).Distinct().ToList();

        var query = _dbContext.Products.Where(x => productIds.Contains(x.Id));

        var productCount = await query.CountAsync();

        if (productCount != productIds.Count)
        {
            throw new Exception("Some products not found");
        }

        var result = await query.ToListAsync();

        decimal totalAmount = 0;

        foreach (var prod in result)    
        {
           //  Tìm trong list giỏ hàng sản phẩm đề tìm số lượng mà khách hàng muốn mua !
           var quantity = request.Products.First(x => 
               x.ProductId == prod.Id).Quantity;

           if (quantity <= 0)
           {
               throw new Exception($"Quantity of product {prod.Id} must be greater than 0");
           }
           
           totalAmount += prod.Price * quantity;
        }

        if (totalAmount <= 0)
        {
            throw new Exception("Total amount must be greater than 0");
        }

        var order = new Repository.Entity.Order()
        {
            Id = Guid.NewGuid(),
            UserId = userIdGuid,
            TotalAmount = totalAmount,
            Address = request.Address,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
        };
        
        _dbContext.Add(order);
        await _dbContext.SaveChangesAsync();

        List<OrderDetail> orderDetails = new List<OrderDetail>();

        foreach (var prod in result)
        {
            var quantity = request.Products.First(x =>
                x.ProductId == prod.Id).Quantity;

            var orderDetail = new OrderDetail()
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = prod.Id,
                Quantity = quantity,
                UnitPrice = prod.Price,
            };
            
            orderDetails.Add(orderDetail);
        }

        if (orderDetails.Any())
        {
            _dbContext.AddRange(orderDetails);
            await _dbContext.SaveChangesAsync();
        }
        
        //Trả ra QR cho người dùng quét
        string description = $"TETPEE-{order.Id}";

        var response = new Response.CreateOrderResponse()
        {
            OrderId = order.Id,
            TotalAmount = order.TotalAmount,
            BankName = "TPBank",
            BankAccount = "00005668350",
            Description = description,
            QRCode = "",
        };
        
        string qrCode = $"https://qr.sepay.vn/img?" +
                        $"acc={response.BankAccount}&" +
                        $"bank={response.BankName}&" +
                        $"amount={(int)totalAmount}&" +
                        $"des={description}&" +
                        $"template=qronly";
        
        response.QRCode = qrCode;
        
        return response;
    }
}