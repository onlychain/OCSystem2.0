#nullable enable

using OnlyChain.Network.Objects;

namespace OnlyChain.Network {
    public sealed class GetValueResult {
        public readonly Node[] Nodes;
        public readonly ValueResult Result;

        public GetValueResult(Node[] nodes, Node? resultNode, BObject? result) {
            Nodes = nodes;
            Result = new ValueResult(resultNode, result);
        }
    }
}
