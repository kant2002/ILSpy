﻿// Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;
using ICSharpCode.ILSpy.Options;
using Mono.Cecil;

namespace ICSharpCode.ILSpy
{
	/// <summary>
	/// Represents an assembly loaded into ILSpy.
	/// </summary>
	public sealed class LoadedAssembly
	{
		readonly Task<ModuleDefinition> assemblyTask;
		readonly AssemblyList assemblyList;
		readonly string fileName;
		readonly string shortName;
		
		public LoadedAssembly(AssemblyList assemblyList, string fileName)
		{
			if (assemblyList == null)
				throw new ArgumentNullException("assemblyList");
			if (fileName == null)
				throw new ArgumentNullException("fileName");
			this.assemblyList = assemblyList;
			this.fileName = fileName;
			
			this.assemblyTask = Task.Factory.StartNew<ModuleDefinition>(LoadAssembly); // requires that this.fileName is set
			this.shortName = Path.GetFileNameWithoutExtension(fileName);
		}
		
		/// <summary>
		/// Gets the Cecil ModuleDefinition.
		/// Can be null when there was a load error.
		/// </summary>
		public ModuleDefinition ModuleDefinition {
			get {
				try {
					return assemblyTask.Result;
				} catch (AggregateException) {
					return null;
				}
			}
		}
		
		/// <summary>
		/// Gets the Cecil AssemblyDefinition.
		/// Is null when there was a load error; or when opening a netmodule.
		/// </summary>
		public AssemblyDefinition AssemblyDefinition {
			get {
				var module = this.ModuleDefinition;
				return module != null ? module.Assembly : null;
			}
		}
		
		public AssemblyList AssemblyList {
			get { return assemblyList; }
		}
		
		public string FileName {
			get { return fileName; }
		}
		
		public string ShortName {
			get { return shortName; }
		}
		
		public bool IsLoaded {
			get { return assemblyTask.IsCompleted; }
		}
		
		public bool HasLoadError {
			get { return assemblyTask.IsFaulted; }
		}
		
		ModuleDefinition LoadAssembly()
		{
			// runs on background thread
			ReaderParameters p = new ReaderParameters();
			p.AssemblyResolver = new MyAssemblyResolver(this);
			ModuleDefinition module = ModuleDefinition.ReadModule(fileName, p);
			if (DecompilerSettingsPanel.CurrentDecompilerSettings.UseDebugSymbols) {
				try {
					LoadSymbols(module);
				} catch (IOException) {
				} catch (UnauthorizedAccessException) {
				} catch (InvalidOperationException) {
					// ignore any errors during symbol loading
				}
			}
			return module;
		}
		
		private void LoadSymbols(ModuleDefinition module)
		{
			// search for pdb in same directory as dll
			string pdbName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + ".pdb");
			if (File.Exists(pdbName)) {
				using (Stream s = File.OpenRead(pdbName)) {
					module.ReadSymbols(new Mono.Cecil.Pdb.PdbReaderProvider().GetSymbolReader(module, s));
				}
				return;
			}
			
			// TODO: use symbol cache, get symbols from microsoft
		}
		
		[ThreadStatic]
		static int assemblyLoadDisableCount;
		
		public static IDisposable DisableAssemblyLoad()
		{
			assemblyLoadDisableCount++;
			return new DecrementAssemblyLoadDisableCount();
		}
		
		sealed class DecrementAssemblyLoadDisableCount : IDisposable
		{
			bool disposed;
			
			public void Dispose()
			{
				if (!disposed) {
					disposed = true;
					assemblyLoadDisableCount--;
					// clear the lookup cache since we might have stored the lookups failed due to DisableAssemblyLoad()
					MainWindow.Instance.CurrentAssemblyList.ClearCache();
				}
			}
		}
		
		sealed class MyAssemblyResolver : IAssemblyResolver
		{
			readonly LoadedAssembly parent;
			
			public MyAssemblyResolver(LoadedAssembly parent)
			{
				this.parent = parent;
			}
			
			public AssemblyDefinition Resolve(AssemblyNameReference name)
			{
				var node = parent.LookupReferencedAssembly(name);
				return node != null ? node.AssemblyDefinition : null;
			}
			
			public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
			{
				var node = parent.LookupReferencedAssembly(name);
				return node != null ? node.AssemblyDefinition : null;
			}
			
			public AssemblyDefinition Resolve(string fullName)
			{
				var node = parent.LookupReferencedAssembly(fullName);
				return node != null ? node.AssemblyDefinition : null;
			}
			
			public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
			{
				var node = parent.LookupReferencedAssembly(fullName);
				return node != null ? node.AssemblyDefinition : null;
			}
		}
		
		public IAssemblyResolver GetAssemblyResolver()
		{
			return new MyAssemblyResolver(this);
		}
		
		public LoadedAssembly LookupReferencedAssembly(AssemblyNameReference name)
		{
			if (name == null)
				throw new ArgumentNullException("name");
			if (name.IsWindowsRuntime) {
				return assemblyList.winRTMetadataLookupCache.GetOrAdd(name.Name, LookupWinRTMetadata);
			} else {
				return assemblyList.assemblyLookupCache.GetOrAdd(name.FullName, LookupReferencedAssemblyInternal);
			}
		}
		
		public LoadedAssembly LookupReferencedAssembly(string fullName)
		{
			return assemblyList.assemblyLookupCache.GetOrAdd(fullName, LookupReferencedAssemblyInternal);
		}
		
		LoadedAssembly LookupReferencedAssemblyInternal(string fullName)
		{
			foreach (LoadedAssembly asm in assemblyList.GetAssemblies()) {
				if (asm.AssemblyDefinition != null && fullName.Equals(asm.AssemblyDefinition.FullName, StringComparison.OrdinalIgnoreCase))
					return asm;
			}
			if (assemblyLoadDisableCount > 0)
				return null;
			
			if (!App.Current.Dispatcher.CheckAccess()) {
				// Call this method on the GUI thread.
				return (LoadedAssembly)App.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Func<string, LoadedAssembly>(LookupReferencedAssembly), fullName);
			}
			
			var name = AssemblyNameReference.Parse(fullName);
			string file = GacInterop.FindAssemblyInNetGacOrWinMetadata(name);
			if (file == null) {
				string dir = Path.GetDirectoryName(this.fileName);
				if (File.Exists(Path.Combine(dir, name.Name + ".dll")))
					file = Path.Combine(dir, name.Name + ".dll");
				else if (File.Exists(Path.Combine(dir, name.Name + ".exe")))
					file = Path.Combine(dir, name.Name + ".exe");
			}
			if (file != null) {
				return assemblyList.OpenAssembly(file);
			} else {
				return null;
			}
		}
		
		LoadedAssembly LookupWinRTMetadata(string name)
		{
			foreach (LoadedAssembly asm in assemblyList.GetAssemblies()) {
				if (asm.AssemblyDefinition != null && name.Equals(asm.AssemblyDefinition.Name.Name, StringComparison.OrdinalIgnoreCase))
					return asm;
			}
			if (assemblyLoadDisableCount > 0)
				return null;
			if (!App.Current.Dispatcher.CheckAccess()) {
				// Call this method on the GUI thread.
				return (LoadedAssembly)App.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Func<string, LoadedAssembly>(LookupWinRTMetadata), name);
			}

            var programFiles86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            string[] lookupFolders = new string[] {
                Path.Combine(Environment.SystemDirectory, "WinMetadata"),
                Path.Combine(programFiles86, @"Windows Kits\8.0\References\CommonConfiguration\Neutral")
            };

            foreach (var lookupFolder in lookupFolders)
	        {
                string file = Path.Combine(lookupFolder, name + ".winmd");
			    if (File.Exists(file)) {
				    return assemblyList.OpenAssembly(file);
			    } 
	        }

			return null;
		}
		
		public Task ContinueWhenLoaded(Action<Task<ModuleDefinition>> onAssemblyLoaded, TaskScheduler taskScheduler)
		{
			return this.assemblyTask.ContinueWith(onAssemblyLoaded, taskScheduler);
		}
		
		/// <summary>
		/// Wait until the assembly is loaded.
		/// Throws an AggregateException when loading the assembly fails.
		/// </summary>
		public void WaitUntilLoaded()
		{
			assemblyTask.Wait();
		}
	}
}
