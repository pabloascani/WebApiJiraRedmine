using System.Collections.Generic;

namespace WebApiJiraRedmineBiblioteca.Data
{
    public class ListadoTicketsJira
    {
        public string expand { get; set; } = string.Empty;
        public int startAt { get; set; } = 0;
        public int maxResults { get; set; } = 0;
        public int total { get; set; } = 0;
        public List<TicketFromJira> issues { get; set; } = new List<TicketFromJira>();
        public List<string> warningMessages { get; set; } = new List<string>();
    }

    public class TicketFromJira
    {
        public string id { get; set; } = string.Empty;
        public string self { get; set; } = string.Empty;
        public string key { get; set; } = string.Empty;
        public Field fields { get; set; } = new Field();
    }

    public class ResponseTicketCreatedJira
    {
        public string id { get; set; } = string.Empty;
        public string self { get; set; } = string.Empty;
        public string key { get; set; } = string.Empty;
        public Transition transition { get; set; } = new Transition();
    }

    public class Transition
    {
        public int status { get; set; } = 0;
        public ErrorCollection errorCollection { get; set; } = new ErrorCollection();
    }

    public class ErrorCollection
    {
        public List<string> errorMessages = new List<string>();
    }

    public class Field
    {
        public Watcher watcher { get; set; } = new Watcher();
        public List<AttachmentItem> attachment { get; set; } = new List<AttachmentItem>();
        public List<SubTaskItem> subtasks { get; set; } = new List<SubTaskItem>();
        public Description description { get; set; } = new Description();
        public Project project { get; set; } = new Project();
        public List<CommentItem> comment { get; set; } = new List<CommentItem>();
        public List<IssueLinkItem> issuelinks { get; set; } = new List<IssueLinkItem>();
        public List<WorklogItem> worklog { get; set; } = new List<WorklogItem>();
        public int updated { get; set; } = 0;
        public TimeTracking timetracking { get; set; } = new TimeTracking();
    }

    public class Watcher
    {
        public string self { get; set; } = string.Empty;
        public bool isWatching { get; set; } = false;
        public int watchCount { get; set; } = 0;
        public List<WatchItem> watchers { get; set; } = new List<WatchItem>();
    }

    public class WatchItem
    {
        public string self { get; set; } = string.Empty;
        public string accountId { get; set; } = string.Empty;
        public string displayName { get; set; } = string.Empty;
        public bool active { get; set; } = false;
    }

    public class AttachmentItem
    {
        public int id { get; set; } = 0;
        public string self { get; set; } = string.Empty;
        public string filename { get; set; } = string.Empty;
        public AuthorJira author { get; set; } = new AuthorJira();
        public string created { get; set; } = string.Empty;
        public int size { get; set; } = 0;
        public string mimeType { get; set; } = string.Empty;
        public string content { get; set; } = string.Empty;
        public string thumbnail { get; set; } = string.Empty;
    }

    public class AuthorJira
    {
        public string self { get; set; } = string.Empty;
        public string key { get; set; } = string.Empty;
        public string accountId { get; set; } = string.Empty;
        public string accountType { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string displayName { get; set; } = string.Empty;
        public bool active { get; set; } = false;
    }

    public class SubTaskItem
    {
        public string id { get; set; } = string.Empty;
        public TypeSubTask type { get; set; } = new TypeSubTask();
        public OutwardIssue outwardIssue { get; set; } = new OutwardIssue();
    }

    public class TypeSubTask
    {
        public string id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string inward { get; set; } = string.Empty;
        public string outward { get; set; } = string.Empty;
    }

    public class OutwardIssue
    {
        public string id { get; set; } = string.Empty;
        public string key { get; set; } = string.Empty;
        public string self { get; set; } = string.Empty;
        public FieldItem fields { get; set; } = new FieldItem();
    }

    public class FieldItem
    {
        public StatusItem status { get; set; } = new StatusItem();
    }

    public class StatusItem
    {
        public string iconUrl { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
    }

    public class Description
    {
        public string type { get; set; } = string.Empty;
        public int version { get; set; } = 0;
        public List<ContentItem> content { get; set; } = new List<ContentItem>();
    }

    public class ContentItem
    {
        public string type { get; set; } = string.Empty;
        public List<SubContentItem> content { get; set; } = new List<SubContentItem>();
    }

    public class SubContentItem
    {
        public string type { get; set; } = string.Empty;
        public string text { get; set; } = string.Empty;
    }

    public class Project
    {
        public string self { get; set; } = string.Empty;
        public string id { get; set; } = string.Empty;
        public string key { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public ProjectCategory projectCategory { get; set; } = new ProjectCategory();
        public bool simplified { get; set; } = false;
        public string style { get; set; } = string.Empty;
        public Insight insight { get; set; } = new Insight();
    }

    public class ProjectCategory
    {
        public string self { get; set; } = string.Empty;
        public string id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
    }

    public class Insight
    {
        public int totalIssueCount { get; set; } = 0;
        public string lastIssueUpdateTime { get; set; } = string.Empty;
    }

    public class CommentItem
    {
        public string self { get; set; } = string.Empty;
        public string id { get; set; } = string.Empty;
        public AuthorJira author { get; set; } = new AuthorJira();
        public BodyComment body { get; set; } = new BodyComment();
        public AuthorJira updateAuthor { get; set; } = new AuthorJira();
        public string created { get; set; } = string.Empty;
        public string updated { get; set; } = string.Empty;
        public VisibilityItem visibility { get; set; } = new VisibilityItem();
    }

    public class BodyComment
    {
        public string type { get; set; } = string.Empty;
        public int version { get; set; } = 0;
        public List<ContentItem> content { get; set; } = new List<ContentItem>();
    }

    public class VisibilityItem
    {
        public string type { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
        public string identifier { get; set; } = string.Empty;
    }

    public class IssueLinkItem
    {
        public string id { get; set; } = string.Empty;
        public Tipo type { get; set; } = new Tipo();
        public OutwardIssue outwardIssue { get; set; } = new OutwardIssue();
        public OutwardIssue inwardIssue { get; set; } = new OutwardIssue();
    }

    public class Tipo
    {
        public string id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string inward { get; set; } = string.Empty;
        public string outward { get; set; } = string.Empty;
    }

    public class WorklogItem
    {
        public string self { get; set; } = string.Empty;
        public AuthorJira author { get; set; } = new AuthorJira();
        public AuthorJira updateAuthor { get; set; } = new AuthorJira();
        public BodyComment comment { get; set; } = new BodyComment();
        public string updated { get; set; } = string.Empty;
        public Visibility visibility { get; set; } = new Visibility();
        public string started { get; set; } = string.Empty;
        public string timeSpent { get; set; } = string.Empty;
        public int timeSpentSeconds { get; set; } = 0;
        public string id { get; set; } = string.Empty;
        public string issueId { get; set; } = string.Empty;
    }

    public class Visibility
    {
        public string type { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
        public string identifier { get; set; } = string.Empty;
    }

    public class TimeTracking
    {
        public string originalEstimate { get; set; } = string.Empty;
        public string remainingEstimate { get; set; } = string.Empty;
        public string timeSpent { get; set; } = string.Empty;
        public int originalEstimateSeconds { get; set; } = 0;
        public int remainingEstimateSeconds { get; set; } = 0;
        public int timeSpentSeconds { get; set; } = 0;
    }

    public class TicketToJira
    {
        public FieldToJira fields { get; set; } = new FieldToJira();
        public HistoryMetadata historyMetadata { get; set; } = new HistoryMetadata();
        public List<GenericTypeDos> properties { get; set; } = new List<GenericTypeDos>();
        public Update update { get; set; } = new Update();
    }

    public class FieldToJira
    {
        public CustomField10000 customfield_10000 { get; set; } = new CustomField10000();
        public int customfield_10010 { get; set; } = 0;
        public string summary { get; set; } = string.Empty;
    }

    public class CustomField10000
    {
        public List<ContentItem> content { get; set; } = new List<ContentItem>();
        public string type { get; set; } = string.Empty;
        public int version { get; set; } = 0;
    }

    public class HistoryMetadata
    {
        public string activityDescription { get; set; } = string.Empty;
        public Actor actor { get; set; } = new Actor();
        public GenericTypeUno cause { get; set; } = new GenericTypeUno();
        public string description { get; set; } = string.Empty;
        public ExtraData extraData { get; set; } = new ExtraData();
        public GenericTypeUno generator { get; set; } = new GenericTypeUno();
        public string type { get; set; } = string.Empty;
    }

    public class Actor
    {
        public string avatarUrl { get; set; } = string.Empty;
        public string displayName { get; set; } = string.Empty;
        public string id { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public string url { get; set; } = string.Empty;
    }

    public class GenericTypeUno
    {
        public string id { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
    }

    public class GenericTypeDos
    {
        public string key { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
    }

    public class GenericTypeTres
    {
        public string set { get; set; } = string.Empty;
    }

    public class GenericTypeCuatro
    {
        public string add { get; set; } = string.Empty;
        public string remove { get; set; } = string.Empty;
    }

    public class ExtraData
    {
        public string Iteration { get; set; } = string.Empty;
        public string Step { get; set; } = string.Empty;
    }

    public class Update
    {
        public List<GenericTypeTres> components { get; set; } = new List<GenericTypeTres>();
        public List<GenericTypeCuatro> labels { get; set; } = new List<GenericTypeCuatro>();
        public List<GenericTypeTres> summary { get; set; } = new List<GenericTypeTres>();
        public List<TimeTrackingItem> timetracking { get; set; } = new List<TimeTrackingItem>();
    }

    public class TimeTrackingItem
    {
        public EditItem edit { get; set; } = new EditItem();
    }

    public class EditItem
    {
        public string originalEstimate { get; set; } = string.Empty;
        public string remainingEstimate { get; set; } = string.Empty;
    }
}