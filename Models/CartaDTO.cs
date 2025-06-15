using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoramaUnidad2HTTP_8._1G.Models
{
    public class CartaDTO
    {
        public int Indice { get; set; }
        public string Imagen { get; set; }
        public bool Descubierta { get; set; }
        public bool Encontrada => Descubierta;
    }
}
