using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace WebApiJiraRedmineBiblioteca.Data
{
    public static class VariablesSesion
    {
        public static IConfiguration Configuracion = null;
    }

    public class ListadoTicketsRedmine
    {
        public List<Issue> issues { get; set; } = new List<Issue>();
        public int total_count { get; set; } = 0;
        public int offset { get; set; } = 0;
        public int limit { get; set; } = 0;
    }

    public class TicketFromApiRedmine
    {
        public Issue issue { get; set; } = new Issue();
    }

    public class Issue
    {
        public int id { get; set; } = 0;
        public BasicEntity project { get; set; } = new BasicEntity();
        public BasicEntity tracker { get; set; } = new BasicEntity();
        public BasicEntity status { get; set; } = new BasicEntity();
        public BasicEntity priority { get; set; } = new BasicEntity();
        public BasicEntity author { get; set; } = new BasicEntity();
        public BasicEntity category { get; set; } = new BasicEntity();
        public string subject { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string start_date { get; set; } = string.Empty;
        public string due_date { get; set; } = string.Empty;
        public int? done_ratio { get; set; } = 0;
        public bool is_private { get; set; } = false;
        public double? estimated_hours { get; set; } = 0;
        public List<CustomField> custom_fields { get; set; } = new List<CustomField>();
        public string created_on { get; set; } = string.Empty;
        public string updated_on { get; set; } = string.Empty;
        public string closed_on { get; set; } = string.Empty;
        public List<Journal> journals { get; set; } = new List<Journal>();
    }

    public class BasicEntity
    {
        public int id { get; set; } = 0;
        public string name { get; set; } = string.Empty;
    }

    public class CustomField
    {
        public int id { get; set; } = 0;
        public string name { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
    }

    public class Journal
    {
        public int id { get; set; } = 0;
        public string notes { get; set; } = string.Empty;
        public string created_on { get; set; } = string.Empty;
    }

    public class TicketToApiRedmine
    {
        public BodyTicketToApiRedmine issue { get; set; } = new BodyTicketToApiRedmine();
    }

    public class BodyTicketToApiRedmine
    {
        public int id { get; set; } = 0;
        public int project_id { get; set; } = 0;
        public int tracker_id { get; set; } = 0;
        public int status_id { get; set; } = 0;
        public int priority_id { get; set; } = 0;
        public string subject { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public int category_id { get; set; } = 0;
        public int fixed_version_id { get; set; } = 0;
        public int assigned_to_id { get; set; } = 0;
        public int parent_issue_id { get; set; } = 0;
        public List<CustomField> custom_fields { get; set; } = new List<CustomField>();
        public List<int> watcher_user_ids { get; set; } = new List<int>();
        public bool is_private { get; set; } = false;
        public double estimated_hours { get; set; } = 0;
        public string notes { get; set; } = string.Empty;
        public bool private_notes { get; set; } = false;
    }

    // Clase exclusiva para devolución de procesos
    public class ObjetoDevuelto
    {
        public bool Success { get; set; } = true;
        public string Mensaje { get; set; } = string.Empty;
        public object[] Data { get; set; } = Array.Empty<object>();
    }
}