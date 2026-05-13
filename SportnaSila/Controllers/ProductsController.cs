using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportnaSila.Data;
using SportnaSila.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportnaSila.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Clients> _userManager;

        public ProductsController(ApplicationDbContext context, UserManager<Clients> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? categoryId)
        {
            var products = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            return View(await products.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var products = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (products == null) return NotFound();

            return View(products);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "BrandName");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            // Зареждаме доставчиците за падащото меню
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name"); 
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Name,Description,ImgUrl,Price,Quantity,CategoryId,BrandId,SupplierId")] Products products)
        {
            if (ModelState.IsValid)
            {
                _context.Add(products);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "BrandName", products.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", products.CategoryId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", products.SupplierId);
            return View(products);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var products = await _context.Products.FindAsync(id);
            if (products == null) return NotFound();

            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "BrandName", products.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", products.CategoryId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", products.SupplierId);
            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ImgUrl,Price,Quantity,CategoryId,BrandId,SupplierId")] Products products)
        {
            if (id != products.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(products);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductsExists(products.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "BrandName", products.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", products.CategoryId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", products.SupplierId);
            return View(products);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var products = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (products == null) return NotFound();

            return View(products);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var products = await _context.Products.FindAsync(id);
            if (products != null)
            {
                _context.Products.Remove(products);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductsExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}