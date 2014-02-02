namespace ICSharpCode.ILSpy.Decompilation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Resources;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using ICSharpCode.Decompiler;
    using ICSharpCode.Decompiler.Ast;
    using Mono.Cecil;

    /// <summary>
    /// Base class which allows decompile the assembly to the project file
    /// </summary>
    internal abstract class ProjectDecompiler
    {
        private Language language;

        public static string GetPlatformName(ModuleDefinition module)
        {
            switch (module.Architecture)
            {
                case TargetArchitecture.I386:
                    if ((module.Attributes & ModuleAttributes.Preferred32Bit) == ModuleAttributes.Preferred32Bit)
                        return "AnyCPU";
                    else if ((module.Attributes & ModuleAttributes.Required32Bit) == ModuleAttributes.Required32Bit)
                        return "x86";
                    else
                        return "AnyCPU";
                case TargetArchitecture.AMD64:
                    return "x64";
                case TargetArchitecture.IA64:
                    return "Itanium";
                default:
                    return module.Architecture.ToString();
            }
        }

        public void Decompile(Language language, LoadedAssembly assembly, ITextOutput output, DecompilationOptions options)
        {
            this.language = language;

            HashSet<string> directories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var defaultNamespace = ProjectDecompiler.GetDefaultNamespace(assembly.ModuleDefinition, options);
            var files = WriteCodeFilesInProject(assembly, options, directories, defaultNamespace).ToList();
            files.AddRange(WriteResourceFilesInProject(assembly, options, directories));
            WriteProjectFile(new TextOutputWriter(output), files, assembly.ModuleDefinition, defaultNamespace);
        }

        /// <summary>
        /// Writes the project file
        /// </summary>
        /// <param name="writer">Text writer to which write the project file.</param>
        /// <param name="files">The files which will be in the project.</param>
        /// <param name="module">Assembly module for which project file generated.</param>
        /// <param name="defaultNamespace">Default namespace for the project.</param>
        public abstract void WriteProjectFile(TextWriter writer, IEnumerable<Tuple<string, string>> files, ModuleDefinition module, string defaultNamespace);

        /// <summary>
        /// This method try to guess default namespace which would be suitable for the module.
        /// </summary>
        /// <param name="module">Module for which default namespace should be guessed.</param>
        /// <param name="options">Decompilation options.</param>
        /// <returns>Default namespace for the classes in the module.</returns>
        internal static string GetDefaultNamespace(ModuleDefinition module, DecompilationOptions options)
        {
            var types = module.Types.Where(t => IncludeTypeWhenDecompilingProject(t, options));
            var group = types.GroupBy(t => FindFirstNamespacePart(t)).OrderByDescending(a => a.Count()).FirstOrDefault();
            return group == null ? "" : group.Key;
        }
        
        private static string FindFirstNamespacePart(TypeDefinition type)
        {
            int index = type.Namespace.IndexOf('.');
            return index > 0 ? type.Namespace.Substring(0, index) : type.Namespace;
        }

        private static bool IncludeTypeWhenDecompilingProject(TypeDefinition type, DecompilationOptions options)
        {
            if (type.Name == "<Module>" || AstBuilder.MemberIsHidden(type, options.DecompilerSettings))
                return false;
            if (type.Namespace == "XamlGeneratedNamespace" && type.Name == "GeneratedInternalTypeHelper")
                return false;
            return true;
        }

        private static string GetNamespaceFolder(string typeNamespace, string defaultNamespace)
        {
            string dname = TextView.DecompilerTextView.CleanUpName(defaultNamespace);
            string name = TextView.DecompilerTextView.CleanUpName(typeNamespace);
            name = name.Replace('.', Path.DirectorySeparatorChar);
            if (name.StartsWith(dname + Path.DirectorySeparatorChar))
                return name.Substring(dname.Length + 1);
            else if (name == dname)
                return "";
            return name;
        }

        private IEnumerable<Tuple<string, string>> WriteAssemblyInfo(LoadedAssembly assembly, DecompilationOptions options, HashSet<string> directories)
        {
            // don't automatically load additional assemblies when an assembly node is selected in the tree view
            using (LoadedAssembly.DisableAssemblyLoad())
            {
                string propertiesDirectory = "Properties";
                if (directories.Add(propertiesDirectory))
                {
                    Directory.CreateDirectory(Path.Combine(options.SaveAsProjectDirectory, propertiesDirectory));
                }

                var assemblyInfoProjectItem = Path.Combine(propertiesDirectory, "AssemblyInfo" + language.FileExtension);
                var assemblyInfoFileName = Path.Combine(options.SaveAsProjectDirectory, assemblyInfoProjectItem);
                using (StreamWriter w = new StreamWriter(assemblyInfoFileName))
                {
                    var fullDecompilation = options.FullDecompilation;
                    options.FullDecompilation = false;
                    var textOutput = new PlainTextOutput(w);
                    textOutput.SetIndentationString(options.DecompilerSettings.IndentString);
                    this.language.DecompileAssembly(
                        assembly,
                        textOutput, 
                        options);
                    options.FullDecompilation = fullDecompilation;
                }

                return new Tuple<string, string>[] { Tuple.Create("Compile", assemblyInfoProjectItem) };
            }
        }

        private IEnumerable<Tuple<string, string>> WriteCodeFilesInProject(LoadedAssembly assembly, DecompilationOptions options, HashSet<string> directories, string defaultNamespace)
        {
            var files = assembly.ModuleDefinition.Types.Where(t => IncludeTypeWhenDecompilingProject(t, options)).GroupBy(
                delegate(TypeDefinition type)
                {
                    string file = TextView.DecompilerTextView.CleanUpName(type.Name) + this.language.FileExtension;
                    if (string.IsNullOrEmpty(type.Namespace))
                    {
                        return file;
                    }
                    else
                    {
                        string dir = GetNamespaceFolder(type.Namespace, defaultNamespace);
                        if (dir.Length > 0)
                        {
                            if (directories.Add(dir))
                            {
                                Directory.CreateDirectory(Path.Combine(options.SaveAsProjectDirectory, dir));
                            }

                            return Path.Combine(dir, file);
                        }

                        return file;
                    }
                }, StringComparer.OrdinalIgnoreCase).ToList();
            AstMethodBodyBuilder.ClearUnhandledOpcodes();
            Parallel.ForEach(
                files,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                delegate(IGrouping<string, TypeDefinition> file)
                {
                    using (StreamWriter w = new StreamWriter(Path.Combine(options.SaveAsProjectDirectory, file.Key)))
                    {
                        var fullDecompilation = options.FullDecompilation;
                        options.FullDecompilation = false;
                        var textOutput = new PlainTextOutput(w);
                        textOutput.SetIndentationString(options.DecompilerSettings.IndentString);
                        this.language.DecompileType(file.First(), textOutput, options);
                        options.FullDecompilation = fullDecompilation;
                    }
                });
            AstMethodBodyBuilder.PrintNumberOfUnhandledOpcodes();
            return files.Select(f => Tuple.Create("Compile", f.Key)).Concat(WriteAssemblyInfo(assembly, options, directories));
        }

        private IEnumerable<Tuple<string, string>> WriteResourceFilesInProject(LoadedAssembly assembly, DecompilationOptions options, HashSet<string> directories)
        {
            //AppDomain bamlDecompilerAppDomain = null;
            //try {
            foreach (EmbeddedResource r in assembly.ModuleDefinition.Resources.OfType<EmbeddedResource>())
            {
                string fileName;
                Stream s = r.GetResourceStream();
                s.Position = 0;
                if (r.Name.EndsWith(".g.resources", StringComparison.OrdinalIgnoreCase))
                {
                    IEnumerable<DictionaryEntry> rs = null;
                    try
                    {
                        rs = new ResourceSet(s).Cast<DictionaryEntry>();
                    }
                    catch (ArgumentException)
                    {
                    }
                    if (rs != null && rs.All(e => e.Value is Stream))
                    {
                        foreach (var pair in rs)
                        {
                            fileName = Path.Combine(((string)pair.Key).Split('/').Select(p => TextView.DecompilerTextView.CleanUpName(p)).ToArray());
                            string dirName = Path.GetDirectoryName(fileName);
                            if (!string.IsNullOrEmpty(dirName) && directories.Add(dirName))
                            {
                                Directory.CreateDirectory(Path.Combine(options.SaveAsProjectDirectory, dirName));
                            }
                            Stream entryStream = (Stream)pair.Value;
                            entryStream.Position = 0;
                            if (fileName.EndsWith(".baml", StringComparison.OrdinalIgnoreCase))
                            {
                                //									MemoryStream ms = new MemoryStream();
                                //									entryStream.CopyTo(ms);
                                // TODO implement extension point
                                //									var decompiler = Baml.BamlResourceEntryNode.CreateBamlDecompilerInAppDomain(ref bamlDecompilerAppDomain, assembly.FileName);
                                //									string xaml = null;
                                //									try {
                                //										xaml = decompiler.DecompileBaml(ms, assembly.FileName, new ConnectMethodDecompiler(assembly), new AssemblyResolver(assembly));
                                //									}
                                //									catch (XamlXmlWriterException) { } // ignore XAML writer exceptions
                                //									if (xaml != null) {
                                //										File.WriteAllText(Path.Combine(options.SaveAsProjectDirectory, Path.ChangeExtension(fileName, ".xaml")), xaml);
                                //										yield return Tuple.Create("Page", Path.ChangeExtension(fileName, ".xaml"));
                                //										continue;
                                //									}
                            }
                            using (var fs = new FileStream(Path.Combine(options.SaveAsProjectDirectory, fileName), FileMode.Create, FileAccess.Write))
                            {
                                entryStream.CopyTo(fs);
                            }
                            yield return Tuple.Create("Resource", fileName);
                        }
                        continue;
                    }
                }
                fileName = GetFileNameForResource(r.Name, directories);
                using (var fs = new FileStream(Path.Combine(options.SaveAsProjectDirectory, fileName), FileMode.Create, FileAccess.Write))
                {
                    s.CopyTo(fs);
                }
                yield return Tuple.Create("EmbeddedResource", fileName);
            }
            //}
            //finally {
            //    if (bamlDecompilerAppDomain != null)
            //        AppDomain.Unload(bamlDecompilerAppDomain);
            //}
        }

        private string GetFileNameForResource(string fullName, HashSet<string> directories)
        {
            string[] splitName = fullName.Split('.');
            string fileName = TextView.DecompilerTextView.CleanUpName(fullName);
            for (int i = splitName.Length - 1; i > 0; i--)
            {
                string ns = string.Join(".", splitName, 0, i);
                if (directories.Contains(ns))
                {
                    string name = string.Join(".", splitName, i, splitName.Length - i);
                    fileName = Path.Combine(ns, TextView.DecompilerTextView.CleanUpName(name));
                    break;
                }
            }
            return fileName;
        }
    }
}
