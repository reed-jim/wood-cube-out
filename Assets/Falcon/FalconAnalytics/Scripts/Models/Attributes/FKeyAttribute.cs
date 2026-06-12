using System;

namespace Falcon.FalconAnalytics.Scripts.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class FKeyAttribute : Attribute
    {
        public bool Ignore { get; set; } = false;
        public string Name { get; set; } = null;
        public bool RemoveIfNull { get; set; } = false;
    }
}