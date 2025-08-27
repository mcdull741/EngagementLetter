using System;

namespace EngagementLetter.Models.Base
{
    public class BaseEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }
}