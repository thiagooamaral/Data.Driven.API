using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
    [Route("v1/categories")]
    public class CategoryController : ControllerBase
    {
        // Endpoint = URL
        // https://localhost:5001
        // Porta 5000 - http
        // Porta 5001 - https

        // [Route("")] - utilizado para localizar as controllers e métodos
        // Padrão REST - utiliza a mesma rota, só troca os verbos

        // ActionResult - boa prática de utilização para retornar o padrão esperado (200, 204...)
        // Task - programação assíncrona, não trava a Thread principal do sistema (mais detalhes futuramente)

        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        [ResponseCache(VaryByHeader = "User-Agent", Location = ResponseCacheLocation.Any, Duration = 30)]
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<ActionResult<List<Category>>> Get([FromServices]DataContext context)
        {
            // AsNoTracking: exclusivo do EF, utilizar em casos de somente leitura (GET por exemplo)
            // Nas consultas, o EF traz mais informações dentro do objeto, para não trazer, utilizamos o AsNoTracking
            // ToListAsync(): é quando o comando será executado no BD de fato, deixar por ultimo (realizar todos os filtros antes, melhora na performance)
            var categories = await context.Categories.AsNoTracking().ToListAsync();
            return Ok(categories);
        }

        // [Route("{id:int}")] - restrição de rota, retorna 404 caso não seja informado um int como parâmetro

        [HttpGet]
        [Route("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<Category>> GetById(int id, [FromServices]DataContext context)
        {
            var category = await context.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return Ok(category);
        }

        // [FromBody] - usar no POST para saber de onde buscar as informações
        // POST, PUT e DELETE possui cabeçalho e corpo (header e body)

        [HttpPost]
        [Route("")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<Category>> Post([FromBody]Category model, [FromServices]DataContext context)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                context.Categories.Add(model);
                await context.SaveChangesAsync();

                return Ok(model);
            }
            catch
            {
                return BadRequest(new { message = "Não foi possível criar a categoria" });
            }
        }

        // PUT: Mescla o [FromBody] do POST com [Route("{id:int}")] do GetById

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<Category>> Put(int id, [FromBody]Category model, [FromServices]DataContext context)
        {
            // Verifica se o Id informado é o mesmo do modelo
            if (model.Id != id)
                return NotFound(new { message = "Categoria não encontrada"} );

            // Verifica se os dados são válidos
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                context.Entry<Category>(model).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return Ok(model);
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(new { message = "Este registro já foi atualizado" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possível atualizar a categoria" });
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<Category>> Delete(int id, [FromServices]DataContext context)
        {
            var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (category == null)
                return NotFound(new { message = "Categoria não encontrada" });

            try
            {
                context.Categories.Remove(category);
                await context.SaveChangesAsync();
                return Ok(new { message = "Categoria removida com sucesso" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possível remover a categoria" });
            }
        }
    }
}

