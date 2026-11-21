using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrderSystem.Data;
using OrderSystem.Models;
using OrderSystem.Services;

namespace OrderSystem.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductDbContext _dbContext;
        private readonly IProductService _productService;

        public ProductController(ProductDbContext dbContext, IProductService productService)
        {
            _dbContext = dbContext;
            _productService = productService;
        }

        public IActionResult Index(string? searchString)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                ViewBag.SearchString = searchString;
            }

            return View(_productService.SearchProductsByName(searchString));
        }

        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_dbContext.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        public IActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Products.Add(product);
                _dbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Categories = new SelectList(_dbContext.Categories, "Id", "Name");
            return View(product);
        }

        public IActionResult Details(int id)
        {
            var product = _productService.GetProductById(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }


    }
}
