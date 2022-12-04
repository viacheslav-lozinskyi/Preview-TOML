using Nett;
using Nett.Parser;
using System;

namespace resource.preview
{
    internal class VSPreview : extension.AnyPreview
    {
        protected override void _Execute(atom.Trace context, int level, string url, string file)
        {
            try
            {
                var a_Context = Toml.ReadFile(file);
                if (a_Context != null)
                {
                    foreach (var a_Context1 in a_Context.Rows)
                    {
                        __Execute(context, level, a_Context1.Value, a_Context1.Key);
                    }
                }
            }
            catch (ParseException ex)
            {
                context.
                    SetUrl(file, __GetErrorValue(ex.Message, "Line", ","), __GetErrorValue(ex.Message, "column", ":")).
                    Send(NAME.SOURCE.PREVIEW, NAME.EVENT.EXCEPTION, level, __GetErrorMessage(ex.Message)).
                    SendPreview(NAME.EVENT.EXCEPTION, url);
            }
        }

        private static void __Execute(atom.Trace context, int level, object data, string name)
        {
            if (data == null)
            {
                return;
            }
            if (GetState() == NAME.STATE.CANCEL)
            {
                return;
            }
            if (string.IsNullOrEmpty(name) == false)
            {
                context.
                    SetComment(__GetComment(data), "[[[Data Type]]]").
                    Send(NAME.SOURCE.PREVIEW, __GetType(data), level, name, __GetValue(data));
            }
            if (data is TomlTable)
            {
                foreach (var a_Context in (data as TomlTable).Rows)
                {
                    __Execute(context, level + 1, a_Context.Value, a_Context.Key);
                }
            }
            if (data is TomlArray)
            {
                var a_Index = 0;
                foreach (var a_Context in (data as TomlArray).Items)
                {
                    a_Index++;
                    __Execute(context, level + 1, a_Context.UntypedValue, "[" + a_Index.ToString() + "]");
                }
            }
            if (data is TomlValue[])
            {
                var a_Index = 0;
                foreach (var a_Context in (data as TomlValue[]))
                {
                    a_Index++;
                    __Execute(context, level + 1, a_Context.UntypedValue, "[" + a_Index.ToString() + "]");
                }
            }
        }

        private static string __GetErrorMessage(string data)
        {
            var a_Index = data.IndexOf(":");
            if (a_Index > 0)
            {
                return data.Substring(a_Index + 1);
            }
            return data;
        }

        private static int __GetErrorValue(string data, string begin, string end)
        {
            var a_Index1 = data.IndexOf(begin);
            var a_Index2 = data.IndexOf(end);
            if ((a_Index1 >= 0) && (a_Index2 >= 0) && (a_Index1 < a_Index2))
            {
                var a_Context = data.Substring(a_Index1 + begin.Length, a_Index2 - a_Index1 - begin.Length).Trim();
                var a_Result = 0.0;
                if ((string.IsNullOrEmpty(a_Context) == false) && double.TryParse(a_Context, out a_Result))
                {
                    return (int)a_Result;
                }
            }
            return 0;
        }

        private static string __GetValue(object data)
        {
            if (data is TomlArray)
            {
                return "";
            }
            if (data is TomlObject)
            {
                return GetFinalText((data as TomlObject).ToString());
            }
            if (data is TomlValue[])
            {
                return "";
            }
            return (data == null) ? "" : data.ToString();
        }

        private static string __GetComment(object data)
        {
            if (data is TomlValue[])
            {
                return "[[[Array]]]";
            }
            if (data is TomlObject)
            {
                var a_Result = (data as TomlObject).TomlType.ToString();
                {
                    if (a_Result == "Array") return "[[[Array]]]";
                    if (a_Result == "DateTime") return "[[[Time]]]";
                    if (a_Result == "Float") return "[[[Float]]]";
                    if (a_Result == "String") return "[[[String]]]";
                    if (a_Result == "Int") return "[[[Integer]]]";
                    if (a_Result == "Table") return "[[[Object]]]";
                    if (a_Result == "Bool") return "[[[Boolean]]]";
                }
                return a_Result;
            }
            {
                if (data is DateTime) return "[[[Time]]]";
                if (data is Double) return "[[[Float]]]";
                if (data is String) return "[[[String]]]";
                if (data is Int32) return "[[[Integer]]]";
                if (data is Int64) return "[[[Integer]]]";
                if (data is Boolean) return "[[[Boolean]]]";
            }
            return "";
        }

        private static string __GetType(object data)
        {
            if ((data is TomlTable) || (data is TomlArray) || (data is TomlValue[]))
            {
                return NAME.EVENT.PARAMETER;
            }
            return NAME.EVENT.PARAMETER;
        }
    };
}
