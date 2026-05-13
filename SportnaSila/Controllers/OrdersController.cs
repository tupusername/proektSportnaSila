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

        public async Task<IActionResult> Index()
        {
            if (TempData["OrderSuccess"] != null)
            {
                ViewBag.OrderNumber = TempData["OrderNumber"];
                return View("Index", new List<Orders>());
            }

            var userId = _userManager.GetUserId(User);
            var query = _context.Orders.Include(o => o.Product).Include(o => o.Client);

            if (User.IsInRole("Admin"))
            {
                // Показваме само поръчки със статус "Completed" (тези, които чакат потвърждение)
                return View(await query.Where(o => o.Status == "Completed").ToListAsync());
            }

            ViewBag.Suppliers = await _context.Suppliers.ToListAsync();

            var groupedCart = await query
                .Where(o => o.ClientId == userId && o.Status == "Cart")
                .GroupBy(o => o.ProductId)
                .Select(g => new Orders
                {
                    Id = g.First().Id,
                    ProductId = g.Key,
                    ClientId = userId,
                    Status = "Cart",
                    Quantity = g.Sum(x => x.Quantity),
                    Product = g.First().Product,
                    Client = g.First().Client
                }).ToListAsync();

            return View(groupedCart);
        }

        // НОВ МЕТОД ЗА АДМИНА: Потвърждава и премахва от списъка
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminConfirm(string clientId)
        {
            var ordersToConfirm = await _context.Orders
                .Where(o => o.ClientId == clientId && o.Status == "Completed")
                .ToListAsync();

            foreach (var order in ordersToConfirm)
            {
                // Можеш да ги изтриеш или да им смениш статуса на "Confirmed", за да не се виждат в Index
                order.Status = "Archived";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeOrder(string firstName, string lastName, int supplierId, string deliveryOffice)
        {
            var userId = _userManager.GetUserId(User);
            var items = await _context.Orders.Where(o => o.ClientId == userId && o.Status == "Cart").ToListAsync();

            if (!items.Any()) return RedirectToAction(nameof(Index));

            foreach (var item in items)
            {
                item.Status = "Completed";
                item.DateOrder = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            var lastOrder = items.Last();
            TempData["OrderSuccess"] = true;
            TempData["OrderNumber"] = lastOrder.Id;

            return RedirectToAction(nameof(Index));
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int productId, int quantity = 1)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Json(new { success = false });

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
    }
}