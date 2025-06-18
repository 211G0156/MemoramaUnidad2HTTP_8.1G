using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Net;
using MemoramaUnidad2HTTP_8._1G.Services;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json;
using MemoramaUnidad2HTTP_8._1G.ViewModels;

namespace MemoramaUnidad2HTTP_8._1G.Models
{
    public class ServidorMemorama
    {

        private HttpListener listener;
        private bool activo = true;

        //private SesionJuego sesion;
        //private MemoramaViewModel juego;

        private Dictionary<Guid, MemoramaViewModel> sesiones = new();
        private Dictionary<int, Guid> jugadorASesion = new();
        private int siguienteIdJugador = 1;

        public ServidorMemorama()
        {
            //sesion = new SesionJuego();
            //juego = new MemoramaViewModel(sesion);
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:7777/");
        }

        //public void Iniciar()
        //{
        //    listener.Start();
        //    Console.WriteLine("Servidor corriendo en http://localhost:7777/");
        //    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        //    {
        //        FileName = "http://localhost:7777/",
        //        UseShellExecute = true
        //    });

        //    Escuchar();
        //}
        public void Iniciar()
        {
            if (listener == null)
            {
                listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:7777/");
            }

            if (!listener.IsListening)
            {
                try
                {
                    listener.Start();
                    Console.WriteLine("Servidor corriendo en http://localhost:7777/");
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "http://localhost:7777/",
                        UseShellExecute = true
                    });

                    activo = true;
                    Escuchar();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al iniciar el servidor: {ex.Message}");
                }
            }
        }

        private async void Escuchar()
        {
            while (activo)
            {
                HttpListenerContext ctx = null;

                try
                {
                    ctx = await listener.GetContextAsync();
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (HttpListenerException)
                {
                    if (!activo)
                        break;
                    else
                        throw; 
                }

                if (ctx == null) continue;

                string ruta = ctx.Request.Url.LocalPath;

                switch (ruta)
                {
                    case "/":
                    case "/index.html":
                        EnviarArchivo(ctx, "Assets/index.html", "text/html");
                        break;

                    case "/conectar":
                        await RutaConectar(ctx);
                        break;

                    case "/estado":
                        await RutaEstado(ctx);
                        break;

                    case "/voltear":
                        await RutaVoltear(ctx);
                        break;
                    case "/reiniciar":
                        await RutaReiniciar(ctx);
                        break;

                    default:
                        if (ruta.StartsWith("/Assets/"))
                            EnviarArchivo(ctx, ruta.TrimStart('/'), GetMimeType(ruta));
                        else
                            Respuesta404(ctx);
                        break;
                }
                ctx.Response.Close();
            }
        }

        private void EnviarArchivo(HttpListenerContext ctx, string archivo, string tipoMime)
        {
            if (File.Exists(archivo))
            {
                byte[] contenido = File.ReadAllBytes(archivo);
                ctx.Response.ContentType = tipoMime;
                ctx.Response.OutputStream.Write(contenido, 0, contenido.Length);
            }
            else
            {
                ctx.Response.StatusCode = 404;
                byte[] error = Encoding.UTF8.GetBytes("Archivo no encontrado");
                ctx.Response.OutputStream.Write(error, 0, error.Length);
            }
        }

        private async Task RutaConectar(HttpListenerContext ctx)
        {
            using var reader = new StreamReader(ctx.Request.InputStream);
            string jsonRecibido = await reader.ReadToEndAsync();
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonRecibido);

            if (!data.ContainsKey("nombre") || string.IsNullOrWhiteSpace(data["nombre"]))
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Nombre inválido"));
                return;
            }

            string nombre = data["nombre"];
            Guid idSesion = Guid.Empty;
            MemoramaViewModel partida = null;

            foreach (var par in sesiones)
            {
                if (par.Value.Sesion.ObtenerCantidadJugadores() < 2)
                {
                    partida = par.Value;
                    idSesion = par.Key;
                    break;
                }
            }


            if (partida == null)
            {
                idSesion = Guid.NewGuid();
                var nuevaSesion = new SesionJuego();
                partida = new MemoramaViewModel(nuevaSesion);
                sesiones[idSesion] = partida;
            }

            var jugador = partida.Conectar(nombre);
            jugadorASesion[jugador.Id] = idSesion;

            var respuesta = new
            {
                jugadorId = jugador.Id,
                nombre = jugador.Nombre
            };

            string json = JsonSerializer.Serialize(respuesta);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            ctx.Response.ContentType = "application/json";
            await ctx.Response.OutputStream.WriteAsync(buffer);
        }

        private async Task RutaEstado(HttpListenerContext ctx)
        {
            var query = ctx.Request.QueryString;
            if (!int.TryParse(query["jugadorId"], out int jugadorId))
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Id inválido"));
                return;
            }

            if (!jugadorASesion.TryGetValue(jugadorId, out Guid sesionId) || !sesiones.TryGetValue(sesionId, out var partida))
            {
                ctx.Response.StatusCode = 404;
                await ctx.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Sesión no encontrada"));
                return;
            }

            var estado = partida.ObtenerEstadoExtendido();
            string json = JsonSerializer.Serialize(estado);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            ctx.Response.ContentType = "application/json";
            await ctx.Response.OutputStream.WriteAsync(buffer);
        }

        private async Task RutaVoltear(HttpListenerContext ctx)
        {
            var query = ctx.Request.QueryString;
            int jugadorId = int.Parse(query["jugadorId"]);
            int posicion = int.Parse(query["pos"]);

            if (!jugadorASesion.TryGetValue(jugadorId, out Guid sesionId) || !sesiones.TryGetValue(sesionId, out var partida))
            {
                ctx.Response.StatusCode = 404;
                await ctx.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Sesión no encontrada"));
                return;
            }

            var resultado = partida.VoltearCarta(jugadorId, posicion);

            if (resultado != null && resultado.TryGetValue("imagen", out object imagenObj))
            {
                if (imagenObj is string nombreImagen && !string.IsNullOrEmpty(nombreImagen))
                {
                    resultado["imagen"] = $"Assets/Img/{nombreImagen}";
                }
            }

            string json = JsonSerializer.Serialize(resultado);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            ctx.Response.ContentType = "application/json";
            await ctx.Response.OutputStream.WriteAsync(buffer);
        }
        private async Task EscribirRespuestaJson(HttpListenerContext ctx, object data)
        {
            string json = JsonSerializer.Serialize(data);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            ctx.Response.ContentType = "application/json";
            await ctx.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }

        //private async Task RutaReiniciar(HttpListenerContext ctx)
        //{
        //    sesion = new SesionJuego();
        //    juego = new MemoramaViewModel(sesion);

        //    await EscribirRespuestaJson(ctx, new { exito = true, mensaje = "Juego reiniciado" });
        //}
        private async Task RutaReiniciar(HttpListenerContext ctx)
        {
            var query = ctx.Request.QueryString;
            if (!int.TryParse(query["jugadorId"], out int jugadorId))
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Id inválido"));
                return;
            }

            if (!jugadorASesion.TryGetValue(jugadorId, out Guid sesionId))
            {
                ctx.Response.StatusCode = 404;
                await ctx.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Sesión no encontrada"));
                return;
            }

            sesiones.Remove(sesionId);
            jugadorASesion.Remove(jugadorId);

            var respuesta = new { exito = true };
            string json = JsonSerializer.Serialize(respuesta);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            ctx.Response.ContentType = "application/json";
            await ctx.Response.OutputStream.WriteAsync(buffer);
        }
        private void Respuesta404(HttpListenerContext ctx)
        {
            ctx.Response.StatusCode = 404;
            byte[] buffer = Encoding.UTF8.GetBytes("Recurso no encontrado");
            ctx.Response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        private string GetMimeType(string path)
        {
            string ext = Path.GetExtension(path).ToLower();
            return ext switch
            {
                ".js" => "application/javascript",
                ".css" => "text/css",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".html" => "text/html",
                _ => "application/octet-stream"
            };
        }
        public void Detener()
        {
            try
            {
                activo = false;
                if (listener != null)
                {
                    if (listener.IsListening)
                    {
                        listener.Stop();
                    }
                    listener.Close();
                    listener = null; 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al detener el servidor: {ex.Message}");
            }
        }


        //public void Detener()
        //{
        //    activo = false;
        //    if (listener != null)
        //    {
        //        if (listener.IsListening)
        //        {
        //            listener.Stop();
        //        }
        //        listener.Close();
        //        listener = null;
        //    }
        //}
    }
}
