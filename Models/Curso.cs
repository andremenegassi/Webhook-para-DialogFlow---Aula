using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebhookDF.Models
{
    public class Curso
    {
        int _id;
        string _nome;
        decimal _preco;
        List<string> _sinonimos;

        public int Id { get => _id; set => _id = value; }
        public string Nome { get => _nome; set => _nome = value; }
        public decimal Preco { get => _preco; set => _preco = value; }
        public List<string> Sinonimos { get => _sinonimos; set => _sinonimos = value; }

        public Curso()
        {
            Sinonimos = new List<string>();
        }
    }
}
