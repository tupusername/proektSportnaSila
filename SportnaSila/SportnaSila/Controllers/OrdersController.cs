using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportnaSila.Data;
using SportnaSila.Models;

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

        // GET: Orders - Показва количката или админ панела
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var query = _context.Orders.Include(o => o.Product).Include(o => o.Client);

            if (User.IsInRole("Admin"))
            {
                // Админът вижда финализираните поръчки
                var adminOrders = await query
                    .Where(o => o.Status == "Completed")
                    .OrderByDescending(o => o.DateOrder)
                    .ToListAsync();
                return View(adminOrders);
            }
            else
            {
                // КЛИЕНТ: Стакваме визуално (групираме еднаквите продукти в една карта)
                var userCart = await query
                    .Where(o => o.ClientId == userId && o.Status == "Cart")
                    .GroupBy(o => o.ProductId)
                    .Select(g => new Orders
                    {
                        Id = g.First().Id,
                        ProductId = g.Key,
                        ClientId = userId,
                        Status = "Cart",
                        Quantity = g.Sum(x => x.Quantity), // Тук става 1+1=2
                        Product = g.First().Product,
                        Client = g.First().Client
                    })
                    .ToListAsync();

                return View(userCart);
            }
        }

        // AJAX POST: Добавяне в количка без презареждане
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int productId, int quantity = 1)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Json(new { success = false, message = "Влезте в профила си." });

            var existingOrder = await _context.Orders
                .FirstOrDefaultAsync(o => o.ProductId == productId && o.ClientId == userId && o.Status == "Cart");

            if (existingOrder != null)
            {
                existingOrder.Quantity += quantity;
                _context.Update(existingOrder);
            }
            else
            {
                _context.Add(new Orders { ProductId = productId, ClientId = userId, Quantity = quantity, Status = "Cart", DateOrder = DateTime.Now });
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // POST: Админът завършва и премахва поръчката от списъка
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteOrder(string clientId)
        {
            if (!User.IsInRole("Admin")) return Forbid();
            var items = _context.Orders.Where(o => o.ClientId == clientId && o.Status == "Completed");
            _context.Orders.RemoveRange(items);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Премахване на продукт от количката
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                var allSame = _context.Orders.Where(o => o.ProductId == order.ProductId && o.ClientId == order.ClientId && o.Status == "Cart");
                _context.Orders.RemoveRange(allSame);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}