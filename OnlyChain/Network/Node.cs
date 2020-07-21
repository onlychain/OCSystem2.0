using OnlyChain.Core;
using System;
using System.Net;

namespace OnlyChain.Network {
    public class Node : IEquatable<Node> {
        public readonly Address Address;
        public readonly IPEndPoint IPEndPoint;
        /// <summary>
        /// 上一次ping成功的时间点
        /// </summary>
        public DateTime RefreshTime;

        public Node(in Address address, IPEndPoint ipEndPoint, TimeSpan surviveTime) {
            Address = address;
            IPEndPoint = ipEndPoint;
            RefreshTime = DateTime.Now - surviveTime;
        }

        public Node(in Address address, IPEndPoint ipEndPoint) {
            Address = address;
            IPEndPoint = ipEndPoint;
            RefreshTime = DateTime.Now;
        }

        public bool Equals(Node other)
            => Address == other.Address;

        public override bool Equals(object obj)
            => obj is Address other && Equals(other);

        public override int GetHashCode() => Address.GetHashCode();

        public override string ToString() => $"{Address}, {IPEndPoint}";
    }
}
