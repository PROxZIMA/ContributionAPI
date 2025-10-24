using System.Text.Json.Serialization;

namespace Contribution.GitLab.Models;

// GitLab uses REST API for events, not GraphQL
// Endpoint: GET /users/:username/events

public class GitLabEvent
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("project_id")]
    public long? ProjectId { get; set; }

    [JsonPropertyName("action_name")]
    public string? ActionName { get; set; }

    [JsonPropertyName("target_id")]
    public long? TargetId { get; set; }

    [JsonPropertyName("target_iid")]
    public long? TargetIid { get; set; }

    [JsonPropertyName("target_type")]
    public string? TargetType { get; set; }

    [JsonPropertyName("author_id")]
    public long? AuthorId { get; set; }

    [JsonPropertyName("target_title")]
    public string? TargetTitle { get; set; }

    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }

    [JsonPropertyName("author")]
    public GitLabEventAuthor? Author { get; set; }

    [JsonPropertyName("author_username")]
    public string? AuthorUsername { get; set; }

    [JsonPropertyName("push_data")]
    public GitLabPushData? PushData { get; set; }

    [JsonPropertyName("note")]
    public GitLabNote? Note { get; set; }
}

public class GitLabEventAuthor
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }

    [JsonPropertyName("web_url")]
    public string? WebUrl { get; set; }
}

public class GitLabPushData
{
    [JsonPropertyName("commit_count")]
    public int CommitCount { get; set; }

    [JsonPropertyName("action")]
    public string? Action { get; set; }

    [JsonPropertyName("ref_type")]
    public string? RefType { get; set; }

    [JsonPropertyName("commit_from")]
    public string? CommitFrom { get; set; }

    [JsonPropertyName("commit_to")]
    public string? CommitTo { get; set; }

    [JsonPropertyName("ref")]
    public string? Ref { get; set; }

    [JsonPropertyName("commit_title")]
    public string? CommitTitle { get; set; }
}

public class GitLabNote
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }

    [JsonPropertyName("noteable_type")]
    public string? NoteableType { get; set; }
}
