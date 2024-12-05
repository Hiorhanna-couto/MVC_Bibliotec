using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bibliotec.Contexts;
using Bibliotec.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
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
            //primeira parte: Cadastro livro na tabela de livro
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

            // trabalha com  imagen
            if (form.Files.Count > 0)
            {
                //Primeiro Passo :
                // Armazernaremos o arquivo enviado pelo usuario
                var arquivo = form.Files[0];
                //Segundo Passo:
                //Criar variavel do caminho da minha pasta para calocar as fotos dos livros  
                var pasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagens/Livros");

                //Valiadaremos se a pasta que sera armazenadaas imagens , existe .caso nao exista ,criaremos uma nova pasta 
                if (!Directory.Exists(pasta))
                {

                    //criar a pasta :
                    Directory.CreateDirectory(pasta);

                }
                //terceiro passo:
                //criar  a variavel para armazenar o caminho em que meu arquivo estara ,alem do nome dele 
                var caminhos = Path.Combine(pasta, arquivo.FileName);
                using (var stream = new FileStream(caminhos, FileMode.Create))
                {
                    //Copiou o arquivos para o meu diretoria 
                    arquivo.CopyTo(stream);

                }

                novoLivro.Imagem = arquivo.FileName;
            }
            else
            {


                novoLivro.Imagem = "padrao.png";
            }




            context.Livro.Add(novoLivro);
            context.SaveChanges();


            //SEGUNDA  PARTE : E adicinar dentro de LivroCategoria a categoria que pertence ao  novolivro
            //lista as categoria
            List<LivroCategoria> listalivroCategorias = new List<LivroCategoria>();
            //Array que possui as categorias selecionadas pelo usuario
            string[] categoriasSelecionadas = form["Categoria"].ToString().Split(',');
            // açao,terro, suspense

            foreach (string categoria in categoriasSelecionadas)
            {
                // categoria = 3 ou 5 ou 7
                //string categoria possui a informacao do id 
                LivroCategoria livroCategoria = new LivroCategoria();

                livroCategoria.CategoriaID = int.Parse(categoria);
                livroCategoria.LivroID = novoLivro.LivroID;
                // Adicionamos o obj livroCategoria dentro  da lista   listalivroCategorias
                listalivroCategorias.Add(livroCategoria);


            }
            //peguei a colecao da listalivrocategorias e co
            context.LivroCategoria.AddRange(listalivroCategorias);

            context.SaveChanges();


            return LocalRedirect("/Livro/Cadastro");
        }

        [Route("Editar/{id}")]
        public IActionResult Editar(int id)
        {
            ViewBag.Admin = HttpContext.Session.GetString("Admin")!;
            ViewBag.CategoriasDoSistema = context.Categoria.ToList();

            //LivroID == 3

            // Buscar  quem e o tel do id numero 3 : 
            Livro livroEncontrado = context.Livro.FirstOrDefault(Livro => Livro.LivroID == id)!;

            // Busca as cATEGORIA  que o LivroEncontrado possui
            var categoriasDoLivroEncontrado = context.LivroCategoria.
            Where(identificadorLivro => identificadorLivro.LivroID == id).
            Select(livro => livro.Categoria).
            ToList();

            // Quero pegar as informacoes do meu livro selecionado e mandar para a minha view
            ViewBag.Livro = livroEncontrado;
            ViewBag.Categoria = categoriasDoLivroEncontrado;

            return View();
        }

        //Metodo que atualiza as informacoes  do livro
        [Route("Atualizar / {id}")]
        public IActionResult Atualizar(IFormCollection form, int id, IFormFile imagem)
        {
            //Buscar um livro especifico pelo id
            Livro livroAtualizado = context.Livro.FirstOrDefault(livro => livro.LivroID == id)!;

            livroAtualizado.Nome = form["Nome"];
            livroAtualizado.Escritor = form["Escritor"];
            livroAtualizado.Editora = form["Editora"];
            livroAtualizado.Idioma = form["Idioma"];
            livroAtualizado.Descricao = form["Descricao"];

            //upload de imagem
            if (imagem != null && imagem.Length > 0)
            {
                //Definir o caminho da minha imagem do livro atual , que eu quero alteral :
                var caminhoImagem = Path.Combine("wwwroot/imagem/Livros", imagem.FileName);

                // VERIFICAR se o usuario  minha imagem ainda existe no meu caminho
                //
                if (!string.IsNullOrEmpty(livroAtualizado.Imagem))
                {
                    //Caso existe ,ele ira ser apagada
                    var caminhoImagemAntiga = Path.Combine("wwwroot/imagem/Livros", livroAtualizado.Imagem);

                    //ver se existe uma imagem no caminho antigo
                    if (System.IO.File.Exists(caminhoImagemAntiga))
                    {
                        System.IO.File.Delete(caminhoImagemAntiga);


                    }

                }
                //salva a imagem nova
                using (var stream = new FileStream(caminhoImagem, FileMode.Create))
                {

                    imagem.CopyTo(stream);

                }
                //Subir essa mundaca para o meu banco de dados
                livroAtualizado.Imagem = imagem.FileName;

            }

            //CATEGORIAS: 
            // PRIMEIRO : Precisamos pegar as categorias selecionadas do usuario
            var categoriasSelecionadas = form["Categoria"].ToList();
            // Segungo  : pegaremos as categoria ATUAIS do livro 
            var categoriasAtuais = context.LivroCategoria.Where(livro => livro.LivroID == id).ToList();
            // terceiro : removeremos as categorias antigas
            foreach (var categoria in categoriasAtuais)
            {
                if (!categoriasSelecionadas.Contains(categoria.CategoriaID.ToString()))
                {
                    //nos vamos remover a categoria do nosso context
                    context.LivroCategoria.Remove(categoria);
                }

            }
            // Quarta : Adicionaremos as novas categoria
            foreach (var categoria in categoriasSelecionadas)
            {
                //Verificando se nao existe a categoria nesse livro
                if (!categoriasAtuais.Any(c => c.CategoriaID.ToString() == categoria))
                    context.LivroCategoria.Add(new LivroCategoria
                    {
                        LivroID = id,
                        CategoriaID = int.Parse(categoria)

                    });
            }

            context.SaveChanges();
            return LocalRedirect("/Livro");
        }
        //Metodo de excluir o livro
        [Route("Excluir/{id}")]
        public IActionResult Excluir(int id)
        {
            //Buscar  qual o livro do id que precisamos excluir
            Livro livroEncontrado = context.Livro.First(livro => livro.LivroID == id);

            //buscar as categoria desse livro :
            var categoriasDoLivro = context.LivroCategoria.Where(livro => livro.LivroID == id).ToList();


            // Precisa excluir primeiro o registro da tabela intermediaria
            foreach (var categoria in categoriasDoLivro)
            {
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