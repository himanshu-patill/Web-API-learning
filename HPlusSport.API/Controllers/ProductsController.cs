﻿using HPlusSport.API.Classes;
using HPlusSport.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HPlusSport.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ShopContext _context;

        public ProductsController(ShopContext context)
        {
            _context = context;

            _context.Database.EnsureCreated();
        }

        //[HttpGet]
        //public IActionResult GetAllProducts()
        //{
        //    return Ok(_context.Products.ToArray());
        //    //return _context.Products.ToArray();
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetAllProducts()
        //{
        //    return Ok(await _context.Products.ToArrayAsync());
        //}

        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] ProductQueryParameters queryParameters)
        {
            IQueryable<Product> products = _context.Products;

            if (queryParameters.MinPrice != null &&
                queryParameters.MaxPrice != null)
            {
                products = products.Where(
                    p => p.Price >= queryParameters.MinPrice.Value &&
                         p.Price <= queryParameters.MaxPrice.Value);
            }

            if (!string.IsNullOrEmpty(queryParameters.SearchTerm))
            {
                products = products.Where(p => p.Sku.ToLower().Contains(queryParameters.SearchTerm.ToLower()) ||
                                               p.Name.ToLower().Contains(queryParameters.SearchTerm.ToLower()));
            }


            if (!string.IsNullOrEmpty(queryParameters.Sku))
            {
                products = products.Where(p => p.Sku == queryParameters.Sku);
            }
            if (!string.IsNullOrEmpty(queryParameters.Name))
            {
                products = products.Where(
                    p => p.Name.ToLower().Contains(queryParameters.Name.ToLower()));
            }

            if (!string.IsNullOrEmpty(queryParameters.SortBy))
            {
                if (typeof(Product).GetProperty(queryParameters.SortBy) != null)
                {
                    products = products.OrderByCustom(queryParameters.SortBy, queryParameters.SortOrder);
                }
            }

            products = products
                .Skip(queryParameters.Size * (queryParameters.Page - 1))
                .Take(queryParameters.Size);

            return Ok(await products.ToArrayAsync());
        }

        //[HttpGet("{id}")]
        //public IActionResult GetProduct(int id)
        //{
        //    var product = _context.Products.Find(id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(product);
        //}



        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct([FromBody] Product product)
        {
            foreach (var item in _context.Products)
            {
                if (item.Name.Contains(product.Name))
                {
                    return BadRequest();
                }
                
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                "GetProduct",
                new { id = product.Id },
                product
            );

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct([FromRoute] int id, [FromBody] Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (_context.Products.Find(id) == null)
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();

        }
    }
}
