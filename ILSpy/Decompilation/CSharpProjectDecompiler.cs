namespace ICSharpCode.ILSpy.Decompilation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using Mono.Cecil;

    /// <summary>
    /// Project decompiler for the CSharp.
    /// </summary>
    internal class CSharpProjectDecompiler : ProjectDecompiler
    {
        /// <summary>
        /// Writes the project file
        /// </summary>
        /// <param name="writer">Text writer to which write the project file.</param>
        /// <param name="files">The files which will be in the project.</param>
        /// <param name="module">Assembly module for which project file generated.</param>
        /// <param name="defaultNamespace">Default namespace for the project.</param>
        public override void WriteProjectFile(System.IO.TextWriter writer, IEnumerable<Tuple<string, string>> files, Mono.Cecil.ModuleDefinition module, string defaultNamespace)
        {
            const string ns = "http://schemas.microsoft.com/developer/msbuild/2003";
            string platformName = GetPlatformName(module);
            using (XmlTextWriter w = new XmlTextWriter(writer))
            {
                w.Formatting = Formatting.Indented;
                w.WriteStartDocument();
                w.WriteStartElement("Project", ns);
                w.WriteAttributeString("ToolsVersion", "4.0");
                w.WriteAttributeString("DefaultTargets", "Build");

                w.WriteStartElement("PropertyGroup");
                w.WriteElementString("ProjectGuid", Guid.NewGuid().ToString("B").ToUpperInvariant());

                w.WriteStartElement("Configuration");
                w.WriteAttributeString("Condition", " '$(Configuration)' == '' ");
                w.WriteValue("Debug");
                w.WriteEndElement(); // </Configuration>

                w.WriteStartElement("Platform");
                w.WriteAttributeString("Condition", " '$(Platform)' == '' ");
                w.WriteValue(platformName);
                w.WriteEndElement(); // </Platform>

                switch (module.Kind)
                {
                    case ModuleKind.Windows:
                        w.WriteElementString("OutputType", "WinExe");
                        break;
                    case ModuleKind.Console:
                        w.WriteElementString("OutputType", "Exe");
                        break;
                    default:
                        w.WriteElementString("OutputType", "Library");
                        break;
                }

                w.WriteElementString("AssemblyName", module.Assembly.Name.Name);
                w.WriteElementString("RootNamespace", defaultNamespace);
                switch (module.Runtime)
                {
                    case TargetRuntime.Net_1_0:
                        w.WriteElementString("TargetFrameworkVersion", "v1.0");
                        break;
                    case TargetRuntime.Net_1_1:
                        w.WriteElementString("TargetFrameworkVersion", "v1.1");
                        break;
                    case TargetRuntime.Net_2_0:
                        w.WriteElementString("TargetFrameworkVersion", "v2.0");
                        // TODO: Detect when .NET 3.0/3.5 is required
                        break;
                    default:
                        w.WriteElementString("TargetFrameworkVersion", "v4.0");
                        // TODO: Detect TargetFrameworkProfile
                        break;
                }
                w.WriteElementString("WarningLevel", "4");

                w.WriteEndElement(); // </PropertyGroup>

                w.WriteStartElement("PropertyGroup"); // platform-specific
                w.WriteAttributeString("Condition", " '$(Platform)' == '" + platformName + "' ");
                w.WriteElementString("PlatformTarget", platformName);
                w.WriteEndElement(); // </PropertyGroup> (platform-specific)

                w.WriteStartElement("PropertyGroup"); // Debug
                w.WriteAttributeString("Condition", " '$(Configuration)' == 'Debug' ");
                w.WriteElementString("OutputPath", "bin\\Debug\\");
                w.WriteElementString("DebugSymbols", "true");
                w.WriteElementString("DebugType", "full");
                w.WriteElementString("Optimize", "false");
                w.WriteEndElement(); // </PropertyGroup> (Debug)

                w.WriteStartElement("PropertyGroup"); // Release
                w.WriteAttributeString("Condition", " '$(Configuration)' == 'Release' ");
                w.WriteElementString("OutputPath", "bin\\Release\\");
                w.WriteElementString("DebugSymbols", "true");
                w.WriteElementString("DebugType", "pdbonly");
                w.WriteElementString("Optimize", "true");
                w.WriteEndElement(); // </PropertyGroup> (Release)

                w.WriteStartElement("ItemGroup"); // References
                foreach (AssemblyNameReference r in module.AssemblyReferences)
                {
                    if (r.Name != "mscorlib")
                    {
                        w.WriteStartElement("Reference");
                        w.WriteAttributeString("Include", r.Name);
                        w.WriteEndElement();
                    }
                }
                w.WriteEndElement(); // </ItemGroup> (References)

                foreach (IGrouping<string, string> gr in (from f in files group f.Item2 by f.Item1 into g orderby g.Key select g))
                {
                    w.WriteStartElement("ItemGroup");
                    foreach (string file in gr.OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                    {
                        w.WriteStartElement(gr.Key);
                        w.WriteAttributeString("Include", file);
                        w.WriteEndElement();
                    }
                    w.WriteEndElement();
                }

                w.WriteStartElement("Import");
                w.WriteAttributeString("Project", "$(MSBuildToolsPath)\\Microsoft.CSharp.targets");
                w.WriteEndElement();

                w.WriteEndDocument();
            }
        }
    }
}
