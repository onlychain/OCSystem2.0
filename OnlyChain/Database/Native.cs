using System;
using System.Runtime.InteropServices;
using System.Security;

#pragma warning disable IDE1006 // 命名样式
#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable CA1401 // P/Invokes should not be visible
#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments

namespace OnlyChain.Database {
    [SuppressUnmanagedCodeSecurity]
    unsafe internal static class Native {
        public readonly struct leveldb_t { }
        public readonly struct leveldb_cache_t { }
        public readonly struct leveldb_comparator_t { }
        public readonly struct leveldb_env_t { }
        public readonly struct leveldb_filterpolicy_t { }
        public readonly struct leveldb_iterator_t { }
        public readonly struct leveldb_logger_t { }
        public readonly struct leveldb_options_t { }
        public readonly struct leveldb_readoptions_t { }
        public readonly struct leveldb_snapshot_t { }
        public readonly struct leveldb_writebatch_t { }
        public readonly struct leveldb_writeoptions_t { }


        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void leveldb_writebatch_iterate_put(void* p0, void* k, size_t klen, void* v, size_t vlen);

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void leveldb_writebatch_iterate_deleted(void* p0, void* k, size_t klen);

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void leveldb_comparator_create_destructor(void* state);

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int leveldb_comparator_create_compare(void* state, void* a, size_t alen, void* b, size_t blen);

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void* leveldb_comparator_create_name(void* state);

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void leveldb_filterpolicy_create_destructor(void* p0);

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void* leveldb_filterpolicy_create_create_filter(void* p0, void** key_array, size_t* key_length_array, int num_keys, out size_t filter_length);

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool leveldb_filterpolicy_create_key_may_match(void* p0, void* key, size_t length, void* filter, size_t filter_length);

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void* leveldb_filterpolicy_create_name(void* p0);

        [SuppressUnmanagedCodeSecurity]
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void leveldb_get_callback(void* value, size_t value_length);

        const string Dll = "leveldb";



        [DllImport(Dll, EntryPoint = "leveldb_open", CharSet = CharSet.Ansi)]
        public static extern leveldb_t* open(leveldb_options_t* options, string name, out IntPtr errptr);

        [DllImport(Dll, EntryPoint = "leveldb_close")]
        public static extern void close(leveldb_t* db);

        [DllImport(Dll, EntryPoint = "leveldb_put")]
        public static extern void put(leveldb_t* db, leveldb_writeoptions_t* options, void* key, size_t keylen, void* val, size_t vallen, out IntPtr errptr);

        [DllImport(Dll, EntryPoint = "leveldb_delete")]
        public static extern void delete(leveldb_t* db, leveldb_writeoptions_t* options, void* key, size_t keylen, out IntPtr errptr);

        [DllImport(Dll, EntryPoint = "leveldb_write")]
        public static extern void write(leveldb_t* db, leveldb_writeoptions_t* options, leveldb_writebatch_t* batch, out IntPtr errptr);

        [DllImport(Dll, EntryPoint = "leveldb_get")]
        public static extern void* get(leveldb_t* db, leveldb_readoptions_t* options, void* key, size_t keylen, out size_t vallen, out IntPtr errptr);

        [DllImport(Dll, EntryPoint = "leveldb_get2")]
        public static extern bool get(leveldb_t* db, leveldb_readoptions_t* options, void* key, size_t keylen, leveldb_get_callback callback, out IntPtr errptr);

        [DllImport(Dll, EntryPoint = "leveldb_create_iterator")]
        public static extern leveldb_iterator_t* create_iterator(leveldb_t* db, leveldb_readoptions_t* options);

        [DllImport(Dll, EntryPoint = "leveldb_create_snapshot")]
        public static extern leveldb_snapshot_t* create_snapshot(leveldb_t* db);

        [DllImport(Dll, EntryPoint = "leveldb_release_snapshot")]
        public static extern void release_snapshot(leveldb_t* db, leveldb_snapshot_t* snapshot);

        [DllImport(Dll, EntryPoint = "leveldb_property_value", CharSet = CharSet.Ansi)]
        public static extern void* property_value(leveldb_t* db, string propname);

        [DllImport(Dll, EntryPoint = "leveldb_approximate_sizes")]
        public static extern void approximate_sizes(leveldb_t* db, int num_ranges, void** range_start_key, size_t* range_start_key_len, void** range_limit_key, size_t* range_limit_key_len, ulong* sizes);

        [DllImport(Dll, EntryPoint = "leveldb_compact_range")]
        public static extern void compact_range(leveldb_t* db, void* start_key, size_t start_key_len, void* limit_key, size_t limit_key_len);

        [DllImport(Dll, EntryPoint = "leveldb_destroy_db", CharSet = CharSet.Ansi)]
        public static extern void destroy_db(leveldb_options_t* options, string name, out IntPtr errptr);

        [DllImport(Dll, EntryPoint = "leveldb_repair_db", CharSet = CharSet.Ansi)]
        public static extern void repair_db(leveldb_options_t* options, string name, out IntPtr errptr);

        [DllImport(Dll, EntryPoint = "leveldb_iter_destroy")]
        public static extern void iter_destroy(leveldb_iterator_t* p0);

        [DllImport(Dll, EntryPoint = "leveldb_iter_valid")]
        public static extern bool iter_valid(leveldb_iterator_t* p0);

        [DllImport(Dll, EntryPoint = "leveldb_iter_seek_to_first")]
        public static extern void iter_seek_to_first(leveldb_iterator_t* p0);

        [DllImport(Dll, EntryPoint = "leveldb_iter_seek_to_last")]
        public static extern void iter_seek_to_last(leveldb_iterator_t* p0);

        [DllImport(Dll, EntryPoint = "leveldb_iter_seek")]
        public static extern void iter_seek(leveldb_iterator_t* p0, void* k, size_t klen);

        [DllImport(Dll, EntryPoint = "leveldb_iter_next")]
        public static extern void iter_next(leveldb_iterator_t* p0);

        [DllImport(Dll, EntryPoint = "leveldb_iter_prev")]
        public static extern void iter_prev(leveldb_iterator_t* p0);

        [DllImport(Dll, EntryPoint = "leveldb_iter_key")]
        public static extern void* iter_key(leveldb_iterator_t* p0, out size_t klen);

        [DllImport(Dll, EntryPoint = "leveldb_iter_value")]
        public static extern void* iter_value(leveldb_iterator_t* p0, out size_t vlen);

        [DllImport(Dll, EntryPoint = "leveldb_iter_get_error")]
        public static extern void iter_get_error(leveldb_iterator_t* p0, out IntPtr errptr);

        [DllImport(Dll, EntryPoint = "leveldb_writebatch_create")]
        public static extern leveldb_writebatch_t* writebatch_create();

        [DllImport(Dll, EntryPoint = "leveldb_writebatch_destroy")]
        public static extern void writebatch_destroy(leveldb_writebatch_t* p0);

        [DllImport(Dll, EntryPoint = "leveldb_writebatch_clear")]
        public static extern void writebatch_clear(leveldb_writebatch_t* p0);

        [DllImport(Dll, EntryPoint = "leveldb_writebatch_put")]
        public static extern void writebatch_put(leveldb_writebatch_t* p0, void* key, size_t klen, void* val, size_t vlen);

        [DllImport(Dll, EntryPoint = "leveldb_writebatch_delete")]
        public static extern void writebatch_delete(leveldb_writebatch_t* p0, void* key, size_t klen);

        [DllImport(Dll, EntryPoint = "leveldb_writebatch_iterate")]
        public static extern void writebatch_iterate(leveldb_writebatch_t* p0, void* state, leveldb_writebatch_iterate_put put, leveldb_writebatch_iterate_deleted deleted);

        [DllImport(Dll, EntryPoint = "leveldb_writebatch_append")]
        public static extern void writebatch_append(leveldb_writebatch_t* destination, leveldb_writebatch_t* source);

        [DllImport(Dll, EntryPoint = "leveldb_options_create")]
        public static extern leveldb_options_t* options_create();

        [DllImport(Dll, EntryPoint = "leveldb_options_destroy")]
        public static extern void options_destroy(leveldb_options_t* p0);

        [DllImport(Dll, EntryPoint = "leveldb_options_set_comparator")]
        public static extern void options_set_comparator(leveldb_options_t* p0, leveldb_comparator_t* p1);

        [DllImport(Dll, EntryPoint = "leveldb_options_set_filter_policy")]
        public static extern void options_set_filter_policy(leveldb_options_t* p0, leveldb_filterpolicy_t* p1);

        [DllImport(Dll, EntryPoint = "leveldb_options_set_create_if_missing")]
        public static extern void options_set_create_if_missing(leveldb_options_t* p0, bool p1);

        [DllImport(Dll, EntryPoint = "leveldb_options_set_error_if_exists")]
        public static extern void options_set_error_if_exists(leveldb_options_t* p0, bool p1);

        [DllImport(Dll, EntryPoint = "leveldb_options_set_paranoid_checks")]
        public static extern void options_set_paranoid_checks(leveldb_options_t* p0, bool p1);

        [DllImport(Dll, EntryPoint = "leveldb_options_set_env")]
        public static extern void options_set_env(leveldb_options_t* p0, leveldb_env_t* p1);

        [DllImport(Dll, EntryPoint = "leveldb_options_set_info_log")]
        public static extern void options_set_info_log(leveldb_options_t* p0, leveldb_logger_t* p1);

        [DllImport(Dll, EntryPoint = "leveldb_options_set_write_buffer_size")]
        public static extern void options_set_write_buffer_size(leveldb_options_t* p0, size_t p1);

        [DllImport(Dll, EntryPoint = "leveldb_options_set_max_open_files")]
        public static extern void options_set_max_open_files(leveldb_options_t* p0, int p1);

        [DllImport(Dll, EntryPoint = "leveldb_options_set_cache")]
        public static extern void options_set_cache(leveldb_options_t* p0, leveldb_cache_t* p1);

        [DllImport(Dll, EntryPoint = "leveldb_options_set_block_size")]
        public static extern void options_set_block_size(leveldb_options_t* p0, size_t p1);

        [DllImport(Dll, EntryPoint = "leveldb_options_set_block_restart_interval")]
        public static extern void options_set_block_restart_interval(leveldb_options_t* p0, int p1);

        [DllImport(Dll, EntryPoint = "leveldb_options_set_max_file_size")]
        public static extern void options_set_max_file_size(leveldb_options_t* p0, size_t p1);

        [DllImport(Dll, EntryPoint = "leveldb_options_set_compression")]
        public static extern void options_set_compression(leveldb_options_t* p0, bool p1);

        [DllImport(Dll, EntryPoint = "leveldb_comparator_create")]
        public static extern leveldb_comparator_t* comparator_create(void* state, leveldb_comparator_create_destructor destructor, leveldb_comparator_create_compare compare, leveldb_comparator_create_name name);

        [DllImport(Dll, EntryPoint = "leveldb_comparator_destroy")]
        public static extern void comparator_destroy(leveldb_comparator_t* p0);

        [DllImport(Dll, EntryPoint = "leveldb_filterpolicy_create")]
        public static extern leveldb_filterpolicy_t* filterpolicy_create(void* state, leveldb_filterpolicy_create_destructor destructor, leveldb_filterpolicy_create_create_filter create_filter, leveldb_filterpolicy_create_key_may_match key_may_match, leveldb_filterpolicy_create_name name);

        [DllImport(Dll, EntryPoint = "leveldb_filterpolicy_destroy")]
        public static extern void filterpolicy_destroy(leveldb_filterpolicy_t* p0);

        [DllImport(Dll, EntryPoint = "leveldb_filterpolicy_create_bloom")]
        public static extern leveldb_filterpolicy_t* filterpolicy_create_bloom(int bits_per_key);

        [DllImport(Dll, EntryPoint = "leveldb_readoptions_create")]
        public static extern leveldb_readoptions_t* readoptions_create();

        [DllImport(Dll, EntryPoint = "leveldb_readoptions_destroy")]
        public static extern void readoptions_destroy(leveldb_readoptions_t* p0);

        [DllImport(Dll, EntryPoint = "leveldb_readoptions_set_verify_checksums")]
        public static extern void readoptions_set_verify_checksums(leveldb_readoptions_t* p0, bool p1);

        [DllImport(Dll, EntryPoint = "leveldb_readoptions_set_fill_cache")]
        public static extern void readoptions_set_fill_cache(leveldb_readoptions_t* p0, bool p1);

        [DllImport(Dll, EntryPoint = "leveldb_readoptions_set_snapshot")]
        public static extern void readoptions_set_snapshot(leveldb_readoptions_t* p0, leveldb_snapshot_t* p1);

        [DllImport(Dll, EntryPoint = "leveldb_writeoptions_create")]
        public static extern leveldb_writeoptions_t* writeoptions_create();

        [DllImport(Dll, EntryPoint = "leveldb_writeoptions_destroy")]
        public static extern void writeoptions_destroy(leveldb_writeoptions_t* p0);

        [DllImport(Dll, EntryPoint = "leveldb_writeoptions_set_sync")]
        public static extern void writeoptions_set_sync(leveldb_writeoptions_t* p0, bool p1);

        [DllImport(Dll, EntryPoint = "leveldb_cache_create_lru")]
        public static extern leveldb_cache_t* cache_create_lru(size_t capacity);

        [DllImport(Dll, EntryPoint = "leveldb_cache_destroy")]
        public static extern void cache_destroy(leveldb_cache_t* cache);

        [DllImport(Dll, EntryPoint = "leveldb_create_default_env")]
        public static extern leveldb_env_t* create_default_env();

        [DllImport(Dll, EntryPoint = "leveldb_env_destroy")]
        public static extern void env_destroy(leveldb_env_t* p0);

        [DllImport(Dll, EntryPoint = "leveldb_env_get_test_directory")]
        public static extern void* env_get_test_directory(leveldb_env_t* p0);

        [DllImport(Dll, EntryPoint = "leveldb_free")]
        public static extern void free(void* ptr);

        [DllImport(Dll, EntryPoint = "leveldb_major_version")]
        public static extern int major_version();

        [DllImport(Dll, EntryPoint = "leveldb_minor_version")]
        public static extern int minor_version();
    }
}

#pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments
#pragma warning restore CA1401 // P/Invokes should not be visible
#pragma warning restore CA1034 // Nested types should not be visible
#pragma warning restore IDE1006 // 命名样式