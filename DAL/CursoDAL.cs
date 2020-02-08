using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebhookDF.DAL
{
    public class CursoDAL
    {

        List<Models.Curso> _cursos = new List<Models.Curso>();

        public CursoDAL()
        {
            _cursos.Add(new Models.Curso()
            {
                Id = 1,
                Nome = "Ciência da Computação",
                Preco = 1000,
                Sinonimos = new List<string>() { 
                    "computação",
                    "bcc",
                    "cc"

                }
            });

            _cursos.Add(new Models.Curso()
            {
                Id = 2,
                Nome = "Sistemas de Informação",
                Preco = 800,
                Sinonimos = new List<string>() {
                    "bsi",
                    "si"
                }
            });

            _cursos.Add(new Models.Curso()
            {
                Id = 3,
                Nome = "Sistemas para Internet",
                Preco = 600,
                Sinonimos = new List<string>() {
                    "tsi",
                    "internet"
                }
            });

            _cursos.Add(new Models.Curso()
            {
                Id = 4,
                Nome = "Gestão da Tecnologia da Informação",
                Preco = 600,
                Sinonimos = new List<string>() {
                    "gti",
                    "gt"
                }
            });

            _cursos.Add(new Models.Curso()
            {
                Id = 5,
                Nome = "Redes de Computadores",
                Preco = 600,
                Sinonimos = new List<string>() {
                    "redes",
                    "rc"
                }
            });

            _cursos.Add(new Models.Curso()
            {
                Id = 6,
                Nome = "Jogos Digitais",
                Preco = 600,
                Sinonimos = new List<string>() {
                    "jogos",
                    "jd"
                }
            });

            _cursos.Add(new Models.Curso()
            {
                Id = 7,
                Nome = "Engenharia de Software",
                Preco = 500,
                Sinonimos = new List<string>() {
                    "bes",
                    "es"
                }
            });
        }


        public IEnumerable<Models.Curso> ObterTodos()
        {
            return _cursos;
        }

        public string ObterTodosFormatoTexto() {

            string cursos = string.Join(", ", (from curso in _cursos select curso.Nome).ToArray());
            return cursos;
        }

        public Models.Curso ObterCurso(string busca)
        {
            busca = busca.Trim().ToLower();

            Models.Curso curso = (from c in _cursos
                                  where c.Nome.ToLower() == busca || c.Sinonimos.Contains(busca)
                                  select c).FirstOrDefault();

            return curso;
        }

    }
}
