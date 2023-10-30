using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using WebApiJiraRedmineBiblioteca.Data;

namespace WebApiJiraRedmineBiblioteca.Procesos
{
    public class ProcesosJira
    {
        // Obtengo todos los tickets de Jira, para insertarlos/modificarlos en Redmine
        public string ObtenerTicketsJira()
        {
            ObjetoDevuelto obj = new();
            RestResponse Respuesta;

            Dictionary<string, string> Encabezados = new Dictionary<string, string>
                    {
                        { "Accept", "application/json" },
                        { "TokenJira", VariablesSesion.Configuracion["TokenApiJira"] }
                    };

            Dictionary<string, string> Parametros = new Dictionary<string, string>();

            ObjetoDevuelto objObtencionTickets = ProcesosBase.ObtenerRespuesta(
                VariablesSesion.Configuracion["UrlApiJira"],
                VariablesSesion.Configuracion["ResourceGetTicketsJira"],
                Encabezados,
                Parametros,
                string.Empty,
                Method.Get);

            if (objObtencionTickets.Success)
            {
                Respuesta = (RestResponse)objObtencionTickets.Data[0];

                if (Respuesta == null)
                {
                    obj.Success = false;
                    obj.Mensaje += "<p>* No se ha logrado obtener los tickets de Jira.</p>";
                }
                else if (string.IsNullOrEmpty(Respuesta.Content))
                {
                    obj.Success = false;
                    obj.Mensaje += "<p>* No se han hallado tickets de Jira relacionados.</p>";
                }
                else
                {
                    ListadoTicketsJira listadoTickets = JsonConvert.DeserializeObject<ListadoTicketsJira>(Respuesta.Content);

                    if (listadoTickets.issues.Count > 0)
                    {
                        foreach (TicketFromJira issue in listadoTickets.issues)
                        {
                            TicketFromJira ticketFromJira = ObtenerTicket(issue.id, out string MensajeSalida);

                            if (!string.IsNullOrEmpty(MensajeSalida))
                            {
                                obj.Success = false;
                                obj.Mensaje += MensajeSalida;
                            }
                            else
                            {
                                // Espacio para la definición pendiente de comentarios enviados a Redmine
                                List<CommentItem> Comentarios = ticketFromJira.fields.comment.Where(c => !string.IsNullOrEmpty(c.body.content[0].content[0].text)).ToList();
                                // Espacio para la definición pendiente de comentarios enviados a Redmine

                                string IdTicketRedmine = issue.fields.comment[0].body.content[0].content[0].text; // Campo para encontrar el id de ticket de Redmine Relacionado --> Pendiente
                                bool TicketNuevo = string.IsNullOrEmpty(IdTicketRedmine) || !int.TryParse(IdTicketRedmine, out int IdTicket);
                                ObjetoDevuelto objTicket = TicketNuevo ? new ProcesosRedmine().InsertarTicket(issue) : new ProcesosRedmine().ModificarTicket(issue);

                                if (!objTicket.Success)
                                {
                                    obj.Success = false;
                                    obj.Mensaje += objTicket.Mensaje;
                                }
                                else if (TicketNuevo)
                                {
                                    Respuesta = (RestResponse)objTicket.Data[0];
                                    IdTicketRedmine = "45";// JsonConvert.DeserializeObject<TicketFromApiRedmine>(Respuesta.Content).id --> Falta definir en que lugar de la respuesta se encontrará el id del ticket ingresado

                                    dynamic TicketToJira = new ExpandoObject();

                                    FieldToJira Fields = new FieldToJira();
                                    Fields.customfield_10000 = new CustomField10000();
                                    Fields.customfield_10000.content = new List<ContentItem>
                                    {
                                        new ContentItem() { content = new List<SubContentItem>() }
                                    };
                                    Fields.customfield_10000.content[0].content.Add(new SubContentItem()
                                    {
                                        text = "Texto Inventado",
                                        type = "text"
                                    });
                                    Fields.customfield_10000.type = "doc";
                                    Fields.customfield_10000.version = 1;
                                    Fields.customfield_10010 = 1;
                                    Fields.summary = "Ticket de Redmine Relacionado.";

                                    TicketToJira.fields = Fields;

                                    ObjetoDevuelto objModifTicket = ActualizarCustomField(issue.id, TicketToJira);
                                    if (!objModifTicket.Success)
                                    {
                                        obj.Success = false;
                                        obj.Mensaje += objModifTicket.Mensaje;
                                    }
                                }
                            }
                        }

                        obj.Mensaje = string.IsNullOrEmpty(obj.Mensaje) ? "<p>* Los tickets de Jira han sido integrados exitosamente.</p>" : obj.Mensaje;
                    }
                    else
                    {
                        obj.Success = false;
                        obj.Mensaje += "<p>* No se han hallado tickets de Jira relacionados.</p>";
                    }
                }
            }
            else
            {
                obj.Success = false;
                obj.Mensaje += objObtencionTickets.Mensaje;
            }

            obj.Data = Array.Empty<object>();

            return JsonConvert.SerializeObject(obj);
        }

        // Actualizo el custom field 2, con el id del ticket de Jira ingresado
        public ObjetoDevuelto ActualizarCustomField(string IdTicketJira, dynamic TicketToJira)
        {
            Dictionary<string, string> Encabezados = new Dictionary<string, string>
                {
                    { "Accept", "application/json" },
                    { "Content-Type", "application/json" },
                    { "TokenJira", VariablesSesion.Configuracion["TokenApiJira"] } // Resta definir clave y valor
                };

            Dictionary<string, string> Parametros = new Dictionary<string, string>();

            ObjetoDevuelto obj = ProcesosBase.ObtenerRespuesta(
                VariablesSesion.Configuracion["UrlApiJira"],
                string.Format(VariablesSesion.Configuracion["ResourceUpdateTicketJira"], IdTicketJira),
                Encabezados,
                Parametros,
                JsonConvert.SerializeObject(TicketToJira),
                Method.Put);

            if (obj.Success)
            {
                RestResponse Respuesta = (RestResponse)obj.Data[0];
                if (Respuesta == null)
                {
                    obj.Success = false;
                    obj.Mensaje = "<p>* No se ha logrado modificar el ticket en Jira.</p>";
                }
            }

            obj.Data = Array.Empty<object>();

            return obj;
        }

        // Inserto ticket en la api de Jira
        public ObjetoDevuelto InsertarTicket(Issue ticketFromApiRedmine)
        {
            TicketToJira ticketToJira = ConvertTicketRedmine(ticketFromApiRedmine);

            Dictionary<string, string> Encabezados = new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "Content-Type", "application/json" },
                { "TokenJira", VariablesSesion.Configuracion["TokenApiJira"] } // Resta definir clave y valor
            };

            Dictionary<string, string> Parametros = new Dictionary<string, string>();

            ObjetoDevuelto obj = ProcesosBase.ObtenerRespuesta(
                VariablesSesion.Configuracion["UrlApiJira"],
                VariablesSesion.Configuracion["ResourceInsertTicketJira"],
                Encabezados,
                Parametros,
                JsonConvert.SerializeObject(ticketToJira),
                Method.Post);

            if (obj.Success)
            {
                RestResponse Respuesta = (RestResponse)obj.Data[0];
                if (Respuesta == null)
                {
                    obj.Success = false;
                    obj.Mensaje = "<p>* No se ha logrado insertar el ticket en Jira.</p>";
                    obj.Data = Array.Empty<object>();
                }
            }
            else obj.Data = Array.Empty<object>();

            return obj;
        }

        // Modifico ticket en la api de Jira
        public ObjetoDevuelto ModificarTicket(Issue ticketFromApiRedmine)
        {
            TicketToJira ticketToJira = ConvertTicketRedmine(ticketFromApiRedmine);
            string IdTicketJira = ticketFromApiRedmine.custom_fields.FirstOrDefault(c => c.id.Equals(2) && c.name.Equals("Reference Number")).value;

            Dictionary<string, string> Encabezados = new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "Content-Type", "application/json" },
                { "TokenJira", VariablesSesion.Configuracion["TokenApiJira"] } // Resta definir clave y valor
            };

            Dictionary<string, string> Parametros = new Dictionary<string, string>();

            ObjetoDevuelto obj = ProcesosBase.ObtenerRespuesta(
                VariablesSesion.Configuracion["UrlApiJira"],
                string.Format(VariablesSesion.Configuracion["ResourceUpdateTicketJira"], IdTicketJira),
                Encabezados,
                Parametros,
                JsonConvert.SerializeObject(ticketToJira),
                Method.Put);

            if (obj.Success)
            {
                RestResponse Respuesta = (RestResponse)obj.Data[0];
                if (Respuesta == null)
                {
                    obj.Success = false;
                    obj.Mensaje = "<p>* No se ha logrado modificar el ticket en Jira.</p>";
                }
            }

            obj.Data = Array.Empty<object>();

            return obj;
        }

        // Convierto el ticket de Redmine en ticket de Jira --> A definir traducción
        private TicketToJira ConvertTicketRedmine(Issue ticketFromApiRedmine)
        {
            TicketToJira ticketToJira = new TicketToJira();

            ticketToJira.properties = new List<GenericTypeDos>
            {
                new GenericTypeDos()
                {
                    key = "subject",
                    value = ticketFromApiRedmine.subject
                },
                new GenericTypeDos()
                {
                    key = "description",
                    value = ticketFromApiRedmine.description
                }
            };

            return ticketToJira;
        }

        // Obtengo un ticket específico mediante api de Jira
        public TicketFromJira ObtenerTicket(string IdTicket, out string MensajeSalida)
        {
            TicketFromJira ticket = new TicketFromJira();
            MensajeSalida = string.Empty;

            Dictionary<string, string> Encabezados = new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "TokenJira", VariablesSesion.Configuracion["TokenApiJira"] }
            };

            Dictionary<string, string> Parametros = new Dictionary<string, string>();

            ObjetoDevuelto obj = ProcesosBase.ObtenerRespuesta(
                VariablesSesion.Configuracion["UrlApiJira"],
                string.Format(VariablesSesion.Configuracion["ResourceGetUpdateTicketJira"], IdTicket),
                Encabezados,
                Parametros,
                string.Empty,
                Method.Get);

            if (obj.Success)
            {
                RestResponse Respuesta = (RestResponse)obj.Data[0];

                if (Respuesta == null) MensajeSalida = "<p>* No se ha logrado obtener el ticket de Jira.</p>";
                else ticket = JsonConvert.DeserializeObject<TicketFromJira>(Respuesta.Content);
            }
            else MensajeSalida = obj.Mensaje;

            return ticket;
        }
    }
}