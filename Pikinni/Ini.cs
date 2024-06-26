﻿using System.Collections;
using System.Text.RegularExpressions;

namespace Pikinni
{
    /// <summary>
    /// Represents an INI file, allowing access to its sections and properties.
    /// </summary>
    public partial class Ini : IEnumerable<Ini.Section>
    {
        /// <summary>
        /// Represents a section within an INI file, containing a set of properties.
        /// </summary>
        public class Section
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Section"/> class with the specified name.
            /// </summary>
            /// <param name="name">The name of the section.</param>
            public Section(string name)
            {
                Name = name;
                Properties = new Dictionary<string, string>();
            }

            /// <summary>
            /// Gets the name of the section.
            /// </summary>
            public string Name { get; init; }

            /// <summary>
            /// Gets the dictionary of properties within the section.
            /// </summary>
            public Dictionary<string, string> Properties { get; init; }

            /// <summary>
            /// Gets or sets the value associated with the specified property name in this section.
            /// If the property does not exist, getting this value returns an empty string.
            /// </summary>
            /// <param name="propertyName">The name of the property to get or set.</param>
            /// <returns>The value of the property if it exists; otherwise, an empty string.</returns>
            public string this[string propertyName]
            {
                get => Properties.TryGetValue(propertyName, out var value) ? value : "";
                set => Properties[propertyName] = value;
            }
        }

        /// <summary>
        /// Gets the global section of the INI file, which contains properties that are not enclosed
        /// in any named section.
        /// </summary>
        public Section Global { get; init; } = new Section("Global");

        /// <summary>
        /// Named sections of the INI file.
        /// </summary>
        private Dictionary<string, Section> Sections { get; init; } = new Dictionary<string, Section>();

        /// <summary>
        /// Gets or sets the section with the specified name. If the section does not exist, a new
        /// empty Section is created and returned.
        /// </summary>
        /// <param name="sectionName">The name of the section to access.</param>
        /// <returns>The Section with the specified name, or a new empty Section if it does not exist.</returns>
        public Section this[string sectionName]
        {
            get
            {
                if (!Sections.TryGetValue(sectionName, out var section))
                {
                    section = new Section(sectionName);
                    Sections[sectionName] = section;
                }
                return section;
            }
            set => Sections[sectionName] = value;
        }

        /// <summary>
        /// Initializes a new instance of the Ini class by parsing the provided INI data.
        /// </summary>
        /// <param name="source">The INI file content as a string, which will be parsed to populate the
        /// Ini object.</param>
        public Ini(string source = "")
        {
            ParseIniData(source);
        }

        /// <summary>
        /// Parses the INI file content and populates the INI object.
        /// </summary>
        /// <param name="source">The string content of an INI file.</param>
        /// <remarks>
        /// This method splits the input string into lines and processes each line to determine whether it
        /// represents a section header, a property, or a comment. Section headers are added to the Sections
        /// dictionary, and properties are added under the appropriate section. Lines that are recognized as
        /// comments or that do not contain an equal sign ('=') are ignored.
        /// </remarks>
        private void ParseIniData(string source)
        {
            var splitSeparator = new[] { "\r\n", "\n" };
            var splitOptions = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
            var lines = new List<string>(source.Split(splitSeparator, splitOptions));
            Section currentSection = Global;

            foreach (var line in lines)
            {
                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    var sectionName = line[1..^1].Trim();
                    currentSection = new Section(sectionName);
                    Sections[sectionName] = currentSection;
                    continue;
                }
                if (line.StartsWith(';') || line.StartsWith('#') || !line.Contains('='))
                {
                    continue;
                }
                var equalSignPosition = line.IndexOf('=');
                var propertyName = line[..equalSignPosition].Trim();
                var value = line[(equalSignPosition + 1)..].Trim();

                value = value.Replace(@"\\", @"\").Replace(@"\r", "\r").Replace(@"\n", "\n").Replace(@"\t", "\t").Replace(@"\s", " ");


                currentSection[propertyName] = value;
            }
        }

        /// <summary>
        /// Loads configuration data from an INI file and creates a new Ini object with the processed data.
        /// </summary>
        /// <param name="fileName">The path to the INI file to be loaded.</param>
        /// <returns>A new Ini object containing data from the file.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file at the specified path
        /// does not exist.</exception>
        public static Ini LoadFromFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException($"File {fileName} not found.");
            }
            var source = File.ReadAllText(fileName);
            return new Ini(source);
        }

        /// <summary>
        /// Saves the current INI configuration to a file.
        /// </summary>
        /// <param name="fileName">The file path where the INI configuration will be saved.</param>
        /// <exception cref="ArgumentException">Thrown when the file name is null or whitespace.</exception>
        /// <exception cref="IOException">Thrown if an I/O error occurs when writing to the file.</exception>
        public void SaveToFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be null or whitespace.", nameof(fileName));
            }
            try
            {
                File.WriteAllText(fileName, ToString());
            }
            catch (IOException ex)
            {
                throw new IOException($"An error occurred while writing to the file: {fileName}", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException("Check your access permissions for the file path provided.", ex);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the sections of the Ini file.
        /// </summary>
        /// <returns>An IEnumerator for sections in the Ini file.</returns>
        public IEnumerator<Section> GetEnumerator()
        {
            return Sections.Values.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the sections of the Ini file (non-generic version).
        /// </summary>
        /// <returns>An IEnumerator for sections in the Ini file.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Returns a string that represents the current INI configuration.
        /// </summary>
        /// <returns>A string representation of the INI file, including all global properties and sectioned
        /// properties formatted as they would appear in a standard INI file.</returns>
        public override string ToString()
        {
            string source = "";

            foreach (var property in Global.Properties)
            {
                source += $"{property.Key} = {Escape(property.Value)}\n";
            }
            foreach (var section in this)
            {
                source += "\n";
                source += $"[{section.Name}]\n";
                foreach (var property in section.Properties)
                {
                    source += $"{property.Key} = {Escape(property.Value)}\n";
                }
            }
            return source;
        }

        /// <summary>
        /// Escapes special characters in a given text string to ensure that the text is safe for use in an INI file.
        /// </summary>
        /// <param name="text">The text to escape.</param>
        /// <returns>The escaped text string.</returns>
        private static string Escape(string text)
        {
            text = text.Replace(@"\", @"\\").Replace("\r", @"\r").Replace("\n", @"\n").Replace("\t", @"\t");
            text = RegexSpaces().Replace(text, m => RegexSingleSpace().Replace(m.Value, @"\s"));
            return text;
        }

        /// <summary>
        /// Compiled regex to match spaces at the start and end of a string.
        /// </summary>
        /// <returns>A Regex that matches leading and trailing spaces.</returns>
        [GeneratedRegex(@"(^ +| +$)", RegexOptions.Compiled)]
        private static partial Regex RegexSpaces();

        /// <summary>
        /// Compiled regex to match single spaces for replacement.
        /// </summary>
        /// <returns>A Regex that matches single space characters.</returns>
        [GeneratedRegex(" ", RegexOptions.Compiled)]
        private static partial Regex RegexSingleSpace();

    }
}
