using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OctohugSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var originalsDir = args[0];
            var outputDir = args[1];

            foreach (var orgPath in Directory.EnumerateFiles(originalsDir))
            {
                var orgContent = File.ReadAllLines(Path.Combine(originalsDir, orgPath));
                var sb = new StringBuilder();

                var meta = GetMeta(orgContent);

                sb.AppendLine("---");
                foreach (var metaSetting in meta.MetaSettings)
                {
                    var metaName = metaSetting.Substring(0, metaSetting.IndexOf(':'));
                    var metaValue = metaSetting.Substring(metaSetting.IndexOf(':') + 2);

                    if (metaName == "layout" ||
                        metaName == "date_text")
                        continue;
                    if (metaName == "permalink")
                    {
                        sb.AppendLine("url: \"" + metaValue + "\"");
                        continue;
                    }
                    if (!metaValue.StartsWith("\""))
                        metaValue = "\"" + metaValue;
                    if (!metaValue.EndsWith("\""))
                        metaValue = metaValue + "\"";
                    sb.AppendLine(metaName + ": " + metaValue);
                }
                sb.AppendLine("categories:");
                foreach (var category in meta.Categories)
                {
                    sb.AppendLine(category);
                }
                sb.AppendLine("tags:");
                foreach (var tag in meta.Tags)
                {
                    sb.AppendLine(tag);
                }
                sb.AppendLine("---");

                //for (int i = meta.MetaEndTagIndex; i < orgContent.Length; i++)
                //{
                //    var line = orgContent[i];
                //}
                Console.Write(sb);
                //File.WriteAllText(Path.Combine(outputDir, orgPath), sb.ToString());
                Console.Read();
            }
        }

        class MetaData
        {
            public int MetaEndTagIndex { get; set; }
            public List<string> MetaSettings { get; set; }
            public List<string> Categories { get; set; }
            public List<string> Tags { get; set; }
        }

        private static MetaData GetMeta(string[] contents)
        {
            var result = new List<string>();
            var numberOfSectionTags = 0;
            var indexCount = 0;
            var cats = new List<string>();
            var tags = new List<string>();
            foreach (var content in contents)
            {
                indexCount++;
                if (content == "---")
                {
                    numberOfSectionTags++;
                    continue;
                }
                if (numberOfSectionTags == 2)
                    break;
                
                if (content == "categories:")
                {
                    cats = RemoveListFromMeta(contents, "categories");
                }
                else if (content == "tags:")
                {
                    tags = RemoveListFromMeta(contents, "tags");
                } else if (content.StartsWith(" "))
                    continue;
                else
                {
                    result.Add(content);
                }
            }

            return new MetaData
            {
                MetaEndTagIndex = indexCount,
                MetaSettings = result,
                Categories = cats,
                Tags = tags
            };
        }

        private static List<string> RemoveListFromMeta(string[] contents, string metaName)
        {
            var results = new List<string>();
            for (int i = 0; i < contents.Length; i++)
            {
                if (contents[i] == metaName + ":")
                {
                    while (contents.Length > i + 1 && contents[i+1].Trim().StartsWith("-") && !contents[i+1].Trim().StartsWith("---"))
                    {
                        results.Add(contents[i+1]);
                        i++;
                    }
                    return results;
                }
            }

            return new List<string>();
        }
    }
}
