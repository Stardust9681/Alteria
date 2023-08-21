using Alteria.Common.Structure;
using Alteria.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alteria.Common.Interface
{

    /// <summary>
    /// A thing that can be found and targetted with <see cref="TargetCollective.PullTarget(IRadar)"/>
    /// </summary>
    public interface ITargetable
    {
        /// <summary>
        /// Called to find target state.
        /// </summary>
        /// <param name="source">The searching source</param>
        /// <returns>Returns a signed int dictating the target state such that
        /// <list type="table">
        /// <item><term>-1</term>
        /// <description>Marked for removal; the target has expired and should no longer be available.</description></item>
        /// <item><term>0</term>
        /// <description>Active, but should not be available for radar location</description></item>
        /// <item><term>1</term>
        /// <description>Active and available for radar</description></item></list></returns>
        public int GetState();
        /// <summary>
        /// Get information about the target including position and aggro
        /// </summary>
        /// <param name="radar"></param>
        /// <returns></returns>
        public TargetInfo GetInfo(IRadar radar);
    }
}
