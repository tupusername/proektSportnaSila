using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportnaSila.Data;
using SportnaSila.Models;
using System.Security.Claims;

namespace SportnaSila.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Clients> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<Clients> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 1. ИНДЕКС - Количка на потребителя или списък за Админа
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                var adminOrders = await _context.Orders
                    .Include(o => o.Product)
                    .Include(o => o.Client)
                    .Where(o => o.Status == "Pending")
                    .ToListAsync();

                return View(adminOrders);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var clientCart = await _context.Orders
                .Include(o => o.Product)
                .Where(o => o.ClientId == userId && o.Status == "Cart")
                .ToListAsync();

            return View(clientCart);
        }

        // 2. ДОБАВЯНЕ В КОЛИЧКАТА
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (quantity <= 0) quantity = 1;

            var existingOrder = await _context.Orders
                .FirstOrDefaultAsync(o => o.ClientId == userId && o.ProductId == productId && o.Status == "Cart");

            if (existingOrder != null)
            {
                existingOrder.Quantity += quantity;
                _context.Update(existingOrder);
            }
            else
            {
                var newOrder = new Orders
                {
                    ClientId = userId,
                    ProductId = productId,
                    Quantity = quantity,
                    Status = "Cart",
                };
                _context.Add(newOrder);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Orders");
        }

        // 3. АЯКС CHECKOUT (ПОРЪЧВАНЕ)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(string fullName, string city, string courier, string deliveryType, string deliveryAddress)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cartItems = await _context.Orders
                .Where(o => o.ClientId == userId && o.Status == "Cart")
                .ToListAsync();

            if (cartItems == null || !cartItems.Any())
            {
                return Json(new { success = false, message = "Количката ви е празна!" });
            }

            foreach (var item in cartItems)
            {
                item.Status = "Pending";
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // 4. ПРЕМАХВАНЕ НА ПРОДУКТ ОТ КОЛИЧКАТА
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 5. АДМИН: ЗАВЪРШВАНЕ НА ПОРЪЧКА
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CompleteOrder(string clientId)
        {
            var clientOrders = await _context.Orders
                .Where(o => o.ClientId == clientId && o.Status == "Pending")
                .ToListAsync();

            foreach (var order in clientOrders)
            {
                order.Status = "Completed";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}