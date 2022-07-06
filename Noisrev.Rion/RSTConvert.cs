using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using Noisrev.League.IO.RST;
using Noisrev.League.IO.RST.Helpers;

namespace Noisrev.Rion
{
    /// <summary>
    /// 提供RST文件相关的转换操作。
    /// </summary>
    public class RSTConvert
    {

        #region Const
        const string ASSEMBLY_NAME = "rion";
        const string ASSEMBLY_VERSION = "0.1.1.0-stable";
        const string HASHTABLE_NAME = "RSTHashes.txt";

        const string JSON_CONFIG_NAME = "config";
        const string JSON_ENTRIES_NAME = "entries";
        const string JSON_VERSION_NAME = "version";
        #endregion

        static Dictionary<ulong, string> hashTable = InitializeHashtable();

        public static string GetRSTItemName(ulong key)
        {
                if (hashTable.ContainsKey(key))
                    return hashTable[key];
                else return "N/A";
        }


        public static JsonDocument RstToJson(RSTFile rst)
        {
            // Set up an output stream
            using var ms = new MemoryStream();
            using var jw = new Utf8JsonWriter(ms, new JsonWriterOptions() { Indented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

            // Start
            jw.WriteStartObject();

            // Magic
            jw.WriteProperty("RMAG", RSTFile.Magic);
            jw.WriteProperty(JSON_VERSION_NAME, ((byte)rst.Version).ToString());

            // Config
            if (!string.IsNullOrEmpty(rst.Config))
            {
                jw.WriteProperty(JSON_CONFIG_NAME, rst.Config);
            }

            // Entries
            jw.WritePropertyName(JSON_ENTRIES_NAME);
            // Entries.Start
            jw.WriteStartObject();

            foreach (var item in rst.Entries)
            {
                // The name of the hash
                string hashName;

                ulong hash = item.Key;
                if (hashTable.ContainsKey(hash))
                    /* Get the name from hashTable */
                    hashName = hashTable[hash];
                else /* hashed hexadecimal string */
                    hashName = "{"+ hash.ToString("x8") + "}";

                jw.WriteProperty(hashName, item.Value);
            }
            // Entries.End
            jw.WriteEndObject();

            // End
            jw.WriteEndObject();
            jw.Flush();
            ms.Position = 0;
            return JsonDocument.Parse(ms);
        }
        public static RSTFile JsonToRst(JsonDocument js)
        {
            var json = js.RootElement.EnumerateObject();
            // temp
            JsonProperty tmp;

            // Set Version
            RVersion version;

            if ( /* tmp is null ? */    (tmp = json.FirstOrDefault<JsonProperty>(x => x.Name.ToLower() == JSON_VERSION_NAME)).Value.ValueKind == JsonValueKind.Undefined ||
                 /* version.GetRtype is null ? */    (version = (RVersion)System.Convert.ToByte(tmp.Value.ToString())).GetRType() == null)
            {
                /* No version, get the latest */
                version = RVersionHelper.GetLatestVersion();
            }

            // Initialization
            var rst = new RSTFile(version);

            // Entries
            if ((tmp = json.FirstOrDefault(x => x.Name.ToLower() == JSON_ENTRIES_NAME)).Value.ValueKind == JsonValueKind.Object)
            {
                foreach (var item in tmp.Value.EnumerateObject())
                {
                    // Property Name
                    string property = item.Name;

                    // NULLCHECK
                    if (string.IsNullOrEmpty(property))
                        continue;

                    // Compatible with "CommunityDragon" format
                    if (property.StartsWith("{"))
                        property = property.Replace("{", "").Replace("}", "");

                    // Parse the key
                    if (!ulong.TryParse(property, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out ulong hash))
                    {
                        hash = RSTHash.ComputeHash(property, rst.Type);
                    }
                    rst.Entries.Add(hash, item.Value.ToString());
                }
            }
            // Config
            if ((tmp = json.FirstOrDefault(x => x.Name.ToLower() == JSON_CONFIG_NAME)).Value.ValueKind == JsonValueKind.String)
                rst.Config = tmp.Value.GetString();

            return rst;
        }


        static Dictionary<ulong, string> InitializeHashtable()
        {
            var dict = new Dictionary<ulong, string>();
            List<string> hashLines = Resources.RSTHashes.Split(new string[] { "\n" }, StringSplitOptions.None).ToList();
            hashLines.Remove("");
            foreach (string item in hashLines)
            {
                if (item is null) continue;

                string hName, value;
                if (item.Contains(' '))
                {
                    var index = item.IndexOf(' ');
                    hName = item[..index];
                    value = item[(index + 1)..];
                }
                else
                {
                    hName = value = item;
                }
                var hash = ulong.Parse(hName, NumberStyles.HexNumber);
                if (!dict.ContainsKey(hash))
                {
                    dict.Add(hash, value);
                }
            }
            return dict;
        }

    }
    static class Extensions
    {
        internal static void WriteProperty(this Utf8JsonWriter utf8JsonWriter, string key, string value)
        {
            utf8JsonWriter.WritePropertyName(key);
            utf8JsonWriter.WriteStringValue(value);
        }
    }

}
