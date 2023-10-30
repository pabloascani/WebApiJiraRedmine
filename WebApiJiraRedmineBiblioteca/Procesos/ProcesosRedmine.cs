using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using WebApiJiraRedmineBiblioteca.Data;

namespace WebApiJiraRedmineBiblioteca.Procesos
{
    public class ProcesosRedmine
    {
        public string GenerarToken()
        {
            ObjetoDevuelto obj = new();

            try
            {
                var SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(VariablesSesion.Configuracion["Jwt:Key"]));
                var Credentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);

                var Token = new JwtSecurityToken(
                    VariablesSesion.Configuracion["Jwt:Issuer"],
                    VariablesSesion.Configuracion["Jwt:Issuer"],
                    null,
                    expires: DateTime.Now.AddYears(10),
                    signingCredentials: Credentials
                    );

                obj.Data = new object[] { new JwtSecurityTokenHandler().WriteToken(Token) };
            }
            catch (Exception ex)
            {
                obj.Success = false;
                obj.Mensaje = "<p>* " + ex.Message + ".</p>";
            }

            return JsonConvert.SerializeObject(obj);
        }

        // Obtengo todos los tickets de Redmine del proyecto 310, para insertarlos/modificarlos en Jira
        public string ObtenerTicketsRedmine()
        {
            ObjetoDevuelto obj = new();
            RestResponse Respuesta;

            Dictionary<string, string> Encabezados = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json" },
                        { "X-Redmine-API-Key", VariablesSesion.Configuracion["TokenApiRedmine"] }
                    };

            Dictionary<string, string> Parametros = new Dictionary<string, string>();

            ObjetoDevuelto objObtencionTickets = ProcesosBase.ObtenerRespuesta(
                VariablesSesion.Configuracion["UrlRedmine"],
                VariablesSesion.Configuracion["ResourceGetTicketsRedmine"],
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
                    obj.Mensaje += "<p>* No se ha logrado obtener los tickets de Redmine.</p>";
                }
                else if (string.IsNullOrEmpty(Respuesta.Content))
                {
                    obj.Success = false;
                    obj.Mensaje += "<p>* No se han hallado tickets de Redmine relacionados.</p>";
                }
                else
                {
                    ListadoTicketsRedmine listadoTickets = JsonConvert.DeserializeObject<ListadoTicketsRedmine>(Respuesta.Content);

                    if (listadoTickets.issues.Count > 0)
                    {
                        foreach (Issue issue in listadoTickets.issues)
                        {
                            TicketFromApiRedmine ticketFromApiRedmine = ObtenerTicket(issue.id, out string MensajeSalida);

                            if (!string.IsNullOrEmpty(MensajeSalida))
                            {
                                obj.Success = false;
                                obj.Mensaje += MensajeSalida;
                            }
                            else
                            {
                                // Espacio para la definición pendiente de comentarios enviados a Jira
                                List<Journal> Comentarios = ticketFromApiRedmine.issue.journals.Where(j => !string.IsNullOrEmpty(j.notes)).ToList();
                                // Espacio para la definición pendiente de comentarios enviados a Jira

                                string IdTicketJira = issue.custom_fields.FirstOrDefault(c => c.id.Equals(2) && c.name.Equals("Reference Number")).value;
                                bool TicketNuevo = string.IsNullOrEmpty(IdTicketJira) || IdTicketJira.Equals("NA") || !int.TryParse(IdTicketJira, out int IdTicket);
                                ObjetoDevuelto objTicket = TicketNuevo ? new ProcesosJira().InsertarTicket(issue) : new ProcesosJira().ModificarTicket(issue);

                                if (!objTicket.Success)
                                {
                                    obj.Success = false;
                                    obj.Mensaje += objTicket.Mensaje;
                                }
                                else if (TicketNuevo)
                                {
                                    Respuesta = (RestResponse)objTicket.Data[0];
                                    ResponseTicketCreatedJira responseTicketCreatedJira = JsonConvert.DeserializeObject<ResponseTicketCreatedJira>(Respuesta.Content);

                                    if (!responseTicketCreatedJira.transition.status.Equals(200)
                                        || responseTicketCreatedJira.transition.errorCollection.errorMessages.Count > 0)
                                    {
                                        obj.Success = false;
                                        if (responseTicketCreatedJira.transition.errorCollection.errorMessages.Count > 0)
                                            foreach (string errorMessage in responseTicketCreatedJira.transition.errorCollection.errorMessages)
                                                obj.Mensaje += "<p>* " + errorMessage + ".</p>";
                                        else obj.Mensaje += "<p>* No se ha logrado crear el ticket en Jira.</p>";
                                    }
                                    else
                                    {
                                        IdTicketJira = responseTicketCreatedJira.id;

                                        dynamic TicketToRedmine = new ExpandoObject();
                                        dynamic BodyTicket = new ExpandoObject();
                                        BodyTicket.id = issue.id;
                                        BodyTicket.custom_fields = new List<CustomField>();
                                        BodyTicket.custom_fields.Add(new CustomField()
                                        {
                                            id = 2,
                                            name = "Reference Number",
                                            value = IdTicketJira
                                        });
                                        TicketToRedmine.issue = BodyTicket;

                                        ObjetoDevuelto objModifTicket = ActualizarCustomField(TicketToRedmine);
                                        if (!objModifTicket.Success)
                                        {
                                            obj.Success = false;
                                            obj.Mensaje += objModifTicket.Mensaje;
                                        }
                                    }
                                }
                            }
                        }

                        obj.Mensaje = string.IsNullOrEmpty(obj.Mensaje) ? "<p>* Los tickets de Redmine han sido integrados exitosamente.</p>" : obj.Mensaje;
                    }
                    else
                    {
                        obj.Success = false;
                        obj.Mensaje += "<p>* No se han hallado tickets de Redmine relacionados.</p>";
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
        public ObjetoDevuelto ActualizarCustomField(dynamic TicketToRedmine)
        {
            Dictionary<string, string> Encabezados = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "X-Redmine-API-Key", VariablesSesion.Configuracion["TokenApiRedmine"] }
                };

            Dictionary<string, string> Parametros = new Dictionary<string, string>();

            ObjetoDevuelto obj = ProcesosBase.ObtenerRespuesta(
                VariablesSesion.Configuracion["UrlRedmine"],
                string.Format(VariablesSesion.Configuracion["ResourceUpdateTicketRedmine"], TicketToRedmine.issue.id),
                Encabezados,
                Parametros,
                JsonConvert.SerializeObject(TicketToRedmine),
                Method.Put);

            if (obj.Success)
            {
                RestResponse Respuesta = (RestResponse)obj.Data[0];
                if (Respuesta == null)
                {
                    obj.Success = false;
                    obj.Mensaje = "<p>* No se ha logrado modificar el ticket en Redmine.</p>";
                }
            }

            obj.Data = Array.Empty<object>();

            return obj;
        }

        // Inserto ticket mediante api de Redmine
        public ObjetoDevuelto InsertarTicket(TicketFromJira ticketFromJira)
        {
            TicketToApiRedmine ticketToApiRedmine = ConvertTicketJira(ticketFromJira);

            Dictionary<string, string> Encabezados = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "X-Redmine-API-Key", VariablesSesion.Configuracion["TokenApiRedmine"] }
            };

            Dictionary<string, string> Parametros = new Dictionary<string, string>();

            ObjetoDevuelto obj = ProcesosBase.ObtenerRespuesta(
                VariablesSesion.Configuracion["UrlRedmine"],
                VariablesSesion.Configuracion["ResourceInsertTicketRedmine"],
                Encabezados,
                Parametros,
                JsonConvert.SerializeObject(ticketToApiRedmine),
                Method.Post);

            if (obj.Success)
            {
                RestResponse Respuesta = (RestResponse)obj.Data[0];
                if (Respuesta == null)
                {
                    obj.Success = false;
                    obj.Mensaje = "<p>* No se ha logrado insertar el ticket en Redmine.</p>";
                    obj.Data = Array.Empty<object>();
                }
            }
            else obj.Data = Array.Empty<object>();

            return obj;
        }

        // Modifico ticket mediante la api de Redmine
        public ObjetoDevuelto ModificarTicket(TicketFromJira ticketFromJira)
        {
            TicketToApiRedmine ticketToApiRedmine = ConvertTicketJira(ticketFromJira);
            string IdTicketRedmine = ticketFromJira.fields.comment[0].body.content[0].content[0].text;
            //TicketToApiRedmine ticketObtenido = ObtenerTicketRelacionado(ticketFromJira.id, out string MensajeSalida);

            Dictionary<string, string> Encabezados = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "X-Redmine-API-Key", VariablesSesion.Configuracion["TokenApiRedmine"] }
                };

            Dictionary<string, string> Parametros = new Dictionary<string, string>();

            ObjetoDevuelto obj = ProcesosBase.ObtenerRespuesta(
                    VariablesSesion.Configuracion["UrlRedmine"],
                    string.Format(VariablesSesion.Configuracion["ResourceUpdateTicketRedmine"], IdTicketRedmine),
                    Encabezados,
                    Parametros,
                    JsonConvert.SerializeObject(ticketToApiRedmine),
                    Method.Put);

            if (obj.Success)
            {
                RestResponse Respuesta = (RestResponse)obj.Data[0];
                if (Respuesta == null)
                {
                    obj.Success = false;
                    obj.Mensaje = "<p>* No se ha logrado modificar el ticket en Redmine.</p>";
                }
            }

            obj.Data = Array.Empty<object>();

            return obj;
        }

        // Convierto el ticket de Jira, a ticket to api Redmine
        private TicketToApiRedmine ConvertTicketJira(TicketFromJira ticketFromJira)
        {
            TicketToApiRedmine ticketToApiRedmine = new TicketToApiRedmine();

            ticketToApiRedmine.issue.custom_fields = new List<CustomField>
            {
                new CustomField()
                {
                    id = 2,
                    name = "Reference Number",
                    value = ticketFromJira.id
                }
            };
            ticketToApiRedmine.issue.project_id = Convert.ToInt32(VariablesSesion.Configuracion["IdProyectoBinter"]);
            ticketToApiRedmine.issue.subject = ticketFromJira.fields.description.content[0].content[0].text;

            return ticketToApiRedmine;
        }

        // Obtengo un ticket específico mediante api de Redmine
        public TicketFromApiRedmine ObtenerTicket(int IdTicket, out string MensajeSalida)
        {
            TicketFromApiRedmine ticket = new TicketFromApiRedmine();
            MensajeSalida = string.Empty;

            Dictionary<string, string> Encabezados = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "X-Redmine-API-Key", VariablesSesion.Configuracion["TokenApiRedmine"] }
            };

            Dictionary<string, string> Parametros = new Dictionary<string, string>();

            ObjetoDevuelto obj = ProcesosBase.ObtenerRespuesta(
                VariablesSesion.Configuracion["UrlRedmine"],
                string.Format(VariablesSesion.Configuracion["ResourceGetTicketRedmine"], IdTicket),
                Encabezados,
                Parametros,
                string.Empty,
                Method.Get);

            if (obj.Success)
            {
                RestResponse Respuesta = (RestResponse)obj.Data[0];

                if (Respuesta == null) MensajeSalida = "<p>* No se ha logrado obtener el ticket de Redmine.</p>";
                else ticket = JsonConvert.DeserializeObject<TicketFromApiRedmine>(Respuesta.Content);
            }
            else MensajeSalida = obj.Mensaje;

            return ticket;
        }

        // Obtengo el ticket de Rdmine relacionado, a un ticket de Jira específico
        public TicketFromApiRedmine ObtenerTicketRelacionado(string IdTicketJira, out string MensajeSalida)
        {
            TicketFromApiRedmine ticket = new TicketFromApiRedmine();
            MensajeSalida = string.Empty;

            Dictionary<string, string> Encabezados = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "X-Redmine-API-Key", VariablesSesion.Configuracion["TokenApiRedmine"] }
            };

            Dictionary<string, string> Parametros = new Dictionary<string, string>();

            ObjetoDevuelto obj = ProcesosBase.ObtenerRespuesta(
                VariablesSesion.Configuracion["UrlRedmine"],
                string.Format(VariablesSesion.Configuracion["ResourceGetRelationedTicket"], IdTicketJira),
                Encabezados,
                Parametros,
                string.Empty,
                Method.Get);

            if (obj.Success)
            {
                RestResponse Respuesta = (RestResponse)obj.Data[0];

                if (Respuesta == null) MensajeSalida = "<p>* No se ha logrado obtener el ticket de Redmine relacionado.</p>";
                else
                {
                    ListadoTicketsRedmine tickets = JsonConvert.DeserializeObject<ListadoTicketsRedmine>(Respuesta.Content);
                    Issue issue = tickets.issues.FirstOrDefault(t => t.custom_fields.Contains(new CustomField() { id = 2, name = "Reference Number", value = IdTicketJira }));
                    if (issue == null) MensajeSalida = "<p>* No se ha logrado obtener el ticket de Redmine relacionado.</p>";
                    else ticket.issue = issue;
                }
            }
            else MensajeSalida = obj.Mensaje;

            return ticket;
        }
    }
}