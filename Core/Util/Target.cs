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
using Alteria.Common.ChangeNPC;
using Microsoft.CodeAnalysis;
using static Alteria.Core.Util.Utils;
using Alteria.Common.Interface;
using Alteria.Common.Structure;

namespace Alteria.Core.Util
{
    /*
    //Arbitrary entity target types
    //For when someone adds an unknown entity type, and doesn't properly use API to create their own type.
    public class EntityTargetSource : IRadar
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
        public virtual TargetInfo GetInfo(IRadar source)
        {
            return new TargetInfo(0, Source.Center, Faction.UnivNoFac);
        }
    }

    //Player target types
    //Use this for effects that happen from the player but aren't directly controlled by the player (ex: retaliation/parry strikes)
    public class PlayerTargetSource : IRadar, ISourceable<Player>
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
        public virtual TargetInfo GetInfo(IRadar source)
        {
            byte aggro = (byte)((Source.aggro + 4300) * 255 / 6500);
            Faction infoFac = Source.team switch
            {
                1 => Faction.TeamRed,
                2 => Faction.TeamGreen,
                3 => Faction.TeamBlue,
                4 => Faction.TeamYellow,
                5 => Faction.TeamPink,
                _ => Faction.TeamWhite,
            };
            return new TargetInfo(aggro, Source.Center, infoFac);
        }
    }

    //Projectile target types
    //Use these for projectile homing (minions, chlorophyte/nanite bullets, etc), or redirect (Stardust Guardian)
    public class ProjectileTargetSource : IRadar, ISourceable<Projectile>
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
        public virtual TargetInfo GetInfo(IRadar source)
        {
            if(Source.hostile)
                return new TargetInfo(0, Source.Center, );
        }
    }

    //NPC target types
    //Primary mod objective uses these.
    public class NPCTargetSource : IRadar, ISourceable<NPC>
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
        public virtual TargetInfo GetInfo(IRadar source)
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
    public class RadarTargetSource : IRadar
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
            info = new TargetInfo(aggro, pos, Faction.UnivHostile);
        }
        TargetInfo info;
        public int GetState() => -1;
        public TargetInfo GetInfo(IRadar source) => info;
    }*/

    //For modded NPCs that DON'T use this system
    public class NPCTarget : ITargetable, ISourceable<NPC>
    {
        protected int _index;
        public NPC Source => Main.npc[_index];
        public NPCTarget(int index = 0)
        {
            _index = index;
        }

        public virtual TargetInfo GetInfo(IRadar radar)
        {
            return new TargetInfo(Source.GetGlobalNPC<AlteriaNPC>().aggro, Source.Center, Faction.UnivNoFac | Faction.UnivHostile);
        }
        public virtual int GetState()
        {
            return Source.active ? 1 : -1;
        }
    }
    //For NPCs that DO use this system
    public class NPCTarget<T> : NPCTarget where T : Common.ChangeNPC.AI.AIStyleType
    {
        private Common.ChangeNPC.AI.AIStyleType _style;
        public NPCTarget(int index = 0) : base(index)
        {
            _style = ModContent.GetInstance<T>();
        }

        private TargetInfo _cache;
        public override TargetInfo GetInfo(IRadar radar)
        {
            if (!_cache.Initiated)
                return (_cache = base.GetInfo(radar));
            _style.UpdateInfo(ref _cache, _index, radar);
            return _cache;
        }
        public override int GetState()
        {
            return Source.active ? 1 : -1;
        }
    }

    //Projectiles that wish to be targeted should inherit this class
    public class ProjectileTarget : ITargetable, ISourceable<Projectile>
    {
        protected int _index;
        public Projectile Source => Main.projectile[_index];
        public ProjectileTarget(int index = 0)
        {
            _index = 0;
        }

        public TargetInfo GetInfo(IRadar radar)
        {
            return new TargetInfo(100, Source.position, Faction.UnivNoFac);
        }
        public int GetState() => 0;
    }

    //Player target
    public class PlayerTarget : ITargetable, ISourceable<Player>
    {
        protected int _index;
        public Player Source => Main.player[_index];
        public PlayerTarget(int index = 0)
        {
            _index = index;
        }

        public TargetInfo GetInfo(IRadar radar)
        {
            byte aggro = (byte)MathHelper.Min(((Source.aggro + 4300) * 255 / 6500), 255);
            return new TargetInfo(aggro, Source.position, Faction.UnivFriendly);
        }
        public int GetState() => 0;
    }

    public class Target : ITargetable
    {
        private Vector2 _position;
        private byte _aggro;
        public Target(Vector2 pos, byte aggro)
        {
            _position = pos;
            _aggro = aggro;
        }
        public TargetInfo GetInfo(IRadar radar) => new TargetInfo(_aggro, _position, Faction.UnivHostile);
        public int GetState() => 0;
    }



    public class NPCRadar : IRadar, ISourceable<NPC>
    {
        protected int _index;
        public NPC Source => Main.npc[_index];
        public NPCRadar(int index = 0)
        {
            _index = index;
            _info = new RadarInfo(Source.position, false, 3f, true, true);
        }
        public virtual float GetWeight(ITargetable target)
        {
            return (Vector2.DistanceSquared(Info.Position, target.GetInfo(this).Position) * Info.AggroFactor).SafeInvert();
        }
        protected RadarInfo _info;
        public virtual RadarInfo Info
        {
            get
            {
                _info.Position = Source.position;
                //_info.Faction = Source.GetGlobalNPC<OtherworldNPC>()
                return _info;
            }
        }
    }

    public class ProjectileRadar : IRadar, ISourceable<Projectile>
    {
        protected int _index;
        public Projectile Source => Main.projectile[_index];
        public ProjectileRadar(int index = 0)
        {
            _index = index;
            _info = new RadarInfo(Source.position, true, 3, false, true);
        }
        public float GetWeight(ITargetable target)
        {
            return (Vector2.DistanceSquared(Info.Position, target.GetInfo(this).Position) * Info.AggroFactor).SafeInvert();
        }
        private RadarInfo _info;
        public RadarInfo Info
        {
            get
            {
                _info.Position = Source.position;
                return _info;
            }
        }
    }

    public class PlayerRadar : IRadar, ISourceable<Player>
    {
        protected int _index;
        public Player Source => Main.player[_index];
        public PlayerRadar(int index = 0)
        {
            _index = index;
            _info = new RadarInfo(Source.position, true, 3, false, true);
        }
        public float GetWeight(ITargetable target)
        {
            return (Vector2.DistanceSquared(Info.Position, target.GetInfo(this).Position) * Info.AggroFactor).SafeInvert();
        }
        private RadarInfo _info;
        public RadarInfo Info
        {
            get
            {
                _info.Position = Source.position;
                return _info;
            }
        }
    }

    public class Radar : IRadar
    {
        public Radar(Vector2 pos)
        {
            _info = new RadarInfo(pos, true, 1, true, true);
        }
        public float GetWeight(ITargetable target)
        {
            return (Vector2.DistanceSquared(Info.Position, target.GetInfo(this).Position) * Info.AggroFactor).SafeInvert();
        }
        private RadarInfo _info;
        public RadarInfo Info
        {
            get
            {
                return _info;
            }
        }
    }

    public static class TargetCollective
    {
        private static RigidList<ITargetable> targets = new RigidList<ITargetable>(1024);
        public static void AddTarget(ITargetable target)
        {
            targets.Add(target);
        }
        public static void RemoveTarget(ITargetable target)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (!targets.TryGet(i, out ITargetable comp))
                    continue;
                if (target.Equals(comp))
                {
                    targets.RemoveAt(i);
                    break;
                }
            }
        }

        public static void Update()
        {
            for(int i = 0; i < targets.Count; i++)
            {
                if (targets.TryGet(i, out ITargetable target))
                {
                    int state = target.GetState();
                    if (state == -1)
                    {
                        targets.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Finds the optimal target for a given <see cref="IRadar"/> radar.
        /// </summary>
        /// <param name="source">Searching object</param>
        /// <param name="info"></param>
        /// <param name="cutoff">Cutoff point for maximum target weight for early return</param>
        /// <returns>Index of found target</returns>
        public static int PullTarget(IRadar source, out TargetInfo info, float cutoff = 2048)
        {
            return targets.IndexOf(PullTargetDirect(source, out info, cutoff));
        }

        /// <summary>
        /// Get a target from private list
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool TryGetTarget(int index, out ITargetable target)
        {
            return targets.TryGet(index, out target);
        }

        /// <summary>
        /// Finds the optimal target for a given <see cref="IRadar"/> radar.
        /// </summary>
        /// <param name="radar">Searching object</param>
        /// <returns>Found target</returns>
        public static ITargetable PullTargetDirect(IRadar radar, out TargetInfo info, float cutoff = 2048)
        {
            //If no target at index 0 (safer than assuming one will exist at 0)
            if (targets.Count == 0)
            {
                ITargetable def = default(ITargetable);
                info = new TargetInfo(0, Vector2.Zero, Faction.UnivNoFac);
                return def;
            }

            ITargetable returnVal = default(ITargetable);
            cutoff.SafeInvert();
            float prevWeight = -1;

            for(int i = 0; i < targets.Count; i++)
            {
                if (targets.TryGet(i, out ITargetable target))
                {
                    float targetWeight = radar.GetWeight(target);
                    if (targetWeight < 0)// || target.GetState() != 1)
                        continue;
                    if (targetWeight > prevWeight)
                    {
                        returnVal = target;
                        prevWeight = targetWeight;
                        if (targetWeight > cutoff)
                            break;
                    }
                }
            }
            info = returnVal?.GetInfo(radar)??new TargetInfo();
            return returnVal;
        }

        public static int CountTargetsInRange(Vector2 position, float range = 32f, Predicate<ITargetable>? pred = null)
        {
            IRadar omniScanner = new Radar(position);
            int count = 0;
            for (int i = 0; i < targets.Count; i++)
            {
                if (!targets.TryGet(i, out ITargetable target))
                    continue;
                if (target.GetInfo(omniScanner).Position.DistanceSQ(position) > MathF.Pow(range, 2))
                    continue;
                if (pred?.Invoke(target) ?? true)
                    count++;
            }
            return count;
        }
    }
}
