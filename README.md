# Pikinni - INI File Handler for .NET

## Overview

Pikinni is a lightweight .NET library designed to manipulate and handle INI files.
This library allows easy access to INI file sections and properties, supporting
operations likereading, modifying, and saving configurations.

## Features

- **Load INI Files**: Directly load INI files from a file path.
- **Access Settings**: Retrieve global settings and settings within named sections.
- **Dynamic Configuration**: Update and manage settings dynamically during runtime.
- **Iterate Sections**: Easily iterate through all sections and their respective properties.

## Getting Started

### Installation

Clone this repository to your machine and include the source code in your .NET projects.

### Usage

#### Loading an INI File

Here's how you can use Pikinni to load an INI file and access its contents:

```
using Pikinni;

// Load an INI file
Ini myIni = Ini.LoadFromFile("path_to_your_ini_file.ini");

// Access a global property
string globalValue = myIni.Global["propertyName"];

// Access a property from a specific section
string sectionValue = myIni["SectionName"]["propertyName"];
```

**Note:** Properties which don't exist return empty string.

#### Creating and Saving a New INI File

```
Ini myNewIni = new Ini();
myNewIni.Global["newGlobalProperty"] = "value1";
myNewIni["NewSection"]["newProperty"] = "value2";

// Print the INI content
Console.WriteLine(myNewIni.ToString());

// Save the new INI file
myNewIni.SaveToFile("MyNew.ini");
```

**Note:** Comments in the original INI file are not preserved during parsing.

#### Iterating Through Sections

```
foreach (Ini.Section section in myIni)
{
    Console.WriteLine($"Section: {section.Name}");
    foreach (var property in section.Properties)
    {
        Console.WriteLine($"Property: {property.Key}, Value: {property.Value}");
    }
}
```

## Contributing

If you have any ideas, bug reports, or enhancements, please feel free to fork the repository,
make changes, and submit a pull request. You can also create an issue to discuss modifications
or additional features.

## License

See LICENSE.txt.
