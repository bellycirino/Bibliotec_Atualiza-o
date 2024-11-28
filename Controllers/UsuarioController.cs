using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bibliotec.Contexts;
using Bibliotec.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
namespace Bibliotec_mvc.Controllers
{
    [Route("[controller]")]
    public class UsuarioController : Controller
    {
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(ILogger<UsuarioController> logger)
        {
            _logger = logger;
        }

        Context context = new Context();

        public IActionResult Index()
        {
             int id = int.Parse(HttpContext.Session.GetString("UsuarioID")!);
            ViewBag.admin = HttpContext.Session.GetString("Admin")!;

            Usuario usuarioEncontrado = context.Usuario.FirstOrDefault(usuario => usuario.UsuarioID == id)!;

            if (usuarioEncontrado == null){
                return NotFound();
            } 

            Curso cursoEncontrado = context.Curso.FirstOrDefault(curso => curso.CursoID == usuarioEncontrado.CursoID)!;

            if (cursoEncontrado == null){
                //no usuario nao possui curso cadastro
                //Preciso que vc manda essa mensagem para a View:
                ViewBag.Curso = "O usuario nao possui curso cadastrado";
            }else { 
                //o usuario possui o curso XXX
                ViewBag.Curso = cursoEncontrado.Nome; 
            } 

            ViewBag.Nome = usuarioEncontrado.Nome;
            ViewBag.Email = usuarioEncontrado;
            ViewBag.Contato = usuarioEncontrado.Contato;
            ViewBag.Nasc = usuarioEncontrado.DtNascimento.ToString("dd/MM/yyyy");





            return View();
        }

        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View("Error!");
        // }
    }
}