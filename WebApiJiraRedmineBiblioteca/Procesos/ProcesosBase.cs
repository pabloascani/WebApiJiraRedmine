using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using WebApiJiraRedmineBiblioteca.Data;

namespace WebApiJiraRedmineBiblioteca.Procesos
{
    public static class ProcesosBase
    {
        public static string GetCurrentMethod()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return sf.GetMethod().Name;
        }

        // Función de Llamada hacia la web api Bonita o Glpi
        public static ObjetoDevuelto ObtenerRespuesta(
            string UrlPrincipal,
            string Recurso,
            Dictionary<string, string> Encabezados,
            Dictionary<string, string> Parametros,
            string Body,
            Method Metodo)
        {
            ObjetoDevuelto Objeto = new();

            try
            {
                var options = new RestClientOptions(UrlPrincipal)
                {
                    MaxTimeout = -1,
                };
                RestClient cliente = new RestClient(options);

                RestRequest Llamada = new RestRequest(Recurso, Metodo);

                foreach (KeyValuePair<string, string> Encabezado in Encabezados)
                    Llamada.AddHeader(Encabezado.Key, Encabezado.Value);

                foreach (KeyValuePair<string, string> Parametro in Parametros)
                    Llamada.AddParameter(Parametro.Key, Parametro.Value);

                if (!string.IsNullOrEmpty(Body)) Llamada.AddStringBody(Body, DataFormat.Json);

                RestResponse Respuesta = cliente.Execute(Llamada);
                Objeto.Success = Respuesta.IsSuccessful;
                Objeto.Mensaje = !Objeto.Success ? (string.IsNullOrEmpty(Respuesta.ErrorMessage + Respuesta.ErrorException.Message + Respuesta.Content) ? "<p>* No se ha logrado procesar la solicitud en " + UrlPrincipal + Recurso + ".</p>" : "<p>* " + Respuesta.ErrorMessage + " " + Respuesta.ErrorException.Message + " " + Respuesta.Content + ".</p>") : string.Empty;
                Objeto.Data = new object[1] { Respuesta };
            }
            catch (Exception ex)
            {
                Objeto.Success = false;
                Objeto.Mensaje = string.Format(Mensajes.MessageException, GetCurrentMethod(), ex.Message);
                Objeto.Data = Array.Empty<object>();
            }

            return Objeto;
        }
    }
}