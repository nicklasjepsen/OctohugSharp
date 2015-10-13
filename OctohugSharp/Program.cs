using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
                var fInfo = new FileInfo(orgPath);
                var orgContent = File.ReadAllLines(Path.Combine(originalsDir, orgPath));
                var sb = ParseMetaData(orgContent);
                sb = ParsePost(sb, orgContent);


                Console.Write(sb);
                File.WriteAllText(Path.Combine(outputDir, fInfo.Name), sb.ToString());
            }
        }

        private static StringBuilder ParsePost(StringBuilder sb, string[] contents)
        {
            var numberOfMetaSectionTags = 0;
            for (int i = 0; i < contents.Length; i++)
            {
                var line = contents[i];
                if (line == "---")
                {
                    numberOfMetaSectionTags++;
                    continue;
                }
                if (numberOfMetaSectionTags < 2)
                    continue;


                // Now in the post
                var hashTagCount = 0;
                if (line.StartsWith("#"))
                {
                    while (line.StartsWith("#"))
                    {
                        line = line.Trim();
                        hashTagCount++;
                        if (line.LastIndexOf("#") == line.Length - 1)
                            line = line.Substring(1, line.Length - 2);
                    }

                    var hashes = string.Empty;
                    for (var j = 0; j < hashTagCount; j++)
                    {
                        hashes = hashes + "#";
                    }

                    line = hashes + " " + line;
                }

                sb.Append(line);
                if (i + 1 != contents.Length)
                    sb.AppendLine();
            }
            return sb;
        }

        private static StringBuilder ParseMetaData(string[] orgContent)
        {
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

            return sb;
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
                }
                else if (content.StartsWith(" "))
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
                    while (contents.Length > i + 1 && contents[i + 1].Trim().StartsWith("-") && !contents[i + 1].Trim().StartsWith("---"))
                    {
                        results.Add(contents[i + 1]);
                        i++;
                    }
                    return results;
                }
            }

            return new List<string>();
        }
    }
}
