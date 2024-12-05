using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
        private dynamic livroEncontrado;

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
        public IActionResult Cadastro()
        {

            ViewBag.admin = HttpContext.Session.GetString("Admin")!;

            ViewBag.Categoria = context.Categoria.ToList();

            return View();
        }

        //metodo para cadastrar um livro
        [Route("(Cadastrar)")]
        public IActionResult Cadastrar(IFormCollection form)
        {

            Livro novolivro = new Livro();

            //o que meu usuario escrever no formulario sera atribuido ao novolivro
            novolivro.Nome = form["Nome"].ToString();
            novolivro.Descricao = form["Descricao"].ToString();
            novolivro.Editora = form["Editora"].ToString();
            novolivro.Escritor = form["Escritor"].ToString();
            novolivro.Idioma = form["Idioma"].ToString();
            //trabalhar com imagens; 
            if (form.Files.Count > 0)
            {
                //Primeiro passo
                var arquivo = form.Files[0];

                //Segundo passo:
                //Criar variavel do caminho da minha pasta para colocar as fotos dos livros
                var pasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Livros");
                //Validaremos se apasta que sera armazenada as imagens, existe. Caso não exista, criaremos uma nova pasta.
                if (!Directory.Exists(pasta))
                {
                    //Criar a pasta:
                    Directory.CreateDirectory(pasta);
                }
                //Terceiro passo:
                //Criar a variavel para armazenar o caminho em que meu arquivo estara, alem do nome dele
                var caminho = Path.Combine(pasta, arquivo.FileName);

                using (var stream = new FileStream(caminho, FileMode.Create))
                {
                    //Copiou o arquivo para o meu diretorio
                    arquivo.CopyTo(stream);
                }

                novolivro.Imagem = arquivo.FileName;


            }



            //img 
            context.Livro.Add(novolivro);
            context.SaveChanges();

            //segunda parte: e adicionar dentro de LivroCategoria e categoria que pertence ao novoLivro

            //Lista as categorias
            List<LivroCategoria> listalivroCategorias = new List<LivroCategoria>();

            //Array que possui as categorias selecionadas pelo usuario 
            string[] categoriasSelecionadas = form[("Categoria")].ToString().Split(',');
            //ação, terror, suspense  
            //3, 5, 7

            foreach (string categoria in categoriasSelecionadas)
            {
                LivroCategoria livroCategoria = new LivroCategoria();

                livroCategoria.CategoriaID = int.Parse(categoria);
                livroCategoria.LivroID = novolivro.LivroID;

                listalivroCategorias.Add(livroCategoria);
            }

            context.LivroCategoria.AddRange(listalivroCategorias);

            context.SaveChanges();

            return LocalRedirect("/Cadastro");
        }

        [Route("Editar/{id}")]

        public IActionResult Editar(int id)
        {
            ViewBag.admin = HttpContext.Session.GetString("Admin")!;

            ViewBag.CategoriasDoSistema = context.Categoria.ToList()!;
            //LivroID == 3

            //Buscar quem e o tal do id numero 3:
            Livro LivroEncontrado = context.Livro.FirstOrDefault(livro => livro.LivroID == id)!;

            //Buscar as categorias que o livroEncontrado possui 
            var CategoriaDoLivroEncontrado = context.LivroCategoria.Where(identificadorLivro => identificadorLivro.LivroID == id)
            .Select(livro => livro.Categoria).ToList();

            //Quero pegar as informacoes do meu livro selecionar e  mandar para ViewBang 
            ViewBag.Livro = livroEncontrado;
            ViewBag.Categoria = CategoriaDoLivroEncontrado;

            return View();
        }

        //metodo que atualiza as informacoes do livro:
        [Route("Atualizar/{id}")]
        public IActionResult Atualizar(IFormCollection form, int id, IFormFile imagem)
        {
            //Buscar um livro especifico pelo ID 
            Livro livroAtualizado = context.Livro.FirstOrDefault(livro => livro.LivroID == id)!;

            livroAtualizado.Nome = form["Nome"];
            livroAtualizado.Escritor = form["Nome"];
            livroAtualizado.Editora = form["Nome"];
            livroAtualizado.Descricao = form["Nome"];

            if (imagem.Length > 0)
            {
                //definir o caminho da minha imagem do livro ATUAL, que quero alterar
                var caminhoimagem = Path.Combine("wwwroot/images/Livros", imagem.FileName);

                //verificar se o usuario colocou uma imagem para atualizar o livro 

                if (string.IsNullOrEmpty(livroAtualizado.Imagem))
                {
                    //caso exite, ela ira ser apagada 

                    var caminhoimagemAntiga = Path.Combine("wwwroot/ImageFileMachine/Livros", livroAtualizado.Imagem);

                    //ver se existe uma imagem no caminho antigo
                    if (System.IO.File.Exists(caminhoimagemAntiga))
                    {
                        System.IO.File.Delete(caminhoimagemAntiga);
                    }
                }
                //Salvar a imagem nova 
                using (var stream = new FileStream(caminhoimagem,FileMode.Create))
                {
                    imagem.CopyTo(stream);
                } 
                //subir essa mudanca  para o banco de dados 
                livroAtualizado.Imagem = imagem.FileName;
            } 
            //Upload de imagem
            if(imagem != null && imagem.Length > 0){}

            //CATEGORIA:

            //PRIMEIRO: Precisamos pegar as categorias selecionadas do usuario
            var categoriasSelecionadas = form["Categoria"].ToList();
            //SEGUNDO: Pegaremos as categorias ATUAIS do livros
            var categoriasAtuais = context.LivroCategoria.Where(livro => livro.LivroID == id);
            //TERCEIRO: Removeremos as categorias antigas 
            foreach(var categoria in categoriasAtuais){
                if(!categoriasSelecionadas.Contains(categoria.CategoriaID.ToString())){
                    //nos vamos remover a categoria do nosso context
                    context.LivroCategoria.Remove(categoria);
                }

            }
            //QUARTO: Adicionaremos as novas categorias
            foreach(var categoria in categoriasSelecionadas){
                //verificando se nao existe a categoria nesse livro 
                if(!categoriasAtuais.Any(c => c.CategoriaID.ToString()== categoria)){
                    context.LivroCategoria.Add(new LivroCategoria{
                        LivroID = id,
                        CategoriaID = int.Parse(categoria)
                    });

                }

            }  


            context.SaveChanges();

            return LocalRedirect("/Livro");
        } 
        //Metodo de excluir o livro
        [Route("Excluir")] 
        public IActionResult Excluir(int id){
            //Buscar qual o livro id que precisamos excluir 
            Livro livroEncontrado = context.Livro.First(livro => livro.LivroID == id);

            //Buscar as categorias desse livro:
            var CategoriasDoLivro = context.LivroCategoria.Where(livro => livro.LivroID == id).ToList();

            //Precisa excluir primeiro o registro da tabela intermediaria
            foreach(var categoria in CategoriasDoLivro){
                context.LivroCategoria.Remove(categoria);
            } 

            context.Livro.Remove(livroEncontrado);

        




            context.SaveChanges();
            return LocalRedirect("/Livro");
        }

        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View("Error!");
        // }
    }
}