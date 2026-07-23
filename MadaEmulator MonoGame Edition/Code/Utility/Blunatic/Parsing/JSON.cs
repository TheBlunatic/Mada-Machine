using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Blunatic.Parsing
{
    public static class JSON
    {
        /// <summary>
        /// Writes an object to a specified file with comment and reference handling.
        /// </summary>
        /// <param name="objectToWrite">The object to write to a JSON file.</param>
        /// <param name="path">The path of the file to write to.</param>
        public static void WriteToJSON(object objectToWrite, string path)
        {
            File.WriteAllText(path, JsonSerializer.Serialize(objectToWrite,
            new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = true,
                IncludeFields = true,
                MaxDepth = int.MaxValue,
            }));
        }
        /// <summary>
        /// Reads an object from a file.
        /// </summary>
        /// <typeparam name="T">The object type to read.</typeparam>
        /// <param name="path">The path of the file to read from.</param>
        /// <returns>The object read from the specified file.</returns>
        public static T GetObjectFromJSON<T>(string path)
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(path),
                new JsonSerializerOptions
                {
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    ReferenceHandler = ReferenceHandler.Preserve,
                    WriteIndented = true,
                    IncludeFields = true,
                    MaxDepth = int.MaxValue,
                });
        }
        /// <summary>
        /// Writes a list of a specified object to a file.
        /// </summary>
        /// <typeparam name="T">The type of the object to list.</typeparam>
        /// <param name="objectToWrite">The object to list.</param>
        /// <param name="length">The length of the list.</param>
        /// <param name="path">The path of the file to write to.</param>
        public static void WriteListToJSON<T>(T objectToWrite, int length, string path)
        {
            T[] array = new T[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = objectToWrite;
            }
            WriteToJSON(array, path);
        }
    }
}
