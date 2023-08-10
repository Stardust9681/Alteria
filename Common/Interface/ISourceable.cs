using OtherworldMod.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtherworldMod.Common.Interface
{
    public interface ISourceable<T>
    {
        public T Source { get; }
    }
}
