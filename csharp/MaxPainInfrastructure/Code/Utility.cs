using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace MaxPainInfrastructure.Code
{
    public class Utility
    {
        public static string GetEmbeddedFile(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames()
              .Single(str => str.EndsWith(filename));

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        #region Regex
        public static bool RegexTest(string data, string pattern)
        {
            return Regex.IsMatch(data, pattern);
        }
        #endregion

        #region is functions
        public static bool IsSPX(string ticker)
        {
            return ticker.Equals("SPX", StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        #region Date
        public static DateTime YMDToDate(int value)
        {
            string d = value.ToString();
            return new DateTime(int.Parse(d.Substring(0, 4)), int.Parse(d.Substring(4, 2)), int.Parse(d.Substring(6, 2)));
        }

        public static int DateToYMD(DateTime d)
        {
            return int.Parse(d.ToString("yyyyMMdd"));
        }

        public static DateTime NextFriday()
        {
            return NextFriday(DateTime.Now);
        }

        public static DateTime NextFriday(DateTime dt)
        {
            int numDays = ((int)DayOfWeek.Friday - (int)dt.DayOfWeek + 7) % 7;
            return dt.AddDays(numDays).Date;
        }

        public static bool IsThirdFriday(DateTime dt)
        {
            DateTime firstOfMonth = new DateTime(dt.Year, dt.Month, 1);
            int firstFriday = ((int)DayOfWeek.Friday - (int)firstOfMonth.DayOfWeek + 7) % 7;
            DateTime thirdFriday = firstOfMonth.AddDays(firstFriday + 14);
            return dt.Date == thirdFriday.Date;
        }

        public static DateTime GMTToEST(DateTime utc)
        {
            TimeZoneInfo tz = GetESTTimeZoneInfo();
            return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
        }

        public static DateTime CurrentDateEST()
        {
            return GMTToEST(DateTime.UtcNow);
        }

        public static DateTime ESTToGMT(DateTime est)
        {
            TimeZoneInfo tz = GetESTTimeZoneInfo();
            return TimeZoneInfo.ConvertTimeToUtc(est, tz);
        }

        private static TimeZoneInfo GetESTTimeZoneInfo()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
            }
            throw new Exception("Utility.cs GetESTTimeZoneInfo could not set Eastern Standard TimeZone");
        }

        public static DateTime ToSmallDateTime(DateTime dt)
        {
            if (dt < System.Data.SqlTypes.SqlDateTime.MinValue.Value) return System.Data.SqlTypes.SqlDateTime.MinValue.Value;
            if (dt > System.Data.SqlTypes.SqlDateTime.MaxValue.Value) return System.Data.SqlTypes.SqlDateTime.MaxValue.Value;
            return dt;
        }

        public static DateTime UnixTimestampToDateTime(long unixTime)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(unixTime).UtcDateTime;
        }

        public static double DateTimeToUnixTimestamp(string dateTime)
        {
            return DateTimeToUnixTimestamp(Convert.ToDateTime(dateTime));
        }

        public static long DateTimeToUnixTimestamp(DateTime dt)
        {
            return ((DateTimeOffset)dt).ToUnixTimeSeconds();
        }

        private DateTime LocalToEST(DateTime localTime)
        {
            DateTime dt = DateTime.SpecifyKind(localTime, DateTimeKind.Unspecified);
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(dt, easternZone);
        }
        #endregion

        #region web
        public static string GetPhysicalPath(string virtualPath)
        {
            return virtualPath;
        }

        public static string DumpCollection(System.Collections.Specialized.NameValueCollection c)
        {
            var sb = new System.Text.StringBuilder("<div><table border=\"1\" cellpadding=\"0\" cellspacing=\"0\">");
            foreach (string key in c)
            {
                sb.AppendFormat("<tr><td>{0}</td><td>{1}</td></tr>", key, c[key]);
            }
            sb.Append("</table></div>");
            return sb.ToString();
        }
        #endregion

        #region IO
        public static bool DoesFileExist(string strPhysicalPath)
        {
            return System.IO.File.Exists(strPhysicalPath);
        }

        public static string ReadFile(string physicalFile)
        {
            return System.IO.File.ReadAllText(physicalFile);
        }

        public static void SaveFile(string physicalFile, string contents)
        {
            System.IO.File.WriteAllText(physicalFile, contents);
        }

        public static void AppendFile(string physicalFile, string contents)
        {
            using (var sw = new StreamWriter(physicalFile, true))
            {
                sw.WriteLine(contents);
            }
        }

        public static void CreateFolder(string physicalPath)
        {
            var di = new DirectoryInfo(physicalPath);
            if (!di.Exists)
            {
                di.Create();
            }
        }
        #endregion

        #region string
        public static string Left(string strData, int intNumber)
        {
            return strData.Substring(0, intNumber);
        }

        public static string Right(string strData, int intNumber)
        {
            return strData.Substring(strData.Length - intNumber, intNumber);
        }

        public static string CapitalizeEachWord(string value)
        {
            var words = value.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
                }
            }
            return string.Join(" ", words);
        }

        public static bool IsDate(string data)
        {
            return DateTime.TryParse(data, out _);
        }
        #endregion

        #region xml
        public static string TransformXml(string xml, string xslContent)
        {
            if (!xml.StartsWith("<"))
                xml = ReadFile(xml);

            using (var memory = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(xml)))
            {
                var transform = new XslCompiledTransform();
                using (var xmlReader = XmlReader.Create(new StringReader(xslContent)))
                {
                    transform.Load(xmlReader);
                }

                var xpathDoc = new XPathDocument(memory);
                var sb = new System.Text.StringBuilder();
                using (var sw = new System.IO.StringWriter(sb))
                {
                    transform.Transform(xpathDoc, null, sw);
                }

                var result = sb.ToString();
                result = RemoveHeader(result);
                result = RemoveComment(result);
                return result;
            }
        }

        public static string RemoveHeader(string xml)
        {
            return Regex.Replace(xml, "(<[/?]xml[a-zA-Z0-9 =\".-]+[/?][>])", string.Empty);
        }

        public static string RemoveComment(string xml)
        {
            return Regex.Replace(xml, "(<!--[^-]*(?:-[^-]+)*-->)", string.Empty);
        }

        public static XmlDocument ConvertAttrToNode(XmlDocument xmlAttr)
        {
            var xmlNode = new XmlDocument();
            xmlNode.LoadXml($"<{xmlAttr.DocumentElement.Name}/>");

            foreach (XmlElement xmlAttrChild in xmlAttr.DocumentElement.ChildNodes)
            {
                var xmlNodeChild = xmlNode.CreateElement(xmlAttrChild.Name);
                xmlNode.DocumentElement.AppendChild(xmlNodeChild);

                foreach (XmlAttribute xmlAttrItem in xmlAttrChild.Attributes)
                {
                    var xmlNodeItem = xmlNode.CreateElement(xmlAttrItem.Name);
                    xmlNodeChild.AppendChild(xmlNodeItem);
                    xmlNodeItem.InnerText = xmlAttrItem.InnerText;
                }
            }
            return xmlNode;
        }
        #endregion

        #region CSV
        public static string ConvertToCSV(XmlDocument xmlAttr)
        {
            var sb = new System.Text.StringBuilder();

            var xmlRoot = xmlAttr.DocumentElement;
            if (xmlRoot != null)
            {
                for (int i = 0; i < xmlRoot.ChildNodes.Count; i++)
                {
                    var xmlAttrChild = (XmlElement)xmlRoot.ChildNodes[i];

                    if (i == 0)
                    {
                        for (int j = 0; j < xmlAttrChild.Attributes.Count; j++)
                        {
                            if (j != 0) sb.Append(",");
                            sb.Append($"\"{xmlAttrChild.Attributes[j].Name}\"");
                        }
                        sb.Append("\r\n");
                    }

                    for (int j = 0; j < xmlAttrChild.Attributes.Count; j++)
                    {
                        if (j != 0) sb.Append(",");
                        sb.Append($"\"{xmlAttrChild.Attributes[j].InnerText}\"");
                    }
                    sb.Append("\r\n");
                }
            }

            return sb.ToString();
        }
        #endregion

        #region Validation
        public static string MapTicker(string ticker)
        {
            return ticker.ToUpper() switch
            {
                "^GSPC" => "SPX",
                "GSPC" => "SPX",
                _ => ticker,
            };
        }
        #endregion

        #region Conversion
        public static DateTime ToDateTime(string value)
        {
            return DateTime.TryParse(value, out var result) ? result : new DateTime();
        }
        #endregion

        #region Serialization
        public static string SerializeXmlClean<T>(T value)
        {
            var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var serializer = new XmlSerializer(value.GetType());
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true
            };

            using (var stream = new StringWriter())
            using (var writer = XmlWriter.Create(stream, settings))
            {
                serializer.Serialize(writer, value, emptyNamespaces);
                return stream.ToString();
            }
        }

        public static string SerializeXml<T>(object instance)
        {
            var xs = new XmlSerializer(typeof(T));
            using (var sw = new StringWriter())
            using (var w = XmlWriter.Create(sw))
            {
                xs.Serialize(w, instance);
                return sw.ToString();
            }
        }

        public static XmlElement SerializeXmlToElement<T>(object myObject)
        {
            var xmlDom = new XmlDocument();
            xmlDom.LoadXml(SerializeXml<T>(myObject));
            return xmlDom.DocumentElement;
        }

        public static T Deserialize<T>(string xml)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(xml))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        public static string RemoveXmlHeader(string xml)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            foreach (XmlNode node in xmlDoc)
            {
                if (node.NodeType == XmlNodeType.XmlDeclaration)
                {
                    xmlDoc.RemoveChild(node);
                }
            }
            return xmlDoc.OuterXml;
        }
        #endregion

        #region unicode
        public static string EncodeNonAsciiCharacters(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                if (c > 127)
                {
                    // This character is too big for ASCII
                    sb.AppendFormat("\\u{0}", ((int)c).ToString("x4"));
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static string DecodeEncodedNonAsciiCharacters(string value)
        {
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m =>
                {
                    return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
                });
        }
        #endregion

        public static void Sleep(int milliseconds)
        {
            System.Threading.Thread.Sleep(milliseconds);
        }

        public static void OpenInNotepad(string content)
        {
            string file = $"{AppDomain.CurrentDomain.BaseDirectory}//OpenInNotepad.txt";
            System.IO.File.WriteAllText(file, content);

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "notepad.exe",
                Arguments = file
            };

            var process = new System.Diagnostics.Process
            {
                StartInfo = startInfo
            };
            process.Start();
        }
    }
}
