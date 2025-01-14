﻿using System;
using Microsoft.Build.Utilities;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using System.Linq;
using System.IO;
using Xamarin.Android.Tools;

namespace Xamarin.Android.Tasks
{
	public class DetermineJavaLibrariesToCompile : AndroidTask
	{
		public override string TaskPrefix => "DJL";

		[Required]
		public ITaskItem[] MonoPlatformJarPaths { get; set; }

		public bool EnableInstantRun { get; set; }

		public ITaskItem[] JavaSourceFiles { get; set; }

		public ITaskItem[] JavaLibraries { get; set; }

		public ITaskItem[] ExternalJavaLibraries { get; set; }

		public ITaskItem[] DoNotPackageJavaLibraries { get; set; }

		public ITaskItem[] LibraryProjectJars { get; set; }

		public ITaskItem[] AdditionalJavaLibraryReferences { get; set; }

		[Output]
		public ITaskItem[] JavaLibrariesToCompile { get; set; }

		[Output]
		public ITaskItem[] ReferenceJavaLibraries { get; set; }

		public override bool RunTask ()
		{
			var jars = new List<ITaskItem> ();
			if (!EnableInstantRun)
				jars.AddRange (MonoPlatformJarPaths);
			if (JavaSourceFiles != null)
				foreach (var jar in JavaSourceFiles.Where (p => Path.GetExtension (p.ItemSpec) == ".jar"))
					jars.Add (jar);
			if (JavaLibraries != null)
				foreach (var jarfile in JavaLibraries)
					jars.Add (jarfile);
			if (LibraryProjectJars != null)
				foreach (var jar in LibraryProjectJars)
					if (!MonoAndroidHelper.IsEmbeddedReferenceJar (jar.ItemSpec))
						jars.Add (jar);
			if (AdditionalJavaLibraryReferences != null)
				foreach (var jar in AdditionalJavaLibraryReferences.Distinct (TaskItemComparer.DefaultComparer))
					jars.Add (jar);

			var distinct  = MonoAndroidHelper.DistinctFilesByContent (jars);

			var javaLibrariesToCompile = new List<ITaskItem> ();
			var referenceJavaLibraries = new List<ITaskItem> ();
			if (ExternalJavaLibraries != null)
				referenceJavaLibraries.AddRange (ExternalJavaLibraries);

			foreach (var item in distinct) {
				if (!HasClassFiles (item.ItemSpec))
					continue;
				if (IsExcluded (item.ItemSpec)) {
					referenceJavaLibraries.Add (item);
				} else {
					javaLibrariesToCompile.Add (item);
				}
			}
			JavaLibrariesToCompile = javaLibrariesToCompile.ToArray ();
			ReferenceJavaLibraries = referenceJavaLibraries.ToArray ();

			Log.LogDebugTaskItems ("  JavaLibrariesToCompile:", JavaLibrariesToCompile);
			Log.LogDebugTaskItems ("  ReferenceJavaLibraries:", ReferenceJavaLibraries);

			return true;
		}

		bool HasClassFiles (string jar)
		{
			return Files.ZipAny (jar, entry => entry.FullName.EndsWith (".class", StringComparison.OrdinalIgnoreCase));
		}

		bool IsExcluded (string jar)
		{
			if (DoNotPackageJavaLibraries == null)
				return false;
			return DoNotPackageJavaLibraries.Any (x => Path.GetFileName (jar).EndsWith (x.ItemSpec, StringComparison.OrdinalIgnoreCase));
		}
	}

	class TaskItemComparer : IEqualityComparer<ITaskItem> {
		public static readonly TaskItemComparer     DefaultComparer     = new TaskItemComparer ();

		public bool Equals (ITaskItem a, ITaskItem b)
		{
			return string.Compare (a.ItemSpec, b.ItemSpec, StringComparison.OrdinalIgnoreCase) == 0;
		}

		public int GetHashCode (ITaskItem value)
		{
			return value.ItemSpec.GetHashCode ();
		}
	}
}

