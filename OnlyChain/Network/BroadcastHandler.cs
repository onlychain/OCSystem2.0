using System.Threading.Tasks;

namespace OnlyChain.Network {
    public delegate ValueTask BroadcastHandler(IClient client, BroadcastEventArgs e);
}
