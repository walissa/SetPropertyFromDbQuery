using System;
using BizTalkComponents.Utils;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using IComponent = Microsoft.BizTalk.Component.Interop.IComponent;
using System.ComponentModel;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using System.Text.RegularExpressions;

namespace BizTalkComponents.PipelineComponents.SetPropertyFromDbQuery
{
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [ComponentCategory(CategoryTypes.CATID_Any)]
    [System.Runtime.InteropServices.Guid("a040f5b6-6fbf-499d-9c92-596d1601c691")]
    public partial class SetPropertyFromDBQuery : IBaseComponent, IComponent, IComponentUI, IPersistPropertyBag
    {
        [RequiredRuntime]
        [DisplayName("Connection")]
        [Description("Refers to the connection key in the ConnectionStrings section in the config file, or system environment key that holds the connection string, otherwise the connectionstring to be used.")]
        public string Connection { get; set; }

        [RequiredRuntime]
        [DisplayName("Query")]
        [Description("The query to execute that returns the value")]
        public string Query { get; set; }

        [RequiredRuntime]
        [DisplayName("Destination Property")]
        [Description("The destination property to write the returned value to (e.g. http://namespace#property)")]
        public string DestinationProperty { get; set; }

        [RequiredRuntime]
        [DisplayName("No Promotion")]
        [Description("Determines wether the property should be promoted, the default value is false, meaning the property will be promoted.")]
        public bool NoPromotion { get; set; }

        [RequiredRuntime]
        [DisplayName("Throw Exception")]
        [Description("Specifies if the component should throw an exception if no records returned, set True to throw an exception.")]
        public bool ThrowException { get; set; }

        [DisplayName("Returned Field")]
        [Description("The field that holds the returned value.")]
        public string ReturnedField { get; set; }

        public IBaseMessage Execute(IPipelineContext pContext, IBaseMessage pInMsg)
        {
            if (Disabled)
            {
                return pInMsg;
            }

            string errorMessage;
            if (!Validate(out errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }
            object ret = null;
            string connStr = string.Empty;
            if (ConfigurationManager.ConnectionStrings[Connection] != null)
            {
                connStr = ConfigurationManager.ConnectionStrings[Connection].ConnectionString;
            }
            else if (Environment.GetEnvironmentVariable(Connection, EnvironmentVariableTarget.Machine) != null)
            {
                connStr = Environment.GetEnvironmentVariable(Connection, EnvironmentVariableTarget.Machine);
            }
            else
            {
                connStr = Connection;
            }
            string evaluatedQuery = Query;
            var m = Regex.Match(evaluatedQuery, "{(?<property>.+?#\\w+?)}", RegexOptions.Compiled);
            while (m.Success)
            {
                var propValue = pInMsg.Context.Read(new ContextProperty(m.Groups["property"].Value));
                string sValue = ConvertToString(propValue);
                evaluatedQuery = evaluatedQuery.Replace(m.Value, sValue);
                m = m.NextMatch();
            }

            using (var conn = new SqlConnection(connStr))
            {
                var cmd = new SqlCommand(evaluatedQuery, conn);
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Open();
                }
                var reader = cmd.ExecuteReader();
                if (!reader.HasRows & ThrowException)
                {
                    throw new Exception("The executed query returned no records!");
                }
                if (reader.Read())
                {
                    int fieldIndex = 0;
                    if (!string.IsNullOrEmpty(ReturnedField))
                    {
                        for (fieldIndex = 0; fieldIndex < reader.FieldCount - 1; fieldIndex++)
                        {
                            if (ReturnedField.Equals(reader.GetName(fieldIndex), StringComparison.CurrentCultureIgnoreCase))
                                break;
                        }
                    }
                    if (fieldIndex==reader.FieldCount & ThrowException)
                    {
                        throw new Exception("The specified ReturnedField does not existed in the query result!");
                    }
                    ret = reader.GetValue(fieldIndex);
                }
                reader.Close();
            }
            var ctxProp = new ContextProperty(this.DestinationProperty);
            if (NoPromotion)
            {
                pInMsg.Context.Write(ctxProp, ret);
            }
            else
            {
                pInMsg.Context.Promote(ctxProp, ret);
            }
            return pInMsg;
        }
        private string ConvertToString(object propertyValue)
        {
            if (propertyValue == null)
                return "null";
            else if (propertyValue is string)
                return $"'{(propertyValue as string).Replace("'", "''")}'";
            else if (propertyValue is DateTime)
                return $"'{propertyValue as DateTime?:yyyy-MM-ddTHH:mm:ss.fffffff}'";
            else
                return propertyValue.ToString();
        }
    }
}