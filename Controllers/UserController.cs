using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using Shop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shop.Controllers
{
    [Route("v1/users")]
    public class UserController : Controller
    {
        // [AllowAnonymous]: permite qualquer acesso
        // [Authorize]: permite qualquer acesso que tenha token, independete das roles
        // [Authorize(Roles = "manager")]: permite apenas acesso com a role manager
        // [Authorize(Roles = "employee")]: permite apenas acesso com a role employee

        [HttpGet]
        [Route("")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<List<User>>> Get([FromServices]DataContext context)
        {
            var users = await context.Users.AsNoTracking().ToListAsync();
            return users;
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<User>> Put([FromServices]DataContext context, int id, [FromBody]User model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != model.Id)
                return NotFound(new { Message = "Usuário não encontrado" });

            try
            {
                context.Entry(model).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return model;
            }
            catch (Exception)
            {
                return BadRequest(new { Message = "Não foi possível criar o usuário" });
            }
        }

        [HttpPost]
        [Route("")]
        [AllowAnonymous]
        //[Authorize(Roles = "manager")]
        public async Task<ActionResult<User>> Post([FromServices]DataContext context, [FromBody]User model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Força o usuário a ser sempre 'funcionário'
                model.Role = "employee";

                context.Users.Add(model);
                await context.SaveChangesAsync();

                // Esconde a senha
                model.Password = string.Empty;

                return model;
            }
            catch (Exception)
            {
                return BadRequest(new { Message = "Não foi possível criar o usuário" });
            }
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<dynamic>> Authenticate([FromServices]DataContext context, [FromBody]User model)
        {
            // dynamic: pois nem sempre será um usuário que será retornado e sim uma mensagem de erro
            // https://jwt.io/ site para analisar o token

            var user = await context.Users.AsNoTracking().Where(x => x.Username == model.Username && x.Password == model.Password).FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { Message = "Usuário ou senha inválido" });

            var token = TokenService.GenerateToken(user);

            // Esconde a senha
            user.Password = string.Empty;

            return new
            {
                user = user,
                token = token
            };
        }
    }
}