using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MunicipalReportingApp.Models
{
  
    public enum RequestStatus
    {
        Submitted = 0,
        Assigned = 1,
        InProgress = 2,
        Resolved = 3
    }

    
    public enum RequestPriority
    {
        Critical = 1,
        High = 2,
        Medium = 3,
        Low = 4
    }

    public class StatusUpdate
    {
        public RequestStatus Status { get; set; }
        public DateTime When { get; set; }
        public string Note { get; set; } = string.Empty;
    }


    public class ServiceRequest
    {
      
        public int Number { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        public string Area { get; set; } = string.Empty;

        public string Street { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public RequestPriority Priority { get; set; } = RequestPriority.Medium;
        public RequestStatus Status { get; set; } = RequestStatus.Submitted;

        public DateTime Submitted { get; set; } = DateTime.Now;
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public string AssignedTeam { get; set; } = string.Empty;

       
        public int SourceIssueId { get; set; }

        public List<StatusUpdate> History { get; set; } = new();

        
        [JsonIgnore]
        public string Reference => "SR-" + Number;

    
        [JsonIgnore]
        public bool IsOpen => Status != RequestStatus.Resolved;

        public override string ToString()
        {
            return $"{Reference} {Title} ({Status})";
        }
    }
}