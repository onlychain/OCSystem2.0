using OnlyChain.Core;
using OnlyChain.Network;
using System;
using System.Net;
using System.Threading.Tasks;

namespace OnlyChain.Debug {
    class Program {
        static async Task Main(string[] args) {
            //Console.WriteLine(DateTime.UtcNow);
            //return;


            var (privKey, _, addr) = Config.Users[20]; // 85e23a0c02ed19be939aa9be4499322467cebd17
            var config = new SuperNodeConfig {
                PrivateKey = privKey,
            };
            Client client = new Client(addr, new IPEndPoint(IPAddress.Loopback, 31000), seeds: new[] { IPEndPoint.Parse("127.0.0.1:30000") }, superConfig: config, name: "client-31000");
            await client.Initialization();

            await Task.Delay(-1);
        }
    }
}
