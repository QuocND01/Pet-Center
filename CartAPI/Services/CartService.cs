using AutoMapper;
using CartAPI.DTOs;
using CartAPI.Models;
using CartAPI.Repositories.Interfaces;
using CartAPI.Services.Interfaces;

namespace CartAPI.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepo;
    private readonly ICartDetailRepository _cartDetailRepo;
    private readonly ProductApiService _productApi;
    private readonly IMapper _mapper;

    public CartService(
        ICartRepository cartRepo,
        ICartDetailRepository cartDetailRepo,
        ProductApiService productApi,
        IMapper mapper)
    {
        _cartRepo = cartRepo;
        _cartDetailRepo = cartDetailRepo;
        _productApi = productApi;
        _mapper = mapper;
    }

    
    public async Task<CartReadDto?> GetCartByCustomerIdAsync(Guid customerId)
    {
        var cart = await _cartRepo.GetByCustomerIdAsync(customerId);
        if (cart is null) return null;
        return await EnrichCartAsync(cart);
    }

    
    public async Task<CartReadDto?> GetCartByIdAsync(Guid cartId)
    {
        var cart = await _cartRepo.GetByIdAsync(cartId);
        if (cart is null) return null;
        return await EnrichCartAsync(cart);
    }

    
    public async Task<CartReadDto> InitCartAsync(CartInitDto dto)
    {
        // Idempotent: nếu đã có cart thì trả về cart hiện tại
        if (await _cartRepo.ExistsByCustomerIdAsync(dto.CustomerId))
        {
            var existing = await _cartRepo.GetByCustomerIdAsync(dto.CustomerId);
            return await EnrichCartAsync(existing!);
        }

        var cart = new Cart
        {
            CartId = Guid.NewGuid(),
            CustomerId = dto.CustomerId
        };

        var created = await _cartRepo.CreateAsync(cart);
        return await EnrichCartAsync(created);
    }

    
    public async Task<CartReadDto> AddItemToCartAsync(Guid cartId, CartDetailAddDto dto)
    {
        var cart = await _cartRepo.GetByIdAsync(cartId)
            ?? throw new KeyNotFoundException("Cart not found");

        // Tính tổng quantity nếu item đã có trong cart
        var existing = await _cartDetailRepo.GetByCartAndProductAsync(cartId, dto.ProductId);
        var totalQty = (existing?.Quantity ?? 0) + dto.Quantity;

        // Validate stock
        var (isValid, stock, error) = await _productApi.ValidateStockAsync(dto.ProductId, totalQty);
        if (!isValid)
            throw new ArgumentException(error);

        if (existing is not null)
        {
            existing.Quantity = totalQty;
            await _cartDetailRepo.UpdateAsync(existing);
        }
        else
        {
            var detail = new CartDetail
            {
                CartDetailsId = Guid.NewGuid(),
                CartId = cartId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            };
            await _cartDetailRepo.AddAsync(detail);
        }

        var updated = await _cartRepo.GetByIdAsync(cartId);
        return await EnrichCartAsync(updated!);
    }

    
    public async Task<CartReadDto> UpdateItemQuantityAsync(
        Guid cartId, Guid cartDetailsId, CartDetailUpdateDto dto)
    {
        if (!await _cartRepo.ExistsAsync(cartId))
            throw new KeyNotFoundException("Cart not found");

        var detail = await _cartDetailRepo.GetByIdAsync(cartDetailsId)
            ?? throw new KeyNotFoundException("Cart item not found");

        if (detail.CartId != cartId)
            throw new ArgumentException("Cart item does not belong to this cart");

        // Validate stock với số lượng MỚI (không cộng dồn - đây là set mới)
        if (detail.ProductId.HasValue)
        {
            var (isValid, stock, error) = await _productApi
                .ValidateStockAsync(detail.ProductId.Value, dto.Quantity);
            if (!isValid)
                throw new ArgumentException(error);
        }

        detail.Quantity = dto.Quantity;
        await _cartDetailRepo.UpdateAsync(detail);

        var cart = await _cartRepo.GetByIdAsync(cartId);
        return await EnrichCartAsync(cart!);
    }

    
    public async Task RemoveItemAsync(Guid cartId, Guid cartDetailsId)
    {
        if (!await _cartRepo.ExistsAsync(cartId))
            throw new KeyNotFoundException("Cart not found");

        var detail = await _cartDetailRepo.GetByIdAsync(cartDetailsId)
            ?? throw new KeyNotFoundException("Cart item not found");

        if (detail.CartId != cartId)
            throw new ArgumentException("Cart item does not belong to this cart");

        await _cartDetailRepo.DeleteAsync(cartDetailsId);
    }

    
    public async Task ClearCartAsync(Guid cartId)
    {
        if (!await _cartRepo.ExistsAsync(cartId))
            throw new KeyNotFoundException("Cart not found");

        await _cartDetailRepo.DeleteAllByCartIdAsync(cartId);
    }

    // ── PRIVATE HELPERS ───────────────────────────────────────────────

    private async Task<CartReadDto> EnrichCartAsync(Cart cart)
    {
        var dto = _mapper.Map<CartReadDto>(cart);

        var productIds = cart.CartDetails
            .Where(cd => cd.ProductId.HasValue)
            .Select(cd => cd.ProductId!.Value)
            .Distinct();

        var products = await _productApi.GetProductsByIdsAsync(productIds);

        foreach (var item in dto.CartDetails)
        {
            if (item.ProductId.HasValue && products.TryGetValue(item.ProductId.Value, out var p))
            {
                item.ProductName = p.ProductName;
                item.ProductPrice = p.ProductPrice;
                item.ProductImage = p.Images?.FirstOrDefault();
                item.StockQuantity = p.StockQuantity;
                item.SubTotal = p.ProductPrice * (item.Quantity ?? 0);
            }
        }

        dto.TotalItems = dto.CartDetails.Sum(x => x.Quantity ?? 0);
        dto.TotalPrice = dto.CartDetails.Sum(x => x.SubTotal ?? 0);

        return dto;
    }
}