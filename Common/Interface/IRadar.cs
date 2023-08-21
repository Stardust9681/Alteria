using Alteria.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alteria.Common.Structure;

namespace Alteria.Common.Interface
{
    /// <summary>
    /// The source of a <see cref="TargetCollective.PullTarget(IRadar)"/> call, used to find a valid target (<see cref="IRadar"/>)
    /// </summary>
    public interface IRadar
    {
        /// <summary>
        /// Used to weigh a target's value/worth. Larger (positive) values are deemed as more worthy targets.
        /// </summary>
        /// <param name="target"></param>
        /// <returns>A signed float signalling <paramref name="target"/>'s weight for targeting as determined by table below.
        /// <list type="table">
        /// <item><term>Positive</term>
        /// <description>High Priority Target</description></item>
        /// <item><term>Zero</term>
        /// <description>Low Priority Target</description></item>
        /// <item><term>Negative</term>
        /// <description>Cannot be targeted</description></item>
        /// </list></returns>
        public float GetWeight(ITargetable target);

        public RadarInfo Info { get; }
    }
}
