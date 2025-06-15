using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemoramaUnidad2HTTP_8._1G.Models;
using System.Threading.Tasks;
using MemoramaUnidad2HTTP_8._1G.ViewModels;

namespace MemoramaUnidad2HTTP_8._1G.Models
{
    public class SesionJuego
    {
        public JugadorDTO? Jugador1 { get; private set; }
        public JugadorDTO? Jugador2 { get; private set; }
        public int ObtenerCantidadJugadores()
        {
            int cantidad = 0;
            if (Jugador1 != null) cantidad++;
            if (Jugador2 != null) cantidad++;
            return cantidad;
        }

        public List<JugadorDTO> Jugadores
        {
            get
            {
                var lista = new List<JugadorDTO>();
                if (Jugador1 != null) lista.Add(Jugador1);
                if (Jugador2 != null) lista.Add(Jugador2);
                return lista;
            }
        }
        public int TurnoActual { get; private set; } = 1;

        private string[] imagenes = new string[]
        {
            "Img1.jpg", "Img2.jpg", "Img3.jpg", "Img4.jpg", "Img5.jpg", "Img6.jpg"
        };
        private string estadoFinal = "";
        private string[] tablero;
        private bool[] descubiertas;
        public List<int> CartasDescubiertas { get; private set; } = new();
        private int? cartaVolteada = null;
        private bool[] mostrando = new bool[12];
        public EstadoJuego Estado { get; private set; } = EstadoJuego.EnCurso;
        public void SetEstadoFinal(string texto)
        {
            estadoFinal = texto;
        }
        public string EstadoTexto
        {
            get
            {
                if (estadoFinal != "")
                    return estadoFinal;

                if (Jugador2 == null)
                    return "Esperando jugador 2";
                if (Estado == EstadoJuego.EsperandoCambioTurno)
                    return "Cambiando turno...";
                return "";
            }
        }
        public List<CartaDTO> ObtenerCartas()
        {
            var cartas = new List<CartaDTO>();
            for (int i = 0; i < tablero.Length; i++)
            {
                bool descubierta = descubiertas[i];
                bool mostrando = CartasDescubiertas.Contains(i);

                cartas.Add(new CartaDTO
                {
                    Indice = i,
                    Imagen = (descubierta || mostrando) ? tablero[i] : null,
                    Descubierta = descubierta || mostrando
                });
            }
            return cartas;
        }

        public SesionJuego()
        {
            tablero = imagenes.Concat(imagenes).OrderBy(_ => Guid.NewGuid()).ToArray();
            descubiertas = new bool[12];
        }

        public JugadorDTO ConectarJugador(string nombre)
        {
            if (Jugador1 == null)
            {
                Jugador1 = new JugadorDTO { Id = 1, Nombre = nombre };
                return Jugador1;
            }
            else if (Jugador2 == null)
            {
                Jugador2 = new JugadorDTO { Id = 2, Nombre = nombre };
                return Jugador2;
            }
            else
            {
                if (Jugador1.Nombre == nombre) return Jugador1;
                if (Jugador2.Nombre == nombre) return Jugador2;
                throw new Exception("La partida ya tiene 2 jugadores.");
            }
        }

        public object VoltearCarta(int idJugador, int posicion)
        {
            if (Estado != EstadoJuego.EnCurso || TurnoActual != idJugador)
                return new { exito = false, mensaje = "No es tu turno o el juego no está en curso." };

            if (posicion < 0 || posicion >= tablero.Length || descubiertas[posicion] || CartasDescubiertas.Contains(posicion))
                return new { exito = false, mensaje = "Carta inválida o ya descubierta." };

            cartaVolteada = posicion;
            CartasDescubiertas.Add(posicion);
            mostrando[posicion] = true;

            if (CartasDescubiertas.Count == 2)
            {
                var i1 = CartasDescubiertas[0];
                var i2 = CartasDescubiertas[1];

                if (tablero[i1] == tablero[i2])
                {
                    descubiertas[i1] = true;
                    descubiertas[i2] = true;

                    if (idJugador == 1) Jugador1!.CartasEncontradas++;
                    else Jugador2!.CartasEncontradas++;

                    CartasDescubiertas.Clear();
                    cartaVolteada = null;
                }
                else
                {
                    Estado = EstadoJuego.EsperandoCambioTurno;

                    Task.Run(async () =>
                    {
                        await Task.Delay(1500);
                        CambiarTurnoSiEsNecesario();
                    });
                }
            }
            mostrando[posicion] = true;
            return new
            {
                exito = true,
                imagen = "Assets/Img/" + tablero[posicion],
                indice = posicion
            };
        }


        //public object VoltearCarta(int idJugador, int posicion)
        //{
        //    if (Estado != EstadoJuego.EnCurso || TurnoActual != idJugador)
        //        return new { exito = false, mensaje = "No es tu turno o el juego no está en curso." };

        //    if (posicion < 0 || posicion >= tablero.Length || descubiertas[posicion] || CartasDescubiertas.Contains(posicion))
        //        return new { exito = false, mensaje = "Carta inválida o ya descubierta." };

        //    cartaVolteada = posicion;

        //    CartasDescubiertas.Add(posicion);

        //    if (CartasDescubiertas.Count == 2)
        //    {
        //        var i1 = CartasDescubiertas[0];
        //        var i2 = CartasDescubiertas[1];

        //        if (tablero[i1] == tablero[i2])
        //        {
        //            descubiertas[i1] = true;
        //            descubiertas[i2] = true;

        //            if (idJugador == 1) Jugador1!.CartasEncontradas++;
        //            else Jugador2!.CartasEncontradas++;

        //            CartasDescubiertas.Clear();
        //            cartaVolteada = null;
        //        }
        //        else
        //        {
        //            Estado = EstadoJuego.EsperandoCambioTurno;
        //        }

        //        CambiarTurnoSiEsNecesario();
        //    }
        //    mostrando[posicion] = true;
        //    return new
        //    {
        //        exito = true,
        //        imagen = "Assets/Img/" + tablero[posicion],
        //        indice = posicion
        //    };
        //}

        public JugadorDTO ObtenerOtroJugador(int actual)
        {
            if (actual == 1 && Jugador2 != null) return Jugador2;
            if (actual == 2 && Jugador1 != null) return Jugador1;
            throw new Exception("Jugador no encontrado.");
        }

        public void CambiarTurnoSiEsNecesario()
        {
            if (Estado == EstadoJuego.EsperandoCambioTurno)
            {
                CartasDescubiertas.Clear();
                cartaVolteada = null;
                TurnoActual = TurnoActual == 1 ? 2 : 1;
                Estado = EstadoJuego.EnCurso;
            }
        }

        public EstadoJuegoDTO ObtenerEstado()
        {
            CambiarTurnoSiEsNecesario();

            return new EstadoJuegoDTO
            {
                Jugador1 = Jugador1,
                Jugador2 = Jugador2,
                Turno = TurnoActual,
                CartasDescubiertas = descubiertas.ToArray()
            };
        }

        //public EstadoJuegoDTO ObtenerEstado()
        //{
        //    CambiarTurnoSiEsNecesario();

        //    return new EstadoJuegoDTO
        //    {
        //        Jugador1 = Jugador1 != null ? new JugadorDTO
        //        {
        //            Id = Jugador1.Id,
        //            Nombre = Jugador1.Nombre,
        //            CartasEncontradas = Jugador1.CartasEncontradas
        //        } : null,

        //        Jugador2 = Jugador2 != null ? new JugadorDTO
        //        {
        //            Id = Jugador2.Id,
        //            Nombre = Jugador2.Nombre,
        //            CartasEncontradas = Jugador2.CartasEncontradas
        //        } : null,

        //        Turno = TurnoActual,
        //        CartasDescubiertas = descubiertas.ToArray()
        //    };
        //}

        public object ObtenerEstadoExtendido()
        {
            CambiarTurnoSiEsNecesario();

            return new
            {
                jugadores = new string[]
                {
            Jugador1?.Nombre ?? "",
            Jugador2?.Nombre ?? ""
                },
                pares = new int[]
                {
            Jugador1?.CartasEncontradas ?? 0,
            Jugador2?.CartasEncontradas ?? 0
                },
                turnoActualId = TurnoActual,
                turnoActualNombre = TurnoActual == 1 ? Jugador1?.Nombre : Jugador2?.Nombre,
                estado = Estado.ToString(),
                cartas = Enumerable.Range(0, tablero.Length).Select(i => new
                {
                    indice = i,
                    descubierta = descubiertas[i],
                    mostrando = CartasDescubiertas.Contains(i),
                    imagen = (descubiertas[i] || CartasDescubiertas.Contains(i)) ? "Assets/Img/" + tablero[i] : null
                }).ToList()
            };
        }



    }

    public enum EstadoJuego
    {
        EnCurso,
        EsperandoCambioTurno
    }
}

