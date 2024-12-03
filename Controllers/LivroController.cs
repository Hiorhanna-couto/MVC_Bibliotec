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
        public IActionResult Index()
        {

            ViewBag.Admin = HttpContext.Session.GetString("Admin")!;

            // criar uma lista de livro 
            List<Livro> listaLivros = context.Livro.ToList();

            //Verifeca se o livro tem reserva ou nao
            var livroReservados = context.LivroReserva.ToDictionary(livro => livro.LivroID, livror => livror.DtReserva);

            ViewBag.Livro = listaLivros;
            ViewBag.livroComReserva = livroReservados;



            return View();
        }

        [Route("Cadastro")]
        //metodo que retorna a tela de cadastro:
        public IActionResult Cadastro()
        {
            ViewBag.Admin = HttpContext.Session.GetString("Admin")!;
            ViewBag.Categoria = context.Categoria.ToList();




            // retorna a View de Cadastro:
            return View();
        }
        //metodo para cadastrar u livro:
        [Route("Cadastrar")]
        public IActionResult Cadastrar(IFormCollection form)
        {

            Livro novoLivro = new Livro();
            // o que o usuario escrever no formulario sera atribuido ao novoLivro
            //Nome
            novoLivro.Nome = form["Nome"].ToString();
            //Descricao
            novoLivro.Descricao = form["Descricao"].ToString();
            //Editora
            novoLivro.Editora = form["Editora"].ToString();
            //Escritor
            novoLivro.Escritor = form["Escritor"].ToString();
            //Idioma
            novoLivro.Idioma = form["Idioma"].ToString();
               
           // img  
            context.Livro.Add(novoLivro);
            context.SaveChanges();

           


            return View();
        }


        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View("Error!");
        // }
    }
}