#nullable enable

using OnlyChain.Network.Objects;

namespace OnlyChain.Network {
    public readonly struct ValueResult {
        public readonly Node? Node;
        public readonly BObject? Value;

        public bool HasValue => (Node, Value) is (not null, not null);

        public ValueResult(Node? node, BObject? value) {
            Value = value;
            Node = node;
        }
    }
}
