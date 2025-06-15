using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoramaUnidad2HTTP_8._1G.Models
{
    public class EstadoJuegoDTO
    {
        public JugadorDTO? Jugador1 { get; set; }
        public JugadorDTO? Jugador2 { get; set; }
        public int Turno { get; set; }
        public bool[] CartasDescubiertas { get; set; }
    }
}
