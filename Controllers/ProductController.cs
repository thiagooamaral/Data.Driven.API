using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
    [Route("v1/products")]
    public class ProductController : ControllerBase
    {
        // dotnet watch run: comando para subir a api
        // não abre o navegador e reinicia o servidor a cada mudança

        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Product>>> Get([FromServices]DataContext context)
        {
            // Include: utilizado pelo EF para fazer JOIN entre duas entidades
            // Posso ter quantos includes forem necessários
            var products = await context.Products.Include(x => x.Category).AsNoTracking().ToListAsync();
            return Ok(products);
        }

        [HttpGet]
        [Route("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<Product>> GetById(int id, [FromServices]DataContext context)
        {
            // FirstOrDefaultAsync: utilizado para retornar apenas um registro
            // Quando não encontra nada, retorna null
            var product = await context.Products.Include(x => x.Category).AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            return Ok(product);
        }

        [HttpGet]
        [Route("categories/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Product>>> GetByCategory(int id, [FromServices]DataContext context)
        {
            // Where: utilizado para condições, pode retornar mais de um registro
            var products = await context.Products.Include(x => x.Category).AsNoTracking()
                .Where(x => x.CategoryId == id).ToListAsync();
            return Ok(products);
        }

        [HttpPost]
        [Route("")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<Product>> Post([FromBody]Product model, [FromServices]DataContext context)
        {
            if (ModelState.IsValid)
            {
                context.Products.Add(model);
                await context.SaveChangesAsync();

                return Ok(model);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
    }
}