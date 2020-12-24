#nullable enable

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OnlyChain.Core {
    internal static class MerklePatriciaTreeSupport {
        public static readonly NotSupportedException NotSupportedException = new NotSupportedException();

        public interface IBlock { }

        [StructLayout(LayoutKind.Sequential, Size = 1)] public struct Block1 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 2)] public struct Block2 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 3)] public struct Block3 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 4)] public struct Block4 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 5)] public struct Block5 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 6)] public struct Block6 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 7)] public struct Block7 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 8)] public struct Block8 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 9)] public struct Block9 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 10)] public struct Block10 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 11)] public struct Block11 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 12)] public struct Block12 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 13)] public struct Block13 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 14)] public struct Block14 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 15)] public struct Block15 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 16)] public struct Block16 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 17)] public struct Block17 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 18)] public struct Block18 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 19)] public struct Block19 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 20)] public struct Block20 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 21)] public struct Block21 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 22)] public struct Block22 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 23)] public struct Block23 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 24)] public struct Block24 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 25)] public struct Block25 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 26)] public struct Block26 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 27)] public struct Block27 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 28)] public struct Block28 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 29)] public struct Block29 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 30)] public struct Block30 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 31)] public struct Block31 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 32)] public struct Block32 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 33)] public struct Block33 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 34)] public struct Block34 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 35)] public struct Block35 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 36)] public struct Block36 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 37)] public struct Block37 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 38)] public struct Block38 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 39)] public struct Block39 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 40)] public struct Block40 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 41)] public struct Block41 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 42)] public struct Block42 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 43)] public struct Block43 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 44)] public struct Block44 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 45)] public struct Block45 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 46)] public struct Block46 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 47)] public struct Block47 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 48)] public struct Block48 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 49)] public struct Block49 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 50)] public struct Block50 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 51)] public struct Block51 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 52)] public struct Block52 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 53)] public struct Block53 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 54)] public struct Block54 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 55)] public struct Block55 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 56)] public struct Block56 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 57)] public struct Block57 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 58)] public struct Block58 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 59)] public struct Block59 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 60)] public struct Block60 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 61)] public struct Block61 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 62)] public struct Block62 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 63)] public struct Block63 : IBlock { }
        [StructLayout(LayoutKind.Sequential, Size = 64)] public struct Block64 : IBlock { }

        public interface ISoleReadOnlyList<T> : IList<T>, IReadOnlyList<T>, IEnumerator<T> {
            bool Got { get; set; }

            int ICollection<T>.Count => 1;
            bool ICollection<T>.IsReadOnly => true;
            void ICollection<T>.Add(T item) => throw NotSupportedException;
            bool ICollection<T>.Remove(T item) => throw NotSupportedException;
            void ICollection<T>.Clear() => throw NotSupportedException;
            bool ICollection<T>.Contains(T item) => EqualityComparer<T>.Default.Equals(Current, item);
            void ICollection<T>.CopyTo(T[] array, int arrayIndex) {
                if ((uint)arrayIndex >= (uint)array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
                array[arrayIndex] = Current;
            }

            int IReadOnlyCollection<T>.Count => 1;

            T IReadOnlyList<T>.this[int index] => index == 0 ? Current : throw new ArgumentOutOfRangeException(nameof(index));

            T IList<T>.this[int index] {
                get => index == 0 ? Current : throw new ArgumentOutOfRangeException(nameof(index));
                set => throw NotSupportedException;
            }
            int IList<T>.IndexOf(T item) => EqualityComparer<T>.Default.Equals(Current, item) ? 0 : -1;
            void IList<T>.RemoveAt(int index) => throw NotSupportedException;
            void IList<T>.Insert(int index, T item) => throw NotSupportedException;

            object? IEnumerator.Current => Current;
            IEnumerator IEnumerable.GetEnumerator() => this;
            void IEnumerator.Reset() => throw NotSupportedException;
            bool IEnumerator.MoveNext() => Got ? false : (Got = true);

            IEnumerator<T> IEnumerable<T>.GetEnumerator() => this;

            void IDisposable.Dispose() => Got = false;
        }

        public sealed class SoleList<T> : ISoleReadOnlyList<T> {
            bool ISoleReadOnlyList<T>.Got { get; set; } = false;
            public T Current { get; }

            public SoleList(T value) => Current = value;
        }
    }

    unsafe partial class MerklePatriciaTree<TKey, TValue, THash> {
        internal static class Support {
            delegate Node CreateNodeHandler(int generation, byte* key, Node value);

            static readonly CreateNodeHandler[] createNodeHandlers = {
                (generation, key, child) => child,
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block1>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block2>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block3>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block4>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block5>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block6>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block7>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block8>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block9>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block10>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block11>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block12>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block13>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block14>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block15>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block16>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block17>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block18>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block19>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block20>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block21>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block22>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block23>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block24>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block25>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block26>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block27>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block28>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block29>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block30>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block31>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block32>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block33>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block34>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block35>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block36>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block37>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block38>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block39>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block40>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block41>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block42>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block43>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block44>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block45>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block46>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block47>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block48>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block49>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block50>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block51>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block52>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block53>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block54>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block55>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block56>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block57>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block58>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block59>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block60>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block61>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block62>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block63>(generation, key, child),
                (generation, key, child) => new LongPathNode<MerklePatriciaTreeSupport.Block64>(generation, key, child),
            };

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static ValueNode CreateValue(ref AddArgs args) {
                args.Result = true;
                args.Update = false;
                return new ValueNode(args.Generation, args.Value);
            }

            public ref struct AddArgs {
                public TValue Value;
                public int Generation;
                public bool Result;
                public bool Update;
            }

            public ref struct RemoveArgs {
                public TValue Value;
                public int Generation;
                public bool Result;
                public bool NeedCompareValue;
                public bool NeedSetValue;
            }

            public ref struct ComputeHashArgs {
                public byte* Key;
                public IHashAlgorithm HashAlgorithm;
                public int Generation;
            }

            public ref struct FindFillHashArgs {
                public byte* Key;
                public bool Result;
                public bool LeftBound;
                public bool RightBound;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static Node CreateLongPathNode(int generation, byte* key, int length, Node child) => createNodeHandlers[length](generation, key, child);


            public abstract class Node {
                public readonly int Generation;
                public THash Hash;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public Node(int generation) {
                    Generation = generation;
                }

                public abstract Node Add(ref AddArgs args, byte* key, int length);
                public abstract IEnumerable<KeyValuePair<TKey, TValue>> Enumerate(int index, byte[] key);
                public abstract Node? Remove(ref RemoveArgs args, byte* key);
                public abstract ref TValue TryGetValue(int* generation, byte* key);
                public virtual Node PrefixConcat<TBlock>(LongPathNode<TBlock> parent) where TBlock : unmanaged, MerklePatriciaTreeSupport.IBlock => parent;
                public abstract void ComputeHash(ref ComputeHashArgs args, int index);
                public abstract IHashNode FindFillHash(ref FindFillHashArgs args, int index);
            }


            public sealed class EmptyNode : Node {
                private Node? child;

                public EmptyNode(int generation) : base(generation) => child = null;

                public override Node Add(ref AddArgs args, byte* key, int length) {
                    if (child is null) {
                        child = CreateLongPathNode(args.Generation, key, length, CreateValue(ref args));
                    } else {
                        child = child.Add(ref args, key, length);
                    }
                    return this;
                }

                public override IEnumerable<KeyValuePair<TKey, TValue>> Enumerate(int index, byte[] key) {
                    return child is null ? Enumerable.Empty<KeyValuePair<TKey, TValue>>() : child.Enumerate(index, key);
                }

                public override Node? Remove(ref RemoveArgs args, byte* key) {
                    child = child?.Remove(ref args, key);
                    return this;
                }

                public override ref TValue TryGetValue(int* generation, byte* key) {
                    if (child is null) {
                        return ref Unsafe.AsRef<TValue>(null);
                    }
                    return ref child.TryGetValue(generation, key);
                }

                public override IHashNode FindFillHash(ref FindFillHashArgs args, int index = 0) {
                    if (child is null) return new TreeHashNode(Hash);
                    return child.FindFillHash(ref args, index);
                }

                public override void ComputeHash(ref ComputeHashArgs args, int index) {
                    if (child is null) {
                        Hash = args.HashAlgorithm.ComputeHash(ReadOnlySpan<THash>.Empty);
                    } else {
                        if (child.Generation == args.Generation) {
                            child.ComputeHash(ref args, index);
                        }
                        Hash = child.Hash;
                    }
                }

                public EmptyNode Clear() {
                    child = null;
                    return this;
                }

                public EmptyNode Clone(int generation) => new EmptyNode(generation) { child = child };
            }

            public sealed class PrefixMapNode : Node {
                [StructLayout(LayoutKind.Sequential)]
                struct Children {
                    [SuppressMessage("样式", "IDE0044:添加只读修饰符", Justification = "<挂起>")]
                    public Node? child_0,
                                 child_1,
                                 child_2,
                                 child_3,
                                 child_4,
                                 child_5,
                                 child_6,
                                 child_7,
                                 child_8,
                                 child_9,
                                 child_a,
                                 child_b,
                                 child_c,
                                 child_d,
                                 child_e,
                                 child_f;
                }

                private Children children;


                internal ref Node? this[int i] => ref Unsafe.Add(ref children.child_0, i);

                public PrefixMapNode(int generation) : base(generation) { }

                private PrefixMapNode(int generation, PrefixMapNode other) : base(generation) {
                    children = other.children;
                }

                public override ref TValue TryGetValue(int* generation, byte* key) {
                    if (this[*key] is Node child) {
                        return ref child.TryGetValue(generation, key + 1);
                    }
                    return ref Unsafe.AsRef<TValue>(null);
                }

                public override IHashNode FindFillHash(ref FindFillHashArgs args, int index) {
                    byte* childIndexes = stackalloc byte[16];
                    int childCount = 0;
                    for (int i = 0; i < 16; i++) {
                        if (this[i] is { }) {
                            childIndexes[childCount++] = (byte)i;
                        }
                    }

                    var resultChildren = new IHashNode[childCount];
                    if (!(args.LeftBound | args.RightBound)) {
                        int leftIndex = -1, rightIndex = -1;
                        IHashNode? leftNode = null, rightNode = null;
                        byte currKey = args.Key[index];
                        Node? child = this[currKey];

                        if (child is Node) {
                            var findChild = child.FindFillHash(ref args, index + 1);
                            if (!(args.LeftBound | args.RightBound)) {
                                for (int i = 0; i < resultChildren.Length; i++) {
                                    if (childIndexes[i] != currKey) {
                                        resultChildren[i] = new HashHashNode(this[childIndexes[i]]!.Hash);
                                    } else {
                                        resultChildren[i] = findChild;
                                    }
                                }
                                goto Result;
                            } else if (args.LeftBound) {
                                rightIndex = currKey;
                                rightNode = findChild;
                            } else {
                                leftIndex = currKey;
                                leftNode = findChild;
                            }
                        } else {
                            args.LeftBound = true;
                            args.RightBound = true;
                        }

                        if (args.LeftBound) {
                            for (int i = currKey - 1; i >= 0; i--) {
                                child = this[i];
                                if (child is Node) {
                                    args.Key[index] = (byte)i;
                                    var findRight = args.RightBound;
                                    args.RightBound = false;
                                    leftNode = child.FindFillHash(ref args, index + 1);
                                    args.RightBound = findRight;
                                    leftIndex = i;
                                    break;
                                }
                            }
                        }
                        if (args.RightBound) {
                            for (int i = currKey + 1; i < 16; i++) {
                                child = this[i];
                                if (child is Node) {
                                    args.Key[index] = (byte)i;
                                    var findLeft = args.LeftBound;
                                    args.LeftBound = false;
                                    rightNode = child.FindFillHash(ref args, index + 1);
                                    args.LeftBound = findLeft;
                                    rightIndex = i;
                                    break;
                                }
                            }
                        }

                        for (int i = 0; i < resultChildren.Length; i++) {
                            if (childIndexes[i] == leftIndex) {
                                resultChildren[i] = leftNode!;
                            } else if (childIndexes[i] == rightIndex) {
                                resultChildren[i] = rightNode!;
                            } else {
                                resultChildren[i] = new HashHashNode(this[childIndexes[i]]!.Hash);
                            }
                        }
                    } else if (args.LeftBound) {
                        for (int i = 0; i < resultChildren.Length - 1; i++) {
                            resultChildren[i] = new HashHashNode(this[childIndexes[i]]!.Hash);
                        }
                        args.Key[index] = childIndexes[childCount - 1];
                        resultChildren[^1] = this[childIndexes[childCount - 1]]!.FindFillHash(ref args, index + 1);
                    } else {
                        args.Key[index] = childIndexes[0];
                        resultChildren[0] = this[childIndexes[0]]!.FindFillHash(ref args, index + 1);
                        for (int i = 1; i < resultChildren.Length; i++) {
                            resultChildren[i] = new HashHashNode(this[childIndexes[i]]!.Hash);
                        }
                    }

                Result:
                    return new TreeHashNode(Hash, resultChildren);
                }

                public override Node Add(ref AddArgs args, byte* key, int length) {
                    Node? newChild;
                    if (this[*key] is Node child) {
                        newChild = child.Add(ref args, key + 1, length - 1);
                    } else {
                        newChild = CreateLongPathNode(args.Generation, key + 1, length - 1, CreateValue(ref args));
                    }

                    if (args.Result) {
                        if (Generation == args.Generation) {
                            this[*key] = newChild;
                        } else if (this[*key] != newChild) {
                            var clone = new PrefixMapNode(args.Generation, this);
                            clone[*key] = newChild;
                            return clone;
                        }
                    }
                    return this;
                }

                public override IEnumerable<KeyValuePair<TKey, TValue>> Enumerate(int index, byte[] key) {
                    for (int i = 0; i < 16; i++) {
                        if (this[i] is Node child) {
                            key[index] = (byte)i;
                            foreach (var kv in child.Enumerate(index + 1, key)) yield return kv;
                        }
                    }
                }

                public override Node? Remove(ref RemoveArgs args, byte* key) {
                    if (this[*key] is Node child) {
                        Node? newChild = child.Remove(ref args, key + 1);
                        if (!args.Result) return this;
                        if (newChild is { }) {
                            if (Generation == args.Generation) {
                                this[*key] = newChild;
                                return this;
                            } else if (this[*key] != newChild) {
                                var clone = new PrefixMapNode(args.Generation, this);
                                clone[*key] = newChild;
                                return clone;
                            }
                        }
                    } else {
                        return this;
                    }

                    // newChild == null
                    byte p1 = 0xff, p2 = 0xff;
                    for (int i = 0; i < 16; i++) {
                        if (this[i] is Node) {
                            p2 = p1;
                            p1 = (byte)i;
                        }
                    }

                    if (p2 != 0xff) { // 移除后的子节点数量 >= 2
                        if (Generation == args.Generation) {
                            this[*key] = null;
                            return this;
                        } else {
                            var clone = new PrefixMapNode(args.Generation, this);
                            clone[*key] = null;
                            return clone;
                        }
                    }

                    var result = new LongPathNode<MerklePatriciaTreeSupport.Block1>(args.Generation, &p1, this[p1]!);
                    return this[p1]!.PrefixConcat(result);
                }

                [SkipLocalsInit]
                public override void ComputeHash(ref ComputeHashArgs args, int index) {
                    THash* hashes = stackalloc THash[16];
                    int count = 0;
                    for (int i = 0; i < 16; i++) {
                        if (this[i] is Node child) {
                            if (args.Generation == child.Generation) {
                                args.Key[index] = (byte)i;
                                child.ComputeHash(ref args, index + 1);
                            }
                            hashes[count++] = child.Hash;
                        }
                    }
                    Hash = args.HashAlgorithm.ComputeHash(new ReadOnlySpan<THash>(hashes, count));
                }

                public override string ToString() {
                    var list = new List<char>(16);
                    for (int i = 0; i < 16; i++) {
                        if (this[i] is Node) list.Add("0123456789abcdef"[i]);
                    }
                    return $"[{string.Join(",", list)}],count={list.Count}";
                }
            }

            public sealed class BinaryBranchNode : Node {
                private readonly byte prefix1, prefix2;
                private Node child1, child2;

                public BinaryBranchNode(int generation, byte prefix1, byte prefix2, Node child1, Node child2) : base(generation) {
                    if (prefix1 < prefix2) {
                        this.prefix1 = prefix1;
                        this.prefix2 = prefix2;
                        this.child1 = child1;
                        this.child2 = child2;
                    } else {
                        this.prefix1 = prefix2;
                        this.prefix2 = prefix1;
                        this.child1 = child2;
                        this.child2 = child1;
                    }
                }

                public override ref TValue TryGetValue(int* generation, byte* key) {
                    if (*key == prefix1) return ref child1.TryGetValue(generation, key + 1);
                    if (*key == prefix2) return ref child2.TryGetValue(generation, key + 1);
                    return ref Unsafe.AsRef<TValue>(null);
                }

                public override IHashNode FindFillHash(ref FindFillHashArgs args, int index) {
                    if (!(args.LeftBound | args.RightBound)) {
                        if (args.Key[index] < prefix1) {
                            args.RightBound = true;
                            args.Key[index] = prefix1;
                            var rightNode = child1.FindFillHash(ref args, index + 1);
                            args.LeftBound = true;
                            return new TreeHashNode(Hash, rightNode, new HashHashNode(child2.Hash));
                        } else if (args.Key[index] == prefix1) {
                            var findNode = child1.FindFillHash(ref args, index + 1);
                            if (args.RightBound) {
                                args.Key[index] = prefix2;
                                var rightNode = child2.FindFillHash(ref args, index + 1);
                                return new TreeHashNode(Hash, findNode, rightNode);
                            }
                            return new TreeHashNode(Hash, findNode, new HashHashNode(child2.Hash));
                        } else if (args.Key[index] < prefix2) {
                            args.LeftBound = true;
                            args.Key[index] = prefix1;
                            var leftNode = child1.FindFillHash(ref args, index + 1);
                            args.RightBound = true;
                            args.Key[index] = prefix2;
                            var rightNode = child2.FindFillHash(ref args, index + 1);
                            return new TreeHashNode(Hash, leftNode, rightNode);
                        } else if (args.Key[index] == prefix2) {
                            var findNode = child2.FindFillHash(ref args, index + 1);
                            if (args.LeftBound) {
                                args.Key[index] = prefix1;
                                var leftNode = child1.FindFillHash(ref args, index + 1);
                                return new TreeHashNode(Hash, leftNode, findNode);
                            }
                            return new TreeHashNode(Hash, new HashHashNode(child1.Hash), findNode);
                        } else {
                            args.LeftBound = true;
                            args.Key[index] = prefix2;
                            var leftNode = child2.FindFillHash(ref args, index + 1);
                            args.RightBound = true;
                            return new TreeHashNode(Hash, new HashHashNode(child1.Hash), leftNode);
                        }
                    } else if (args.LeftBound) {
                        args.Key[index] = prefix2;
                        var leftNode = child2.FindFillHash(ref args, index + 1);
                        return new TreeHashNode(Hash, new HashHashNode(child1.Hash), leftNode);
                    } else {
                        args.Key[index] = prefix1;
                        var rightNode = child1.FindFillHash(ref args, index + 1);
                        return new TreeHashNode(Hash, rightNode, new HashHashNode(child2.Hash));
                    }
                }

                public override Node Add(ref AddArgs args, byte* key, int length) {
                    if (*key == prefix1) {
                        Node? newChild = child1.Add(ref args, key + 1, length - 1);
                        if (args.Result) {
                            if (Generation == args.Generation) {
                                child1 = newChild;
                            } else if (child1 != newChild) {
                                return new BinaryBranchNode(args.Generation, prefix1, prefix2, newChild, child2);
                            }
                        }
                        return this;
                    }
                    if (*key == prefix2) {
                        Node? newChild = child2.Add(ref args, key + 1, length - 1);
                        if (args.Result) {
                            if (Generation == args.Generation) {
                                child2 = newChild;
                            } else if (child2 != newChild) {
                                return new BinaryBranchNode(args.Generation, prefix1, prefix2, child1, newChild);
                            }
                        }
                        return this;
                    }

                    var result = new PrefixMapNode(args.Generation);
                    result[prefix1] = child1;
                    result[prefix2] = child2;
                    result[*key] = CreateLongPathNode(args.Generation, key + 1, length - 1, CreateValue(ref args));
                    return result;
                }

                public override IEnumerable<KeyValuePair<TKey, TValue>> Enumerate(int index, byte[] key) {
                    key[index] = prefix1;
                    foreach (var kv in child1.Enumerate(index + 1, key)) yield return kv;
                    key[index] = prefix2;
                    foreach (var kv in child2.Enumerate(index + 1, key)) yield return kv;
                }

                public override Node? Remove(ref RemoveArgs args, byte* key) {
                    byte thisPrefix = *key, otherPrefix;
                    ref Node child = ref Unsafe.AsRef<Node>(null);
                    Node otherChild;
                    if (prefix1 == thisPrefix) {
                        otherPrefix = prefix2;
                        child = ref child1;
                        otherChild = child2;
                    } else if (prefix2 == thisPrefix) {
                        otherPrefix = prefix1;
                        child = ref child2;
                        otherChild = child1;
                    } else {
                        return this;
                    }

                    var newChild = child.Remove(ref args, key + 1);
                    if (!args.Result) return this;
                    if (newChild is Node) {
                        if (Generation == args.Generation) {
                            child = newChild;
                        } else if (child != newChild) {
                            return new BinaryBranchNode(args.Generation, thisPrefix, otherPrefix, newChild, otherChild);
                        }
                        return this;
                    }

                    var result = new LongPathNode<MerklePatriciaTreeSupport.Block1>(args.Generation, &otherPrefix, otherChild);
                    return otherChild.PrefixConcat(result);
                }

                [SkipLocalsInit]
                public override void ComputeHash(ref ComputeHashArgs args, int index) {
                    THash* hashPair = stackalloc THash[2];
                    if (child1.Generation == args.Generation) {
                        args.Key[index] = prefix1;
                        child1.ComputeHash(ref args, index + 1);
                    }
                    if (child2.Generation == args.Generation) {
                        args.Key[index] = prefix2;
                        child2.ComputeHash(ref args, index + 1);
                    }
                    hashPair[0] = child1.Hash;
                    hashPair[1] = child2.Hash;
                    Hash = args.HashAlgorithm.ComputeHash(new ReadOnlySpan<THash>(hashPair, 2));
                }

                public override string ToString() {
                    return $"[{"0123456789abcdef"[prefix1]},{"0123456789abcdef"[prefix2]}]";
                }
            }

            public sealed class LongPathNode<TBlock> : Node where TBlock : unmanaged, MerklePatriciaTreeSupport.IBlock {
                sealed class BlockRef {
                    public TBlock Path;

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    public BlockRef(ReadOnlySpan<byte> key) {
                        key.CopyTo(MemoryMarshal.CreateSpan(ref Unsafe.As<TBlock, byte>(ref Path), sizeof(TBlock)));
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    public void PathWriteTo(Span<byte> span) {
                        MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<TBlock, byte>(ref Path), sizeof(TBlock)).CopyTo(span);
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    public bool SequenceEqual(ReadOnlySpan<byte> key) {
                        return key.SequenceEqual(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<TBlock, byte>(ref Path), sizeof(TBlock)));
                    }

                    public ReadOnlySpan<byte> Span {
                        [MethodImpl(MethodImplOptions.AggressiveInlining)]
                        get => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<TBlock, byte>(ref Path), sizeof(TBlock));
                    }
                }

                static readonly int BlockSize = sizeof(TBlock);

                private readonly BlockRef pathRef;
                private Node child;

                public LongPathNode(int generation, byte* key, Node child) : base(generation) {
                    pathRef = new BlockRef(new ReadOnlySpan<byte>(key, sizeof(TBlock)));
                    this.child = child;
                }

                private LongPathNode(int generation, BlockRef pathRef, Node child) : base(generation) {
                    this.pathRef = pathRef;
                    this.child = child;
                }

                public override Node Add(ref AddArgs args, byte* key, int length) {
                    fixed (TBlock* path = &pathRef.Path) {
                        int commonPrefix = 0;
                        for (; commonPrefix < sizeof(TBlock); commonPrefix++) {
                            if (((byte*)path)[commonPrefix] != key[commonPrefix]) goto Split;
                        }

                        Node newChild = child.Add(ref args, key + sizeof(TBlock), length - sizeof(TBlock));
                        if (args.Result) {
                            if (Generation == args.Generation) {
                                child = newChild;
                            } else if (child != newChild) {
                                return new LongPathNode<TBlock>(args.Generation, pathRef, newChild);
                            }
                        }
                        return this;

                    Split:
                        var binaryNode = new BinaryBranchNode(
                            args.Generation,
                            ((byte*)path)[commonPrefix],
                            key[commonPrefix],
                            CreateLongPathNode(args.Generation, (byte*)path + (commonPrefix + 1), sizeof(TBlock) - (commonPrefix + 1), child),
                            CreateLongPathNode(args.Generation, key + (commonPrefix + 1), length - (commonPrefix + 1), CreateValue(ref args))
                        );
                        return CreateLongPathNode(args.Generation, key, commonPrefix, binaryNode);
                    }
                }

                public override IEnumerable<KeyValuePair<TKey, TValue>> Enumerate(int index, byte[] key) {
                    pathRef.PathWriteTo(key.AsSpan(index));
                    foreach (var kv in child.Enumerate(index + BlockSize, key)) yield return kv;
                }

                public override ref TValue TryGetValue(int* generation, byte* key) {
                    if (pathRef.SequenceEqual(new ReadOnlySpan<byte>(key, sizeof(TBlock)))) {
                        return ref child.TryGetValue(generation, key + sizeof(TBlock));
                    }
                    return ref Unsafe.AsRef<TValue>(null);
                }

                public override IHashNode FindFillHash(ref FindFillHashArgs args, int index) {
                    IHashNode findNode;
                    var key = new Span<byte>(args.Key + index, sizeof(TBlock));
                    if (!(args.LeftBound | args.RightBound)) {
                        int cmp = key.SequenceCompareTo(pathRef.Span);
                        if (cmp < 0) {
                            pathRef.PathWriteTo(key);
                            findNode = child.FindFillHash(ref args, index + sizeof(TBlock));
                            args.LeftBound = true;
                        } else if (cmp > 0) {
                            pathRef.PathWriteTo(key);
                            findNode = child.FindFillHash(ref args, index + sizeof(TBlock));
                            args.RightBound = true;
                        } else {
                            findNode = child.FindFillHash(ref args, index + sizeof(TBlock));
                        }
                    } else {
                        pathRef.PathWriteTo(key);
                        findNode = child.FindFillHash(ref args, index + sizeof(TBlock));
                    }
                    return findNode;
                }

                public override Node? Remove(ref RemoveArgs args, byte* key) {
                    if (pathRef.SequenceEqual(new ReadOnlySpan<byte>(key, sizeof(TBlock)))) {
                        var newChild = child.Remove(ref args, key + sizeof(TBlock));
                        if (args.Result) {
                            if (newChild is null) return null;

                            if (Generation == args.Generation) {
                                child = newChild;
                            } else if (child != newChild) {
                                return new LongPathNode<TBlock>(args.Generation, key, newChild);
                            }
                        }
                    }
                    return this;
                }

                public override Node PrefixConcat<TPrefixBlock>(LongPathNode<TPrefixBlock> parent) {
                    var path = stackalloc byte[sizeof(TPrefixBlock) + sizeof(TBlock)];
                    parent.pathRef.PathWriteTo(new Span<byte>(path, sizeof(TPrefixBlock)));
                    pathRef.PathWriteTo(new Span<byte>(path + sizeof(TPrefixBlock), sizeof(TBlock)));
                    return CreateLongPathNode(parent.Generation, path, sizeof(TPrefixBlock) + sizeof(TBlock), child);
                }

                public override void ComputeHash(ref ComputeHashArgs args, int index) {
                    if (child.Generation == args.Generation) {
                        pathRef.PathWriteTo(new Span<byte>(args.Key + index, sizeof(TBlock)));
                        child.ComputeHash(ref args, index + sizeof(TBlock));
                    }
                    Hash = child.Hash;
                }

                public override string ToString() {
                    fixed (TBlock* path = &pathRef.Path) {
                        var chars = stackalloc char[sizeof(TBlock)];
                        for (int i = 0; i < sizeof(TBlock); i++) {
                            chars[i] = "0123456789abcdef"[((byte*)path)[i]];
                        }
                        return new string(chars, 0, sizeof(TBlock)) + ",length=" + sizeof(TBlock);
                    }
                }
            }

            public sealed class ValueNode : Node {
                private TValue value;

                public ValueNode(int generation, TValue value) : base(generation) => this.value = value;

                public override Node Add(ref AddArgs args, byte* key, int length) {
                    if (args.Update) {
                        args.Result = true;
                        if (Generation == args.Generation) {
                            value = args.Value;
                        } else {
                            return new ValueNode(args.Generation, args.Value);
                        }
                    }
                    return this;
                }

                public override ref TValue TryGetValue(int* generation, byte* key) {
                    if (generation != null) *generation = Generation;
                    return ref value;
                }

                public override IHashNode FindFillHash(ref FindFillHashArgs args, int index) {
                    if (!(args.LeftBound | args.RightBound)) {
                        args.Result = true;
                    }
                    args.LeftBound = false;
                    args.RightBound = false;
                    TKey key;
                    BufferToKey(args.Key, &key);
                    return new ValueHashNode(Hash, key, value);
                }

                public override IEnumerable<KeyValuePair<TKey, TValue>> Enumerate(int index, byte[] key) {
                    TKey tempKey;
                    fixed (byte* buffer = key) {
                        BufferToKey(buffer, &tempKey);
                    }
                    return new MerklePatriciaTreeSupport.SoleList<KeyValuePair<TKey, TValue>>(new KeyValuePair<TKey, TValue>(tempKey, value));
                }

                public override Node? Remove(ref RemoveArgs args, byte* key) {
                    if (args.NeedCompareValue && !EqualityComparer<TValue>.Default.Equals(value, args.Value)) {
                        return this;
                    }
                    if (args.NeedSetValue) args.Value = value;
                    args.Result = true;
                    return null;
                }

                public override void ComputeHash(ref ComputeHashArgs args, int index) {
                    TKey key;
                    BufferToKey(args.Key, &key);
                    Hash = args.HashAlgorithm.ComputeHash(key, value);
                }

                public override string? ToString() => value?.ToString();
            }

            [System.Diagnostics.DebuggerDisplay("Children Count: {Children.Count}")]
            public sealed class TreeHashNode : IHashNode {
                public HashNodeType Type => HashNodeType.Node;
                public THash Hash { get; }
                public IReadOnlyList<IHashNode> Children { get; }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public TreeHashNode(THash hash, params IHashNode[] children) => (Hash, Children) = (hash, children);
            }

            [System.Diagnostics.DebuggerDisplay("Hash:{Hash}")]
            public sealed class HashHashNode : IHashNode {
                public HashNodeType Type => HashNodeType.Hash;
                public THash Hash { get; }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public HashHashNode(THash hash) => Hash = hash;
            }

            [System.Diagnostics.DebuggerDisplay("Key:{Key},Value:{Value}")]
            public sealed class ValueHashNode : IHashNode {
                public HashNodeType Type => HashNodeType.Value;
                public THash Hash { get; }
                public TKey Key { get; }
                public TValue Value { get; }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public ValueHashNode(THash hash, TKey key, TValue value) => (Hash, Key, Value) = (hash, key, value);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static void KeyToBuffer(TKey* key, byte* buffer) {
                for (int i = 0; i < sizeof(TKey); i++) {
                    buffer[2 * i] = (byte)(((byte*)key)[i] >> 4);
                    buffer[2 * i + 1] = (byte)(((byte*)key)[i] & 15);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            public static void BufferToKey(byte* buffer, TKey* key) {
                for (int i = 0; i < sizeof(TKey); i++) {
                    ((byte*)key)[i] = (byte)((buffer[2 * i] << 4) | buffer[2 * i + 1]);
                }
            }
        }

        [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<挂起>")]
        public interface IHashAlgorithm {
            THash ComputeHash(TKey key, in TValue value);
            THash ComputeHash(ReadOnlySpan<THash> hashes);
        }

        [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<挂起>")]
        public interface IHashNode {
            HashNodeType Type { get; }
            THash Hash { get; }
            IReadOnlyList<IHashNode> Children => throw MerklePatriciaTreeSupport.NotSupportedException;
            TKey Key => throw MerklePatriciaTreeSupport.NotSupportedException;
            TValue Value => throw MerklePatriciaTreeSupport.NotSupportedException;

            IHashNode Combine(IHashNode other) {
                if (!EqualityComparer<THash>.Default.Equals(Hash, other.Hash)) throw new InvalidOperationException();

                if (Type is HashNodeType.Node && other.Type is HashNodeType.Node) {
                    var children = new IHashNode[Children.Count];
                    for (int i = 0; i < children.Length; i++) {
                        children[i] = Children[i].Combine(other.Children[i]);
                    }
                    return new Support.TreeHashNode(Hash, children);
                } else if (Type == HashNodeType.Hash) {
                    return other;
                }
                return this;
            }
        }
    }

    unsafe public partial class MerklePatriciaTree<TKey, TValue, THash> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue> where TKey : unmanaged where TValue : notnull where THash : unmanaged {
        private Support.EmptyNode root;

        public int Generation => root.Generation;

        public THash RootHash => root.Hash;

        public int Count { get; private set; }

        public bool IsReadOnly { get; private set; } = false;


        public MerklePatriciaTree(int generation = 0) {
            root = new Support.EmptyNode(generation);
            Count = 0;
        }

        private MerklePatriciaTree(MerklePatriciaTree<TKey, TValue, THash> parentTree) {
            if (!parentTree.IsReadOnly) throw new InvalidOperationException($"{nameof(parentTree)}必须先调用{nameof(ComputeHash)}方法");

            root = parentTree.root.Clone(parentTree.Generation + 1);
            Count = parentTree.Count;
        }

        /// <summary>
        /// 克隆当前MPT，代数+1，并且可修改
        /// </summary>
        /// <returns></returns>
        public MerklePatriciaTree<TKey, TValue, THash> NextNew() => new MerklePatriciaTree<TKey, TValue, THash>(this);

        /// <summary>
        /// 计算整棵树所有节点的Hash值，并使得该<see cref="MerklePatriciaTree{TKey, TValue, THash}"/>对象无法修改。
        /// </summary>
        /// <param name="hashAlgorithm"></param>
        public void ComputeHash(IHashAlgorithm hashAlgorithm) {
            if (!IsReadOnly) {
                var buffer = stackalloc byte[sizeof(TKey) * 2];
                var args = new Support.ComputeHashArgs {
                    Key = buffer,
                    HashAlgorithm = hashAlgorithm,
                    Generation = Generation,
                };
                root.ComputeHash(ref args, 0);
                IsReadOnly = true;
            }
        }

        public bool FindHashTree(TKey key, out IHashNode hashTree) {
            if (!IsReadOnly) throw new InvalidOperationException($"必须先调用{nameof(ComputeHash)}方法");

            var buffer = stackalloc byte[sizeof(TKey) * 2];
            Support.KeyToBuffer(&key, buffer);
            var args = new Support.FindFillHashArgs {
                Key = buffer
            };
            hashTree = root.FindFillHash(ref args);
            return args.Result;
        }

        public TValue this[TKey key] {
            get {
                ref readonly TValue value = ref GetRefValue(key);
                if (Unsafe.AsPointer(ref Unsafe.AsRef(value)) != null) return value;
                throw new KeyNotFoundException($"Key='{key}'不存在");
            }
            set {
                AddOrUpdate(key, value, true);
            }
        }

        public ICollection<TKey> Keys => new KeyEnumerator(this);

        public ICollection<TValue> Values => new ValueEnumerator(this);

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        private bool AddOrUpdate(TKey key, TValue value, bool update) {
            if (IsReadOnly) throw new InvalidOperationException($"此{nameof(MerklePatriciaTree<TKey, TValue, THash>)}是只读的");

            var keyBuffer = stackalloc byte[sizeof(TKey) * 2];
            Support.KeyToBuffer(&key, keyBuffer);

            var args = new Support.AddArgs {
                Generation = Generation,
                Value = value,
                Update = update,
            };
            root.Add(ref args, keyBuffer, sizeof(TKey) * 2);
            if (args.Result & !args.Update) {
                Count++;
            }
            return args.Result;
        }

        public void Add(TKey key, TValue value) {
            if (!AddOrUpdate(key, value, false)) {
                throw new ArgumentException($"Key='{key}'已存在", nameof(key));
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item) {
            Add(item.Key, item.Value);
        }

        public bool TryAdd(TKey key, TValue value) => AddOrUpdate(key, value, false);

        public void Clear() {
            if (IsReadOnly) throw new InvalidOperationException($"此{nameof(MerklePatriciaTree<TKey, TValue, THash>)}是只读的");

            if (Count != 0) {
                root = root.Clear();
                Count = 0;
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            if (TryGetValue(item.Key, out var value)) {
                return EqualityComparer<TValue>.Default.Equals(value, item.Value);
            }
            return false;
        }

        public bool ContainsKey(TKey key) => TryGetValue(key, out _);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            if (arrayIndex < 0 || Count > array.Length - arrayIndex) throw new ArgumentOutOfRangeException();
            int i = 0;
            foreach (var kv in this) {
                array[arrayIndex + i++] = kv;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            if (Count == 0) yield break;

            var buffer = ArrayRent();
            try {
                foreach (var kv in root.Enumerate(0, buffer)) yield return kv;
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            static byte[] ArrayRent() => ArrayPool<byte>.Shared.Rent(sizeof(TKey) * 2);
        }

        public bool Remove(TKey key) {
            if (IsReadOnly) throw new InvalidOperationException($"此{nameof(MerklePatriciaTree<TKey, TValue, THash>)}是只读的");

            if (Count == 0) return false;

            var keyBuffer = stackalloc byte[sizeof(TKey) * 2];
            Support.KeyToBuffer(&key, keyBuffer);

            var args = new Support.RemoveArgs {
                Generation = Generation,
            };
            root.Remove(ref args, keyBuffer);
            if (args.Result) {
                Count--;
                return true;
            }
            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            if (IsReadOnly) throw new InvalidOperationException($"此{nameof(MerklePatriciaTree<TKey, TValue, THash>)}是只读的");

            if (Count == 0) return false;

            TKey key = item.Key;
            var keyBuffer = stackalloc byte[sizeof(TKey) * 2];
            Support.KeyToBuffer(&key, keyBuffer);

            var args = new Support.RemoveArgs {
                Generation = Generation,
                Value = item.Value,
                NeedCompareValue = true,
            };
            root.Remove(ref args, keyBuffer);
            if (args.Result) {
                Count--;
                return true;
            }
            return false;
        }

        public bool Remove(TKey key, out TValue value) {
            if (IsReadOnly) throw new InvalidOperationException($"此{nameof(MerklePatriciaTree<TKey, TValue, THash>)}是只读的");

            if (Count == 0) {
                value = default!;
                return false;
            }

            var keyBuffer = stackalloc byte[sizeof(TKey) * 2];
            Support.KeyToBuffer(&key, keyBuffer);

            var args = new Support.RemoveArgs {
                Generation = Generation,
                NeedSetValue = true,
            };
            root.Remove(ref args, keyBuffer);
            if (args.Result) {
                Count--;
                value = args.Value;
                return true;
            }
            value = default!;
            return false;
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) {
            if (Count == 0) {
                value = default!;
                return false;
            }

            var keyBuffer = stackalloc byte[sizeof(TKey) * 2];
            Support.KeyToBuffer(&key, keyBuffer);
            ref TValue refValue = ref root.TryGetValue(null, keyBuffer);
            if (Unsafe.AsPointer(ref refValue) == null) {
                value = default!;
                return false;
            }
            value = refValue;
            return true;
        }

        /// <summary>
        /// 当Key不存在时返回空引用，否则返回对应的Value。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ref readonly TValue GetRefValue(TKey key) {
            if (Count == 0) return ref Unsafe.AsRef<TValue>(null);

            var keyBuffer = stackalloc byte[sizeof(TKey) * 2];
            Support.KeyToBuffer(&key, keyBuffer);
            return ref root.TryGetValue(null, keyBuffer);
        }

        /// <summary>
        /// 当Key不存在时返回空引用，否则返回对应的Value，并返回Value的代数。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ref readonly TValue GetRefValue(TKey key, out int generation) {
            if (Count == 0) {
                generation = Generation;
                return ref Unsafe.AsRef<TValue>(null);
            }

            var keyBuffer = stackalloc byte[sizeof(TKey) * 2];
            Support.KeyToBuffer(&key, keyBuffer);
            int gen;
            ref TValue value = ref root.TryGetValue(&gen, keyBuffer);
            if (Unsafe.IsNullRef(ref value)) {
                generation = Generation;
            } else {
                generation = gen;
            }
            return ref value;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        sealed class KeyEnumerator : ICollection<TKey> {
            readonly IDictionary<TKey, TValue> source;

            public KeyEnumerator(IDictionary<TKey, TValue> source) {
                this.source = source;
            }

            public int Count => source.Count;

            public bool IsReadOnly => true;

            public void Add(TKey item) {
                throw MerklePatriciaTreeSupport.NotSupportedException;
            }

            public void Clear() {
                throw MerklePatriciaTreeSupport.NotSupportedException;
            }

            public bool Contains(TKey item) {
                return source.ContainsKey(item);
            }

            public void CopyTo(TKey[] array, int arrayIndex) {
                if (arrayIndex < 0 || Count > array.Length - arrayIndex) throw new ArgumentOutOfRangeException();
                int i = 0;
                foreach (var key in this) {
                    array[arrayIndex + i++] = key;
                }
            }

            public IEnumerator<TKey> GetEnumerator() => source.Select(kv => kv.Key).GetEnumerator();

            public bool Remove(TKey item) {
                throw MerklePatriciaTreeSupport.NotSupportedException;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        sealed class ValueEnumerator : ICollection<TValue> {
            readonly IDictionary<TKey, TValue> source;

            public ValueEnumerator(IDictionary<TKey, TValue> source) {
                this.source = source;
            }

            public int Count => source.Count;

            public bool IsReadOnly => true;

            public void Add(TValue item) {
                throw MerklePatriciaTreeSupport.NotSupportedException;
            }

            public void Clear() {
                throw MerklePatriciaTreeSupport.NotSupportedException;
            }

            public bool Contains(TValue item) => source.Select(kv => kv.Value).Contains(item);

            public void CopyTo(TValue[] array, int arrayIndex) {
                if (arrayIndex < 0 || Count > array.Length - arrayIndex) throw new ArgumentOutOfRangeException();
                int i = 0;
                foreach (var value in this) {
                    array[arrayIndex + i++] = value;
                }
            }

            public IEnumerator<TValue> GetEnumerator() => source.Select(kv => kv.Value).GetEnumerator();

            public bool Remove(TValue item) {
                throw MerklePatriciaTreeSupport.NotSupportedException;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
