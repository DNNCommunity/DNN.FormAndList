using System.IO;

namespace DotNetNuke.Modules.UserDefinedTable.CSV
{
    public class CSVWriter
    {
        public static void WriteCSV(string[] data, TextWriter  sw, string delimiter)
        {
            for (var i = 0; i <= data.Length - 1; i++)
            {
                sw.Write(EncodeString(data[i], delimiter));
                //Not last, need a comma after
                if (i != data.Length - 1)
                {
                    sw.Write(delimiter);
                }
            }
            sw.WriteLine("");
            sw.Flush();
        }

        static string EncodeString(string str, string delimiter)
        {
            string escaped;
            var commaPos = str.IndexOf(delimiter);
            var returnPos = str.IndexOf('\r');
            var quotePos = str.IndexOf('\u0022');


            //there are both commas and quotes in string, need to escape
            if (quotePos >= 0)
            {
                //firstly, escape quotes
                escaped = str.Replace("\u0022", "\u0022\u0022");
            }
            else
            {
                escaped = str;
            }

            //there is comma or quote in string, need to escape
            if (commaPos >= 0 || quotePos >= 0 || returnPos >= 0)
            {
                escaped = ('\u0022' + escaped + '\u0022');
            }

            return escaped;
        }
    }
}