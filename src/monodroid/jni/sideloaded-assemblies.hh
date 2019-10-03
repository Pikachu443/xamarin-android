#ifndef INC_MONODROID_INMEMORY_ASSEMBLIES_H
#define INC_MONODROID_INMEMORY_ASSEMBLIES_H

#include "jni-wrappers.hh"
#include <string.h>
#include <mono/metadata/appdomain.h>
#include <mono/metadata/assembly.h>

#define DEFAULT_CAPACITY 8

namespace xamarin::android::internal {
	class SideloadedAssemblies
	{
		private:
			struct SideloadedAssemblyEntry
			{
				int domain_id;
				unsigned int assemblies_count;
				char **names;
				char **assemblies_bytes;
				char **assemblies_paths;
				unsigned int *assemblies_bytes_len;

				SideloadedAssemblyEntry (MonoDomain *domain, JNIEnv *env, jstring_array_wrapper &assemblies, jobjectArray assembliesBytes, jstring_array_wrapper &assembliesPaths);
				~SideloadedAssemblyEntry ();
			};

		public:
			SideloadedAssemblies ()
			{
				capacity = DEFAULT_CAPACITY;
				entries = new SideloadedAssemblyEntry*[DEFAULT_CAPACITY];
			}

			bool has_assemblies () const { return length > 0; }
			MonoAssembly* try_load_assembly (MonoDomain *domain, MonoAssemblyName *name);
			void add_or_update_from_java (MonoDomain *domain, JNIEnv *env, jstring_array_wrapper &assemblies, jobjectArray assembliesBytes, jstring_array_wrapper &assembliesPaths);
			void clear_for_domain (MonoDomain *domain);

		private:
			SideloadedAssemblyEntry **entries;
			unsigned int capacity;
			unsigned int length;

			SideloadedAssemblyEntry* find_entry (int domain_id);
			void add_or_replace_entry (SideloadedAssemblyEntry *new_entry);
			void add_entry (SideloadedAssemblyEntry *entry);
			SideloadedAssemblyEntry* remove_entry (int domain_id);
	};
}

#endif