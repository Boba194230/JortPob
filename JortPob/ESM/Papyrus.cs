using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace JortPob.ESM
{
    public class Papyrus
    {
        public readonly string id;

        public Papyrus(JsonNode json)
        {
            id = json["id"].GetValue<string>();

            string raw = json["text"].GetValue<string>();
            string[] lines = raw.Split("\r\n");
            for(int i=0;i<lines.Length;i++)
            {
                string sanitize = lines[i];

                // Remove trailing comments
                if (sanitize.Contains(";"))
                {
                    sanitize = sanitize.Split(";")[0].Trim();
                }

                // Remove any multi spaces
                while (sanitize.Contains("  "))
                {
                    sanitize = sanitize.Replace("  ", " ");
                }
            }
        }


        public class Flow()
        {

        }

        public class Call()
        {

        }
    }
}
