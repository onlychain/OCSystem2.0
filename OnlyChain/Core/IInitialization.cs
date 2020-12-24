using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyChain.Core {
    public interface IInitialization {
        Task InitializationTask { get; }
    }
}
