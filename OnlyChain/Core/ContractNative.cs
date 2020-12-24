#nullable enable

using OnlyChain.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable IDE1006 // 命名样式

namespace OnlyChain.Core {
    unsafe static class ContractNative {
        [StructLayout(LayoutKind.Sequential)]
        public struct FuncPtr {
            private readonly IntPtr ptr;

            public TDelegate ToDelegate<TDelegate>() where TDelegate : Delegate {
                // if (ptr == IntPtr.Zero) return null;
                return Marshal.GetDelegateForFunctionPointer<TDelegate>(ptr);
            }

            public TDelegate? ToDelegateNullable<TDelegate>() where TDelegate : Delegate {
                if (ptr == IntPtr.Zero) return null;
                return Marshal.GetDelegateForFunctionPointer<TDelegate>(ptr);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct evmc_vm {
            public readonly int AbiVersion;
            readonly byte* namePtr;
            readonly byte* versionPtr;
            readonly FuncPtr DestroyPtr;
            readonly FuncPtr ExecutePtr;
            readonly FuncPtr GetCapabilitiesPtr;
            readonly FuncPtr SetOptionPtr;

            public string Name => Marshal.PtrToStringAnsi((IntPtr)namePtr)!;
            public string Version => Marshal.PtrToStringAnsi((IntPtr)versionPtr)!;
            public VMDestroyDelegate Destroy => DestroyPtr.ToDelegate<VMDestroyDelegate>();
            public VMExecuteDelegate Execute => ExecutePtr.ToDelegate<VMExecuteDelegate>();
            public VMGetCapabilitiesDelegate GetCapabilities => GetCapabilitiesPtr.ToDelegate<VMGetCapabilitiesDelegate>();
            public VMSetOptionDelegate SetOption => SetOptionPtr.ToDelegate<VMSetOptionDelegate>();
        }

        public delegate void VMDestroyDelegate(evmc_vm* vm);
        public delegate evmc_result VMExecuteDelegate(evmc_vm* vm, evmc_host_interface* host, evmc_host_context* context, int revision, evmc_message* msg, byte* code, nuint codeSize);
        public delegate uint VMGetCapabilitiesDelegate(evmc_vm* vm);
        public delegate int VMSetOptionDelegate(evmc_vm* vm, [MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string value);

        [StructLayout(LayoutKind.Sequential)]
        public struct evmc_result {
            public evmc_status_code StatusCode;
            public ulong GasLeft;
            public byte* OutputData;
            public nuint OutputSize;
            public FuncPtr ReleasePtr;
            public Bytes<Address> CreateAddress;
            fixed byte Padding[4];

            public ResultReleaseDelegate? Release => ReleasePtr.ToDelegateNullable<ResultReleaseDelegate>();
        }

        public delegate void ResultReleaseDelegate(evmc_result* result);

        [StructLayout(LayoutKind.Sequential)]
        public struct evmc_message {
            public int Kind;
            public uint Flags;
            public int Depth;
            public long Gas;
            public Bytes<Address> Destination;
            public Bytes<Address> Sender;
            public void* InputData;
            public nuint InputSize;
            public Bytes<U256> Value;
            public Bytes<U256> Create2Salt;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct evmc_host_interface {
            IntPtr _account_exists;
            IntPtr _get_storage;
            IntPtr _set_storage;
            IntPtr _get_balance;
            IntPtr _get_code_size;
            IntPtr _get_code_hash;
            IntPtr _copy_code;
            IntPtr _selfdestruct;
            IntPtr _call;
            IntPtr _get_tx_context;
            IntPtr _get_block_hash;
            IntPtr _emit_log;
            IntPtr _calc_hash;


            public account_exists_fn account_exists { set => _account_exists = Marshal.GetFunctionPointerForDelegate(value); }
            public get_storage_fn get_storage { set => _get_storage = Marshal.GetFunctionPointerForDelegate(value); }
            public set_storage_fn set_storage { set => _set_storage = Marshal.GetFunctionPointerForDelegate(value); }
            public get_balance_fn get_balance { set => _get_balance = Marshal.GetFunctionPointerForDelegate(value); }
            public get_code_size_fn get_code_size { set => _get_code_size = Marshal.GetFunctionPointerForDelegate(value); }
            public get_code_hash_fn get_code_hash { set => _get_code_hash = Marshal.GetFunctionPointerForDelegate(value); }
            public copy_code_fn copy_code { set => _copy_code = Marshal.GetFunctionPointerForDelegate(value); }
            public selfdestruct_fn selfdestruct { set => _selfdestruct = Marshal.GetFunctionPointerForDelegate(value); }
            public call_fn call { set => _call = Marshal.GetFunctionPointerForDelegate(value); }
            public get_tx_context_fn get_tx_context { set => _get_tx_context = Marshal.GetFunctionPointerForDelegate(value); }
            public get_block_hash_fn get_block_hash { set => _get_block_hash = Marshal.GetFunctionPointerForDelegate(value); }
            public emit_log_fn emit_log { set => _emit_log = Marshal.GetFunctionPointerForDelegate(value); }
            public calc_hash_fn calc_hash { set => _calc_hash = Marshal.GetFunctionPointerForDelegate(value); }



            public delegate byte account_exists_fn(evmc_host_context* c, in Bytes<Address> addr);
            public delegate Bytes<U256> get_storage_fn(evmc_host_context* c, in Bytes<Address> addr, in Bytes<U256> key);
            public delegate evmc_storage_status set_storage_fn(evmc_host_context* c, in Bytes<Address> addr, in Bytes<U256> key, in Bytes<U256> value);
            public delegate Bytes<U256> get_balance_fn(evmc_host_context* c, in Bytes<Address> addr);
            public delegate nuint get_code_size_fn(evmc_host_context* c, in Bytes<Address> addr);
            public delegate Bytes<Hash256> get_code_hash_fn(evmc_host_context* c, in Bytes<Address> addr);
            public delegate nuint copy_code_fn(evmc_host_context* c, in Bytes<Address> addr, nuint code_offset, byte* buffer, nuint buffer_size);
            public delegate void selfdestruct_fn(evmc_host_context* c, in Bytes<Address> addr, in Bytes<Address> beneficiary);
            public delegate evmc_result call_fn(evmc_host_context* c, evmc_message* msg);
            public delegate evmc_tx_context get_tx_context_fn(evmc_host_context* c);
            public delegate Bytes<Hash256> get_block_hash_fn(evmc_host_context* c, long number);
            public delegate void emit_log_fn(evmc_host_context* c, in Bytes<Address> addr, byte* data, nuint data_size, Bytes<U256>* topics, nuint topics_count);
            public delegate Bytes<Hash256> calc_hash_fn(byte* data, nuint data_size);
        }

        public struct evmc_host_context {

        }

        [StructLayout(LayoutKind.Sequential)]
        public struct evmc_tx_context {
            public Bytes<U256> tx_gas_price;
            public Bytes<Address> tx_origin;
            public Bytes<Address> block_coinbase;
            public long block_number;
            public long block_timestamp;
            public long block_gas_limit;
            public Bytes<U256> block_difficulty;
            public Bytes<U256> chain_id;
        }

        public enum evmc_storage_status : int {
            /// <summary>
            /// The value of a storage item has been left unchanged: 0 -> 0 and X -> X.
            /// </summary>
            EVMC_STORAGE_UNCHANGED = 0,
            /// <summary>
            /// The value of a storage item has been modified: X -> Y.
            /// </summary>
            EVMC_STORAGE_MODIFIED = 1,
            /// <summary>
            /// A storage item has been modified after being modified before: X -> Y -> Z.
            /// </summary>
            EVMC_STORAGE_MODIFIED_AGAIN = 2,
            /// <summary>
            /// A new storage item has been added: 0 -> X.
            /// </summary>
            EVMC_STORAGE_ADDED = 3,
            /// <summary>
            /// A storage item has been deleted: X -> 0.
            /// </summary>
            EVMC_STORAGE_DELETED = 4
        }


        [DllImport("evmone", EntryPoint = "evmc_create_evmone")]
        extern static evmc_vm* CreateVM();

        public static readonly evmc_vm* VM = CreateVM();


        public const int EVMC_MAX_REVISION = 8;

        public enum evmc_status_code {
            /** Execution finished with success. */
            EVMC_SUCCESS = 0,

            /** Generic execution failure. */
            EVMC_FAILURE = 1,

            /**
             * Execution terminated with REVERT opcode.
             *
             * In this case the amount of gas left MAY be non-zero and additional output
             * data MAY be provided in ::evmc_result.
             */
            EVMC_REVERT = 2,

            /** The execution has run out of gas. */
            EVMC_OUT_OF_GAS = 3,

            /**
             * The designated INVALID instruction has been hit during execution.
             *
             * The EIP-141 (https://github.com/ethereum/EIPs/blob/master/EIPS/eip-141.md)
             * defines the instruction 0xfe as INVALID instruction to indicate execution
             * abortion coming from high-level languages. This status code is reported
             * in case this INVALID instruction has been encountered.
             */
            EVMC_INVALID_INSTRUCTION = 4,

            /** An undefined instruction has been encountered. */
            EVMC_UNDEFINED_INSTRUCTION = 5,

            /**
             * The execution has attempted to put more items on the EVM stack
             * than the specified limit.
             */
            EVMC_STACK_OVERFLOW = 6,

            /** Execution of an opcode has required more items on the EVM stack. */
            EVMC_STACK_UNDERFLOW = 7,

            /** Execution has violated the jump destination restrictions. */
            EVMC_BAD_JUMP_DESTINATION = 8,

            /**
             * Tried to read outside memory bounds.
             *
             * An example is RETURNDATACOPY reading past the available buffer.
             */
            EVMC_INVALID_MEMORY_ACCESS = 9,

            /** Call depth has exceeded the limit (if any) */
            EVMC_CALL_DEPTH_EXCEEDED = 10,

            /** Tried to execute an operation which is restricted in static mode. */
            EVMC_STATIC_MODE_VIOLATION = 11,

            /**
             * A call to a precompiled or system contract has ended with a failure.
             *
             * An example: elliptic curve functions handed invalid EC points.
             */
            EVMC_PRECOMPILE_FAILURE = 12,

            /**
             * Contract validation has failed (e.g. due to EVM 1.5 jump validity,
             * Casper's purity checker or ewasm contract rules).
             */
            EVMC_CONTRACT_VALIDATION_FAILURE = 13,

            /**
             * An argument to a state accessing method has a value outside of the
             * accepted range of values.
             */
            EVMC_ARGUMENT_OUT_OF_RANGE = 14,

            /**
             * A WebAssembly `unreachable` instruction has been hit during execution.
             */
            EVMC_WASM_UNREACHABLE_INSTRUCTION = 15,

            /**
             * A WebAssembly trap has been hit during execution. This can be for many
             * reasons, including division by zero, validation errors, etc.
             */
            EVMC_WASM_TRAP = 16,

            /** EVM implementation generic internal error. */
            EVMC_INTERNAL_ERROR = -1,

            /**
             * The execution of the given code and/or message has been rejected
             * by the EVM implementation.
             *
             * This error SHOULD be used to signal that the EVM is not able to or
             * willing to execute the given code type or message.
             * If an EVM returns the ::EVMC_REJECTED status code,
             * the Client MAY try to execute it in other EVM implementation.
             * For example, the Client tries running a code in the EVM 1.5. If the
             * code is not supported there, the execution falls back to the EVM 1.0.
             */
            EVMC_REJECTED = -2,

            /** The VM failed to allocate the amount of memory needed for execution. */
            EVMC_OUT_OF_MEMORY = -3
        }
    }

}

#pragma warning restore IDE1006 // 命名样式