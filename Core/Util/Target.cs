using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Mono.Cecil;
using OtherworldMod.Common.ChangeNPC;
using Microsoft.CodeAnalysis;
using static OtherworldMod.Core.Util.Utils;

namespace OtherworldMod.Core.Util
{
    public struct TargetInfo
    {
        public TargetInfo()
        {
            Position = Vector2.Zero;
            aggro = 0;
        }
        public TargetInfo(byte aggro, Vector2 position)
        {
            this.aggro = aggro;
            Position = position;
        }
        public Vector2 Position { get; private set; }
        public byte aggro;

        internal static void NetSend(TargetInfo info, System.IO.BinaryWriter writer)
        {
            writer.Write(info.aggro);
            writer.Write(info.Position.X);
            writer.Write(info.Position.Y);
        }
        internal static TargetInfo NetRec(System.IO.BinaryReader reader)
        {
            TargetInfo info = new TargetInfo();
            info.aggro = reader.ReadByte();
            info.Position = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            return info;
        }
    }

    public interface ISourceable<T>
    {
        public T Source { get; }
    }
    /// <summary>
    /// The source of a <see cref="TargetCollective.PullTarget(ITargetSource)"/> call, used to find a valid target (<see cref="ITargetSource"/>)
    /// </summary>
    public interface ITargetSource
    {
        /// <summary>
        /// Whether or not this source will ignore misdirect effects (confusion, invisibility, etc)
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IgnoresMisdirection(ITargetable target);
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
    }
    /// <summary>
    /// A thing that can be found and targetted with <see cref="TargetCollective.PullTarget(ITargetSource)"/>
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
        /// <param name="source"></param>
        /// <returns></returns>
        public TargetInfo GetInfo(ITargetSource source);
    }

    //Arbitrary entity target types
    //For when someone adds an unknown entity type, and doesn't properly use API to create their own type.
    public class EntityTargetSource : ITargetSource
    {
        public EntityTargetSource(IEntitySource src) { source = src; }
        IEntitySource source;
        public Entity Source => (source as EntitySource_Parent).Entity;
        public bool IgnoresMisdirection(ITargetable target)
        {
            return false;
        }
        public float GetWeight(ITargetable target)
        {
            return 0;
        }
    }
    public class EntityTarget : ITargetable
    {
        public EntityTarget(IEntitySource src) { source = src; }
        IEntitySource source;
        public Entity Source => (source as EntitySource_Parent).Entity;
        public virtual int GetState()
        {
            if (Source.active)
                return 1;
            return -1;
        }
        public virtual TargetInfo GetInfo(ITargetSource source)
        {
            return new TargetInfo(0, Source.Center);
        }
    }

    //Player target types
    //Use this for effects that happen from the player but aren't directly controlled by the player (ex: retaliation/parry strikes)
    public class PlayerTargetSource : ITargetSource, ISourceable<Player>
    {
        public PlayerTargetSource(Player player) => index = player.whoAmI;
        int index;
        public Player Source => Main.player[index];
        public virtual bool IgnoresMisdirection(ITargetable target)
        {
            if (target is PlayerTarget)
            {
                return true;
            }
            return true;
        }
        public float GetWeight(ITargetable target)
        {
            if (target is PlayerTarget player)
            {
                if (!player.Source.InOpposingTeam(Source))
                    return 0;
            }
            else if (target is ProjectileTarget projectile)
            {
                Projectile p = projectile.Source;
                if ((!p.hostile || p.friendly) && p.owner != Source.whoAmI)
                    return 0;
            }
            return Vector2.DistanceSquared(Source.Center, target.GetInfo(this).Position).SafeInvert();
        }
    }
    public class PlayerTarget : ITargetable, ISourceable<Player>
    {
        internal PlayerTarget(int playerIndex = 0) => index = playerIndex;
        public PlayerTarget(Player player) => index = player.whoAmI;
        int index;
        public Player Source => Main.player[index];
        public virtual int GetState()
        {
            Player p = Source;
            if (!p.active)
                return -1;
            if (p.dead)
                return 0;
            return 1;
        }
        public virtual TargetInfo GetInfo(ITargetSource source)
        {
            byte aggro = (byte)((Source.aggro + 4300) * 255 / 6500);
            return new TargetInfo(aggro, Source.Center);
        }
    }

    //Projectile target types
    //Use these for projectile homing (minions, chlorophyte/nanite bullets, etc), or redirect (Stardust Guardian)
    public class ProjectileTargetSource : ITargetSource, ISourceable<Projectile>
    {
        public ProjectileTargetSource(Projectile projectile) => index = projectile.whoAmI;
        int index;
        public Projectile Source => Main.projectile[index];
        public virtual bool IgnoresMisdirection(ITargetable target)
        {
            return true;
        }
        public float GetWeight(ITargetable target)
        {
            if (target is NPCTarget npc)
            {
                if (!npc.Source.CanBeChasedBy(Source))
                    return -1;
            }
            Vector2 orig = Source.Center;
            TargetInfo info = target.GetInfo(this);
            if (Source.tileCollide)
            {
                if (!Collision.CanHitLine(orig, 1, 1, info.Position, 1, 1))
                {
                    return 0;
                }
            }
            float mult = MathHelper.Lerp(.5f, 1.5f, info.aggro / 255f);
            return (Vector2.DistanceSquared(orig, info.Position) * mult).SafeInvert();
        }
    }
    public class ProjectileTarget : ITargetable, ISourceable<Projectile>
    {
        internal ProjectileTarget(int projIndex) => index = projIndex;
        public ProjectileTarget(Projectile projectile) => index = projectile.whoAmI;
        int index;
        public Projectile Source => Main.projectile[index];
        public virtual int GetState()
        {
            if (!Source.active)
                return -1;
            return 1;
        }
        public virtual TargetInfo GetInfo(ITargetSource source)
        {
            return new TargetInfo(0, Source.Center);
        }
    }

    //NPC target types
    //Primary mod objective uses these.
    public class NPCTargetSource : ITargetSource, ISourceable<NPC>
    {
        public NPCTargetSource(NPC npc) => index = npc.whoAmI;
        int index;
        public NPC Source => Main.npc[index];
        public virtual bool IgnoresMisdirection(ITargetable target)
        {
            return false;
        }
        public float GetWeight(ITargetable target)
        {
            if (target is NPCTarget npc)
            {
                NPC otherSource = npc.Source;
                if (otherSource.dontTakeDamage)
                    return -1;
                if (!Source.friendly && otherSource.dontTakeDamageFromHostiles)
                    return -1;
                if (otherSource.friendly == Source.friendly)
                    return 0;
            }
            else if (target is PlayerTarget)
            {
                if (Source.friendly)
                    return 0;
            }
            Vector2 orig = Source.Center;
            TargetInfo info = target.GetInfo(this);
            if (!Source.noTileCollide)
            {
                if (!Collision.CanHitLine(orig, 1, 1, info.Position, 1, 1))
                {
                    return 0;
                }
            }
            float mult = MathHelper.Lerp(.5f, 1.5f, info.aggro / 255f);
            return (Vector2.DistanceSquared(orig, info.Position) * mult).SafeInvert();
        }
    }
    public class NPCTarget : ITargetable, ISourceable<NPC>
    {
        internal NPCTarget(int npcIndex) => index = npcIndex;
        public NPCTarget(NPC npc) => index = npc.whoAmI;
        int index;
        public NPC Source => Main.npc[index];
        public virtual int GetState()
        {
            if (!Source.active)
                return -1;
            return 1;
        }
        public virtual TargetInfo GetInfo(ITargetSource source)
        {
            NPC npc = Source;
            byte aggro = npc.GetGlobalNPC<OtherworldNPC>().aggro;
            Vector2 pos = npc.Center;
            if (!source.IgnoresMisdirection(this))
            {
                if (npc.HasBuff(BuffID.Invisibility))
                    pos += new Vector2(Main.rand.NextFloat(aggro - 255, 255 - aggro), Main.rand.NextFloat(aggro - 255, 255 - aggro));
                if (source is NPCTargetSource npcSource)
                {
                    NPC src = npcSource.Source;
                    Vector2 orig = src.Center;
                    if (src.HasBuff(BuffID.Blackout) || src.HasBuff(BuffID.Darkness) || src.HasBuff(BuffID.Obstructed))
                    {
                        pos = Vector2.Lerp(orig, pos, .34f);
                    }
                    if (src.confused)
                    {
                        pos = orig - pos + orig;
                    }
                }
            }
            return new TargetInfo(aggro, pos);
        }
    }

    //Misc target types
    //For adding or using *completely* arbitrary radars/targets...
    //Radar is omniscient, will ignore misdirects, no special exceptions, and ignores tiles, liquid, etc.
    public class RadarTargetSource : ITargetSource
    {
        public RadarTargetSource(Vector2 pos) => orig = pos;
        Vector2 orig;
        public virtual bool IgnoresMisdirection(ITargetable target) => true;
        public float GetWeight(ITargetable target)
        {
            return Vector2.DistanceSquared(orig, target.GetInfo(this).Position).SafeInvert();
        }
    }
    public class EmptyTarget : ITargetable
    {
        public EmptyTarget(Vector2 pos, byte aggro)
        {
            info = new TargetInfo(aggro, pos);
        }
        TargetInfo info;
        public int GetState() => -1;
        public TargetInfo GetInfo(ITargetSource source) => info;
    }

    public static class TargetCollective
    {
        private static List<ITargetable> targets = new List<ITargetable>(2048);
        public static int AddTarget(ITargetable target)
        {
            targets.Add(target);
            return targets.Count;
        }
        public static void Update()
        {
            for (int i = 0; i < targets.Count; i++)
            {
                ITargetable target = targets[i];
                int state = target.GetState();
                if (state == -1)
                {
                    targets.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Finds the optimal target for a given <see cref="ITargetSource"/> radar.
        /// </summary>
        /// <param name="source">Searching object</param>
        /// <returns>Index of found target</returns>
        public static int PullTarget(ITargetSource source, out TargetInfo info, float cutoff = 2048)
        {
            cutoff.SafeInvert();
            int retIndex = 0;
            float prevWeight = -1;
            for(int i = 0; i < targets.Count; i++)
            {
                ITargetable target = targets[i];
                float targetWeight = source.GetWeight(target);
                if (targetWeight < 0 || target.GetState() != 1)
                {
                    continue;
                }
                if (targetWeight > prevWeight)
                {
                    prevWeight = targetWeight;
                    retIndex = i;
                    if (targetWeight > cutoff)
                        break;
                }
            }
            info = targets[retIndex].GetInfo(source);
            return retIndex;
        }
        /// <summary>
        /// Get a target from private array
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static ITargetable GetTargetFromIndex(int index) => targets[index];
        /// <summary>
        /// Finds the optimal target for a given <see cref="ITargetSource"/> radar.
        /// </summary>
        /// <param name="source">Searching object</param>
        /// <returns>Found target</returns>
        public static ITargetable PullTargetDirect(ITargetSource source, out TargetInfo info, float cutoff = 2048)
        {
            cutoff.SafeInvert();
            ITargetable returnVal = targets[0];
            float prevWeight = -1;
            foreach (ITargetable target in targets)
            {
                float targetWeight = source.GetWeight(target);
                if (targetWeight < 0 || target.GetState() != 1)
                    continue;
                if (targetWeight > prevWeight)
                {
                    returnVal = target;
                    prevWeight = targetWeight;
                    if (targetWeight > cutoff)
                        break;
                }
            }
            info = returnVal.GetInfo(source);
            return returnVal;
        }
    }
}
