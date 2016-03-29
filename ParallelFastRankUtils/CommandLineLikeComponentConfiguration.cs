namespace ParallelFastRank
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using FastRank;

    /// <summary>
    /// The general component configuration implementation that leverage command line parser
    /// to set parameter values, so that components can have unified way to define its 
    /// configuration class/object.
    /// </summary>
    public abstract class CommandLineLikeComponentConfiguration : ComponentConfiguration.ComponentConfigurationBase
    {
        public override void SetParameters(IDictionary<string, string> parameters)
        {
            string[] args = parameters.SelectMany(kvp => new string[] { "/" + kvp.Key, kvp.Value }).ToArray();

            Microsoft.TMSN.CommandLine.Parser.ParseArguments(args, this, ErrorReporter);
        }

        /// <summary>
        /// Output the parameters and their values
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            Type t = GetType();
            FieldInfo[] fields = t.GetFields();
            foreach (FieldInfo field in fields)
            {
                sb.AppendFormat("{0} = ", field.Name);

                object value = field.GetValue(this);
                if (field.FieldType.IsArray)
                {
                    sb.Append("[ ");
                    bool comma = false;
                    foreach (var item in (Array)value)
                    {
                        if (comma)
                        {
                            sb.Append(", ");
                        }
                        else
                        {
                            comma = true;
                        }

                        sb.Append(item);
                    }

                    sb.AppendLine(" ]");
                }
                else if (value == null)
                {
                    sb.AppendLine("null");
                }
                else
                {
                    sb.AppendFormat("{0}\n", value);
                }
            }

            return sb.ToString();
        }

        private void ErrorReporter(string errorMessage)
        {
            StaticRuntimeContext.Stderr.WriteLine(
                "component {0} configuration error: {1}",
                GetComponentName(),
                errorMessage);
        }
    }
}
