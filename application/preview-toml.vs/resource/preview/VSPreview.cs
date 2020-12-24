
using Nett;
using Nett.Parser;
using System;

namespace resource.preview
{
    internal class VSPreview : cartridge.AnyPreview
    {
        protected override void _Execute(atom.Trace context, string url, int level)
        {
            try
            {
                var a_Context = Toml.ReadFile(url);
                if (a_Context != null)
                {
                    foreach (var a_Context1 in a_Context.Rows)
                    {
                        __Execute(a_Context1.Value, level, context, a_Context1.Key);
                    }
                }
            }
            catch (ParseException ex)
            {
                context.
                    SetUrl(url, "").
                    SetUrlLine(__GetErrorValue(ex.Message, "Line", ",")).
                    SetUrlPosition(__GetErrorValue(ex.Message, "column", ":")).
                    Send(NAME.SOURCE.PREVIEW, NAME.TYPE.ERROR, level, __GetErrorMessage(ex.Message));
            }
        }

        private static void __Execute(object node, int level, atom.Trace context, string name)
        {
            if (node == null)
            {
                return;
            }
            if (GetState() == STATE.CANCEL)
            {
                return;
            }
            if (string.IsNullOrEmpty(name) == false)
            {
                context.
                    SetComment(__GetComment(node), "[[Data type]]").
                    Send(NAME.SOURCE.PREVIEW, __GetType(node), level, name, __GetValue(node));
            }
            if (node is TomlTable)
            {
                foreach (var a_Context in (node as TomlTable).Rows)
                {
                    __Execute(a_Context.Value, level + 1, context, a_Context.Key);
                }
            }
            if (node is TomlArray)
            {
                var a_Index = 0;
                foreach (var a_Context in (node as TomlArray).Items)
                {
                    a_Index++;
                    __Execute(a_Context.UntypedValue, level + 1, context, "[" + a_Index.ToString() + "]");
                }
            }
            if (node is TomlValue[])
            {
                var a_Index = 0;
                foreach (var a_Context in (node as TomlValue[]))
                {
                    a_Index++;
                    __Execute(a_Context.UntypedValue, level + 1, context, "[" + a_Index.ToString() + "]");
                }
            }
        }

        private static string __GetErrorMessage(string value)
        {
            var a_Index = value.IndexOf(":");
            if (a_Index > 0)
            {
                return value.Substring(a_Index + 1);
            }
            return value;
        }

        private static int __GetErrorValue(string value, string begin, string end)
        {
            var a_Index1 = value.IndexOf(begin);
            var a_Index2 = value.IndexOf(end);
            if ((a_Index1 >= 0) && (a_Index2 >= 0) && (a_Index1 < a_Index2))
            {
                var a_Context = value.Substring(a_Index1 + begin.Length, a_Index2 - a_Index1 - begin.Length).Trim();
                var a_Result = 0.0;
                if ((string.IsNullOrEmpty(a_Context) == false) && double.TryParse(a_Context, out a_Result))
                {
                    return (int)a_Result;
                }
            }
            return 0;
        }

        private static string __GetValue(object value)
        {
            if (value is TomlArray)
            {
                return "";
            }
            if (value is TomlObject)
            {
                return GetCleanString((value as TomlObject).ToString());
            }
            if (value is TomlValue[])
            {
                return "";
            }
            return (value == null) ? "" : value.ToString();
        }

        private static string __GetComment(object value)
        {
            if (value is TomlValue[])
            {
                return "[[Array]]";
            }
            if (value is TomlObject)
            {
                var a_Result = (value as TomlObject).TomlType.ToString();
                {
                    if (a_Result == "Array") return "[[Array]]";
                    if (a_Result == "DateTime") return "[[Time]]";
                    if (a_Result == "Float") return "[[Float]]";
                    if (a_Result == "String") return "[[String]]";
                    if (a_Result == "Int") return "[[Integer]]";
                    if (a_Result == "Table") return "[[Object]]";
                    if (a_Result == "Bool") return "[[Boolean]]";
                }
                return a_Result;
            }
            {
                if (value is DateTime) return "[[Time]]";
                if (value is Double) return "[[Float]]";
                if (value is String) return "[[String]]";
                if (value is Int32) return "[[Integer]]";
                if (value is Int64) return "[[Integer]]";
                if (value is Boolean) return "[[Boolean]]";
            }
            return "";
        }

        private static string __GetType(object value)
        {
            if ((value is TomlTable) || (value is TomlArray) || (value is TomlValue[]))
            {
                return NAME.TYPE.INFO;
            }
            return NAME.TYPE.VARIABLE;
        }
    };
}
