using MicroserviceCommon.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;

namespace FlatteningMiddleware.ResponseShaping
{
    public class AdditionalResponseFieldsConfiguration
    {
        public IList<DataSource> DataSources { get; set; }
        public string Script { get; set; }
        public ScriptFormat ScriptFormat { get; set; }
        public bool RaiseFormat { get; set; }
    }

    public class DataSource
    {
        public string Url { get; set; }
        public string Method { get; set; }
        public bool RequiresAuthentication { get; set; }
        public string Name { get; set; }
    }

    [Enumeration("script-format", "Script Format", "List of available script formats for resolving additional fields")]
    public enum ScriptFormat
    {
        [EnumMember(Value = "javascript")]
        [Description("JavaScript")]
        JavaScript
    }
}
