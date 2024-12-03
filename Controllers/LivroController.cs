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
    public class LivroController : Controller
    {
        private readonly ILogger<LivroController> _logger;

        public LivroController(ILogger<LivroController> logger)
        {
            _logger = logger;
        } 

        Context context = new Context();
        private dynamic livrosReservados;

        public IActionResult Index()
        {
             ViewBag.admin = HttpContext.Session.GetString("Admin")!;
             List<Livro> listaLivros = context.Livro.ToList();

             var livrosReserva = context.LivroReserva.ToDictionary(livro => livro.LivroID, livror => livror.DtReserva);

             ViewBag.Livros = listaLivros;
             ViewBag.LivrosComReserva = livrosReservados;

            return View();
        } 

        [Route("(Cadastro)")]
        //Metodo que retorna a tela de cadastro:
        public IActionResult Cadastro(){

            ViewBag.admin = HttpContext.Session.GetString("Admin")!;
            
            ViewBag.Categoria = context.Categoria.ToList();


            return View();
        }

        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View("Error!");
        // }
    }
}