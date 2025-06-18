using MemoramaUnidad2HTTP_8._1G.Models;
using MemoramaUnidad2HTTP_8._1G.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoramaUnidad2HTTP_8._1G.ViewModels
{
    public class MemoramaViewModel
    {
        public SesionJuego Sesion;

        public MemoramaViewModel(SesionJuego sesion)
        {
            Sesion = sesion;
        }
        public EstadoJuegoDTO ObtenerEstado() => Sesion.ObtenerEstado();

        public Dictionary<string, object> VoltearCarta(int jugadorId, int posicion)
        {
            var respuesta = Sesion.VoltearCarta(jugadorId, posicion);

            var dict = new Dictionary<string, object>();

            foreach (var prop in respuesta.GetType().GetProperties())
            {
                dict[prop.Name] = prop.GetValue(respuesta);
            }

            var todasEmparejadas = Sesion.ObtenerCartas().All(c => c.Encontrada);
            if (todasEmparejadas)
            {
                int puntosJ1 = Sesion.Jugador1?.CartasEncontradas ?? 0;
                int puntosJ2 = Sesion.Jugador2?.CartasEncontradas ?? 0;

                if (puntosJ1 > puntosJ2)
                    Sesion.SetEstadoFinal($"¡{Sesion.Jugador1?.Nombre ?? "Jugador 1"} gana!");
                else if (puntosJ2 > puntosJ1)
                    Sesion.SetEstadoFinal($"¡{Sesion.Jugador2?.Nombre ?? "Jugador 2"} gana!");
                else
                    Sesion.SetEstadoFinal("¡Empate!");
            }

            return dict;
        }
        public JugadorDTO Conectar(string nombre) => Sesion.ConectarJugador(nombre);

        public object ObtenerEstadoExtendido()
        {
            return new
            {
                jugadores = new[]
                {
            new {
                id = Sesion.Jugador1?.Id ?? -1,
                nombre = Sesion.Jugador1?.Nombre ?? "",
                pares = Sesion.Jugador1?.CartasEncontradas ?? 0
            },
            new {
                id = Sesion.Jugador2?.Id ?? -1,
                nombre = Sesion.Jugador2?.Nombre ?? "",
                pares = Sesion.Jugador2?.CartasEncontradas ?? 0
            }
        },
                turnoActualId = Sesion.TurnoActual,
                turnoActualNombre = Sesion.TurnoActual == 1 ? Sesion.Jugador1?.Nombre : Sesion.Jugador2?.Nombre,
                estado = Sesion.EstadoTexto,
                puedeComenzar = (Sesion.Jugador1 != null && Sesion.Jugador2 != null),
                cartas = Sesion.ObtenerCartas().Select(c => new {
                    indice = c.Indice,
                    imagen = (c.Descubierta || Sesion.CartasDescubiertas.Contains(c.Indice)) ? c.Imagen : null,
                    mostrando = c.Descubierta,
                    encontrada = Sesion.CartasDescubiertas.Contains(c.Indice)
                }).ToList()
            };
        }

        //public object ObtenerEstadoExtendido()
        //{
        //    return new
        //    {
        //        jugadores = new[] { Sesion.Jugador1?.Nombre ?? "", Sesion.Jugador2?.Nombre ?? "" },
        //        pares = new[] { Sesion.Jugador1?.CartasEncontradas ?? 0, Sesion.Jugador2?.CartasEncontradas ?? 0 },
        //        turnoActualId = Sesion.TurnoActual,
        //        turnoActualNombre = Sesion.TurnoActual == 1 ? Sesion.Jugador1?.Nombre : Sesion.Jugador2?.Nombre,
        //        estado = Sesion.EstadoTexto,
        //        cartas = Sesion.ObtenerCartas().Select(c => new {
        //            imagen = (c.Descubierta || Sesion.CartasDescubiertas.Contains(c.Indice)) ? c.Imagen : null,
        //            mostrando = c.Descubierta,
        //            encontrada = Sesion.CartasDescubiertas.Contains(c.Indice)

        //        })
        //    };
        //}
    }
}
