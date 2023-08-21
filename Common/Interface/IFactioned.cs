using Alteria.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alteria.Common.Interface
{
    public interface IFactioned
    {
        Common.Structure.Faction Faction { get; }
    }
}
