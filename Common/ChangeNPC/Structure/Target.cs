using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using OtherworldMod.Core.Util;
using Mono.Cecil;

namespace OtherworldMod.Common.ChangeNPC.Structure
{
    /*
    public abstract class TargetEntity
    {
        //public static TargetEntity Create<T>(T instance) where T : Entity
        //{
        //    TargetEntity t = (TargetEntity)Activator.CreateInstance(MethodBase.GetCurrentMethod().DeclaringType);
        //    t.Root = instance.GetSource_FromThis();
        //    t.friendly = false;
        //    return t;
        //}
        public IEntitySource Root { get; protected set; }
        public virtual bool CanBeTargetted => true;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual Vector2 GetTargetPos()
        {
            return (Root as EntitySource_Parent).Entity.Center;
        }
        public virtual byte GetAggro()
        {
            return 0;
        }
        //Change this eventually to enum or int types?
        //Ex: Friendliness.PlayerHostile, Friendliness.PlayerFriendly
        //Potential to include teams such that...
        //Friendliness.PinkFriendly, Friendliness.NoTeamFriendly, or Friendliness.NoTeamHostile
        //To be considered.
        public bool friendly;
    }

    /// <summary>
    /// Exists for unknown types inheriting <see cref="Entity"/>, if a type exists for given <see cref="T"/>, use that instead.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Target<T> : TargetEntity where T : Entity
    {
        public Target(T instance)
        {
            Root = instance.GetSource_FromThis();
            friendly = false;
        }
        public override bool CanBeTargetted => (Root as EntitySource_Parent).Entity.active;
        public override Vector2 GetTargetPos()
        {
            float range = (340 - GetAggro()) * .75f;
            Vector2 offset = new Vector2(Main.rand.NextFloat(-range, range), Main.rand.NextFloat(-range, range));
            return (Root as EntitySource_Parent).Entity.Center + offset;
        }
        public override byte GetAggro() => 127;
    }
    */
    /*
    public interface ITarget
    {
        byte Aggro { get; }
        IEntitySource Source { get; }
        bool IsTargettable(TargetMode mode);
        Vector2 TargetPosition(Vector2 sourcePos);
    }
    */
    /*
    public struct Target
    {
        public Vector2 Position;
        public byte Aggro;
        public TargetMode Requirements;

        public Target()
        {
            Position = Vector2.Zero;
            Aggro = 0;
            Requirements = TargetMode.NoTarget;
        }
        public Target(Vector2 position, byte aggro = 0, byte reqs = (byte)TargetMode.NoTarget)
        {
            Position = position;
            Aggro = aggro;
            Requirements = (TargetMode)reqs;
        }
        public bool ValidateTarget(TargetMode mode)
        {
            //0b1111_1111
            //0b0000_0000
            //Result: false

            //0b0000_0000
            //0b1111_1111
            //Result: false

            //0b1111_1111
            //0b1111_1111
            //Result: true

            //0b0000_1111
            //0b0101_0101
            //Result: true

            //(Input & Req) != 0 maybe?

            return (Requirements & mode) != TargetMode.NoTarget;
        }
    }
    public static class TargetHolder
    {
        static IList<Target> current = new List<Target>();
        static readonly IList<Target> next = new List<Target>();
        public static void AddTarget(Vector2 targetPos, byte aggro = 0, byte matchReqs = (byte)TargetMode.NoTarget)
        {
            next.Add(new Target(targetPos, aggro, matchReqs));
        }
        public static void Flush()
        {
            current = next;
            next.Clear();
        }
        public static Vector2 PullTarget(Vector2 source, TargetMode mode, bool ignoreTiles = false)
        {
            bool noCollide = false;
            float distance = float.MaxValue;
            Vector2 targetPos = source;
            foreach (Target target in current.Where((Target x) => x.ValidateTarget(mode)))
            {
                if (ignoreTiles || !noCollide || Collision.CanHitLine(source, 1, 1, target.Position, 1, 1))
                {
                    noCollide = true;
                    float dist = Vector2.DistanceSquared(source, target.Position) * (target.Aggro / 85f);
                    if (dist < distance)
                    {
                        targetPos = target.Position;
                        distance = dist;
                    }
                }
            }
            return targetPos;
        }
    }*/
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
    public interface ITargetSource : IComparer<ITargetable>
    {
        public bool CanTarget(ITargetable target);
        public bool IgnoresMisdirection(ITargetable target);
    }
    public interface ITargetable
    {
        public bool CanBeTargetted(ITargetSource source);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public TargetInfo GetInfo(ITargetSource source);
    }

    public class EntityTargetSource : ITargetSource
    {
        public EntityTargetSource(IEntitySource src) { source = src; }
        IEntitySource source;
        public Entity Source => (source as EntitySource_Parent).Entity;
        public bool CanTarget(ITargetable target)
        {
            return true;
        }
        public bool IgnoresMisdirection(ITargetable target)
        {
            return false;
        }
        public int Compare(ITargetable a, ITargetable b)
        {
            Vector2 orig = Source.Center;
            return Vector2.DistanceSquared(orig, a.GetInfo(this).Position) < Vector2.DistanceSquared(orig, b.GetInfo(this).Position) ? -1 : 1;
        }
    }
    public class EntityTarget : ITargetable
    {
        public EntityTarget(IEntitySource src) { source = src; }
        IEntitySource source;
        public Entity Source => (source as EntitySource_Parent).Entity;
        public virtual bool CanBeTargetted(ITargetSource source)
        {
            return true;
        }
        public virtual TargetInfo GetInfo(ITargetSource source)
        {
            return new TargetInfo(0, Source.Center);
        }
    }

    public class PlayerTargetSource : ITargetSource, ISourceable<Player>
    {
        public PlayerTargetSource(Player player) => index = player.whoAmI;
        int index;
        public Player Source => Main.player[index];
        public virtual bool CanTarget(ITargetable target)
        {
            if (target is PlayerTarget player)
            {
                return player.Source.InOpposingTeam(Source);
            }
            else if (target is ProjectileTarget projectile)
            {
                Projectile p = projectile.Source;
                return p.hostile && !p.friendly && p.owner != Source.whoAmI;
            }
            return true;
        }
        public virtual bool IgnoresMisdirection(ITargetable target)
        {
            if (target is PlayerTarget)
            {
                return true;
            }
            return true;
        }
        public int Compare(ITargetable a, ITargetable b)
        {
            Vector2 orig = Source.Center;
            if (a is ProjectileTarget)
            {
                if (b is not ProjectileTarget)
                    return 1;
            }
            else if (b is ProjectileTarget)
                return -1;
            return Vector2.DistanceSquared(orig, a.GetInfo(this).Position) < Vector2.DistanceSquared(orig, b.GetInfo(this).Position) ? -1 : 1;
        }
    }
    public class PlayerTarget : ITargetable, ISourceable<Player>
    {
        public PlayerTarget(Player player) => index = player.whoAmI;
        int index;
        public Player Source => Main.player[index];
        public virtual bool CanBeTargetted(ITargetSource source)
        {
            return true;
        }
        public virtual TargetInfo GetInfo(ITargetSource source)
        {
            byte aggro = (byte)((Source.aggro + 4300) * 255 / 6500);
            return new TargetInfo(aggro, Source.Center);
        }
    }

    public class ProjectileTargetSource : ITargetSource, ISourceable<Projectile>
    {
        public ProjectileTargetSource(Projectile projectile) => index = projectile.whoAmI;
        int index;
        public Projectile Source => Main.projectile[index];
        public virtual bool CanTarget(ITargetable target)
        {
            return true;
        }
        public virtual bool IgnoresMisdirection(ITargetable target)
        {
            return true;
        }
        public int Compare(ITargetable a, ITargetable b)
        {
            if (a is NPCTarget npcA)
            {
                if (b is NPCTarget npcB)
                {
                    if (npcA.Source.CanBeChasedBy() && !npcB.Source.CanBeChasedBy())
                        return -1;
                    else if(!npcA.Source.CanBeChasedBy() && npcB.Source.CanBeChasedBy())
                        return 1;
                }
                if (!npcA.Source.CanBeChasedBy())
                    return 1;
            }
            else if(b is NPCTarget npcB && !npcB.Source.CanBeChasedBy())
            {
                return -1;
            }
            Vector2 orig = Source.Center;
            TargetInfo infoA = a.GetInfo(this);
            TargetInfo infoB = b.GetInfo(this);
            if (Source.tileCollide)
            {
                if (Collision.CanHitLine(orig, 1, 1, infoA.Position, 1, 1))
                {
                    if (Collision.CanHitLine(orig, 1, 1, infoB.Position, 1, 1))
                    {
                        return Vector2.DistanceSquared(orig, infoA.Position) * (85 - (infoA.aggro / 3)) < Vector2.DistanceSquared(orig, infoB.Position) * (85 - (infoA.aggro / 3)) ? -1 : 1;
                    }
                    else
                    {
                        return -1;
                    }
                }
                else if (Collision.CanHitLine(orig, 1, 1, infoB.Position, 1, 1))
                {
                    return 1;
                }
            }
            return Vector2.DistanceSquared(orig, infoA.Position) * (85 - (infoA.aggro / 3)) < Vector2.DistanceSquared(orig, infoB.Position) * (85 - (infoA.aggro / 3)) ? -1 : 1;
        }
    }
    public class ProjectileTarget : ITargetable, ISourceable<Projectile>
    {
        public ProjectileTarget(Projectile projectile) => index = projectile.whoAmI;
        int index;
        public Projectile Source => Main.projectile[index];
        public virtual bool CanBeTargetted(ITargetSource source)
        {
            return GetInfo(source).aggro > 0;
        }
        public virtual TargetInfo GetInfo(ITargetSource source)
        {
            return new TargetInfo(0, Source.Center);
        }
    }

    public class NPCTargetSource : ITargetSource, ISourceable<NPC>
    {
        public NPCTargetSource(NPC npc) => index = npc.whoAmI;
        int index;
        public NPC Source => Main.npc[index];
        public virtual bool CanTarget(ITargetable target)
        {
            if (target is NPCTarget npc)
            {
                return npc.Source.friendly != Source.friendly;
            }
            if (target is PlayerTarget)
            {
                return !Source.friendly;
            }
            return true;
        }
        public virtual bool IgnoresMisdirection(ITargetable target)
        {
            return false;
        }
        public int Compare(ITargetable a, ITargetable b)
        {
            Vector2 orig = Source.Center;
            if (a is NPCTarget npcA)
            {
                if (b is NPCTarget npcB)
                {
                    if (npcA.Source.CanBeChasedBy() && !npcB.Source.CanBeChasedBy())
                        return -1;
                    else if (!npcA.Source.CanBeChasedBy() && npcB.Source.CanBeChasedBy())
                        return 1;
                }
                if (!npcA.Source.CanBeChasedBy())
                    return 1;
            }
            else if (b is NPCTarget npcB && !npcB.Source.CanBeChasedBy())
            {
                return -1;
            }
            TargetInfo infoA = a.GetInfo(this);
            TargetInfo infoB = b.GetInfo(this);
            if (!Source.noTileCollide)
            {
                if (Collision.CanHitLine(orig, 1, 1, infoA.Position, 1, 1))
                {
                    if (Collision.CanHitLine(orig, 1, 1, infoB.Position, 1, 1))
                    {
                        return Vector2.DistanceSquared(orig, infoA.Position) * (85 - (infoA.aggro / 3)) < Vector2.DistanceSquared(orig, infoB.Position) * (85 - (infoA.aggro / 3)) ? -1 : 1;
                    }
                    else
                    {
                        return -1;
                    }
                }
                else if (Collision.CanHitLine(orig, 1, 1, infoB.Position, 1, 1))
                {
                    return 1;
                }
            }
            return Vector2.DistanceSquared(orig, infoA.Position) * (85 - (infoA.aggro / 3)) < Vector2.DistanceSquared(orig, infoB.Position) * (85 - (infoA.aggro / 3)) ? -1 : 1;
        }
    }
    public class NPCTarget : ITargetable, ISourceable<NPC>
    {
        public NPCTarget(NPC npc) => index = npc.whoAmI;
        int index;
        public NPC Source => Main.npc[index];
        public virtual bool CanBeTargetted(ITargetSource source)
        {
            if (source is NPCTargetSource npc)
            {
                return npc.Source.friendly != Source.friendly;
            }
            return true;
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
                        pos = (orig - pos) + orig;
                    }
                }
            }
            return new TargetInfo(aggro, pos);
        }
    }

    public class RadarTargetSource : ITargetSource
    {
        public RadarTargetSource(Vector2 pos) => orig = pos;
        Vector2 orig;
        public virtual bool CanTarget(ITargetable target) => true;
        public virtual bool IgnoresMisdirection(ITargetable target) => true;
        public int Compare(ITargetable a, ITargetable b)
        {
            return Vector2.DistanceSquared(orig, a.GetInfo(this).Position) < Vector2.DistanceSquared(orig, b.GetInfo(this).Position) ? -1 : 1;
        }
    }
    public class EmptyTarget : ITargetable
    {
        public EmptyTarget(Vector2 pos, byte aggro)
        {
            info = new TargetInfo(aggro, pos);
        }
        TargetInfo info;
        public bool CanBeTargetted(ITargetSource source) => true;
        public TargetInfo GetInfo(ITargetSource source) => info;
    }

    public static class TargetCollective
    {
        private static ITargetable[] targets;
        private static int capacity;
        public static int AddCapacity(int addedSpace = 0)
        {
            int prevCap = capacity;
            capacity += addedSpace;
            return prevCap;
        }
        public static void Load()
        {
            targets = new ITargetable[capacity];
        }
        public static void Update(int index, ITargetable target)
        {
            if (index < 0 || index > capacity)
                return;
            targets[index] = target;
        }

        public static ITargetable PullTarget(ITargetSource source)
        {
            ITargetable returnVal = targets[0];
            foreach (ITargetable target in targets)
            {
                if (!(target.CanBeTargetted(source) && source.CanTarget(target)))
                    continue;
                if (source.Compare(target, returnVal) < 0)
                {
                    returnVal = target;
                }
            }
            return returnVal;
        }
    }
}
