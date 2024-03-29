﻿namespace HealthForms.Api.Core.Models.SessionMember;

public class SessionMemberFormResponse
{
    public string? FormId { get; set; }
    public string? FormName { get; set; }
    public string? FormStatus { get; set; }
    public bool IsComplete { get; set; }
    public bool IsRequired { get; set; }
}