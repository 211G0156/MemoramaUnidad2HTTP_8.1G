using MemoramaUnidad2HTTP_8._1G.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoramaUnidad2HTTP_8._1G.Services
{
    public class JuegoService
    {
        private SesionJuego sesion;

        public JuegoService()
        {
            sesion = new SesionJuego();
        }

        public JugadorDTO ConectarJugador(string nombre)
        {
            return sesion.ConectarJugador(nombre);
        }

        public EstadoJuegoDTO ObtenerEstado()
        {
            return sesion.ObtenerEstado();
        }

        public object VoltearCarta(int jugadorId, int posicion)
        {
            return sesion.VoltearCarta(jugadorId, posicion);
        }
    }
}
