using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static OtherworldMod.Core.Util.Utils;
using static OtherworldMod.Common.ChangeNPC.Utilities.OtherworldNPCSets;

namespace OtherworldMod.Common.ChangeNPC
{
    //Splitting GlobalX into partial X_Info files, moving to Common.ChangeX
    public partial class OtherworldNPC : GlobalNPC
    {
        //Don't expect these to change
        public int[]? shootProj;
        public int[]? spawnNPC;
        public bool ignoreAIChanges;
        public bool ignoreWater;
        public bool ignoreTiles;

        //Can be changed through AI
        public bool allowContactDmg = true;

        //public bool npcTarget = true;
        public string? phase = "";

        private void SetProjectiles(NPC npc, int checkType, params int[] projs)
        {
            if (npc.type == checkType || npc.netID == checkType)
            {
                npc.GetGlobalNPC<OtherworldNPC>().shootProj = projs;
            }
        }
        private void SetSpawnedNPCs(NPC npc, int checkType, params int[] npcs)
        {
            if (npc.type == checkType || npc.netID == checkType)
            {
                npc.GetGlobalNPC<OtherworldNPC>().spawnNPC = npcs;
            }
        }
        private void SetIgnoreAI(NPC npc, int checkType, bool val)
        {
            if (npc.type == checkType || npc.netID == checkType)
            {
                npc.GetGlobalNPC<OtherworldNPC>().ignoreAIChanges = val;
            }
        }
        private void SetIgnoreWater(NPC npc, int checkType, bool val)
        {
            if (npc.type == checkType || npc.netID == checkType)
            {
                npc.GetGlobalNPC<OtherworldNPC>().ignoreWater = val;
            }
        }
        private void SetIgnoreTiles(NPC npc, int checkType, bool val)
        {
            if (npc.type == checkType || npc.netID == checkType)
            {
                npc.GetGlobalNPC<OtherworldNPC>().ignoreTiles = val;
            }
        }

        public void SetVanillaDefaults(NPC npc)
        {
            //Vanilla NPCs that Fire Projectiles
            SetProjectiles(npc, NPCID.SlimeSpiked, ProjectileID.SpikedSlimeSpike);
            SetProjectiles(npc, NPCID.SpikedIceSlime, ProjectileID.IceSpike);
            SetProjectiles(npc, NPCID.SpikedJungleSlime, ProjectileID.JungleSpike);
            SetProjectiles(npc, NPCID.QueenSlimeMinionBlue, ProjectileID.QueenSlimeMinionBlueSpike);
            SetProjectiles(npc, NPCID.QueenSlimeMinionPink, ProjectileID.QueenSlimeMinionPinkBall);
            SetProjectiles(npc, NPCID.Hornet, ProjectileID.HornetStinger);
            SetProjectiles(npc, NPCID.MossHornet, ProjectileID.HornetStinger);
            SetProjectiles(npc, NPCID.Harpy, ProjectileID.HarpyFeather);
            #region Unused
            //SetProjectiles(npc, NPCID.HornetHoney, ProjectileID.HornetStinger);
            //RegisterShotProjectile(NPCID.HornetLeafy, ProjectileID.HornetStinger);
            //RegisterShotProjectile(NPCID.HornetSpikey, ProjectileID.HornetStinger);
            //RegisterShotProjectile(NPCID.HornetStingy, ProjectileID.HornetStinger);
            //ShotProjectiles[NPCID.BigHornetFatty] = ProjectileID.HornetStinger;
            //ShotProjectiles[NPCID.BigHornetHoney] = ProjectileID.HornetStinger;
            //ShotProjectiles[NPCID.BigHornetLeafy] = ProjectileID.HornetStinger;
            //ShotProjectiles[NPCID.BigHornetSpikey] = ProjectileID.HornetStinger;
            //ShotProjectiles[NPCID.BigHornetStingy] = ProjectileID.HornetStinger;
            //ShotProjectiles[NPCID.BigMossHornet] = ProjectileID.HornetStinger;
            //ShotProjectiles[NPCID.GiantMossHornet] = ProjectileID.HornetStinger;
            //ShotProjectiles[NPCID.LittleHornetFatty] = ProjectileID.HornetStinger;
            //ShotProjectiles[NPCID.LittleHornetHoney] = ProjectileID.HornetStinger;
            //ShotProjectiles[NPCID.LittleHornetLeafy] = ProjectileID.HornetStinger;
            //ShotProjectiles[NPCID.LittleHornetSpikey] = ProjectileID.HornetStinger;
            //ShotProjectiles[NPCID.LittleHornetStingy] = ProjectileID.HornetStinger;
            //ShotProjectiles[NPCID.LittleMossHornet] = ProjectileID.HornetStinger;
            //RegisterShotProjectile(NPCID.MossHornet, ProjectileID.HornetStinger);
            //RegisterShotProjectile(NPCID.Harpy, ProjectileID.HarpyFeather);
            #endregion
            SetIgnoreWater(npc, NPCID.EnchantedSword, true);
            SetIgnoreWater(npc, NPCID.ChaosBall, true);
            SetIgnoreWater(npc, NPCID.ChaosBallTim, true);
            SetIgnoreWater(npc, NPCID.CrimsonAxe, true);
            SetIgnoreWater(npc, NPCID.CursedHammer, true);
            SetIgnoreWater(npc, NPCID.EyeofCthulhu, true);
            SetIgnoreWater(npc, NPCID.EaterofWorldsHead, true);
            SetIgnoreWater(npc, NPCID.EaterofWorldsBody, true);
            SetIgnoreWater(npc, NPCID.EaterofWorldsTail, true);
            SetIgnoreWater(npc, NPCID.VileSpitEaterOfWorlds, true);
            SetIgnoreWater(npc, NPCID.BrainofCthulhu, true);
            SetIgnoreWater(npc, NPCID.Creeper, true);
            SetIgnoreWater(npc, NPCID.CursedSkull, true);
            SetIgnoreWater(npc, NPCID.GiantCursedSkull, true);
            SetIgnoreWater(npc, NPCID.Skeleton, true);
            SetIgnoreWater(npc, NPCID.KingSlime, true);
            SetIgnoreWater(npc, NPCID.Ghost, true);
            SetIgnoreWater(npc, NPCID.Wraith, true);
            SetIgnoreWater(npc, NPCID.Reaper, true);
            SetIgnoreWater(npc, NPCID.WallofFlesh, true);
            SetIgnoreWater(npc, NPCID.WallofFleshEye, true);
            SetIgnoreWater(npc, NPCID.WaterSphere, true);
            SetIgnoreWater(npc, NPCID.Clinger, true);
            SetIgnoreWater(npc, NPCID.FloatyGross, true);
            SetIgnoreWater(npc, NPCID.GiantWormHead, true);
            SetIgnoreWater(npc, NPCID.GiantWormBody, true);
            SetIgnoreWater(npc, NPCID.GiantWormTail, true);
            SetIgnoreWater(npc, NPCID.DiggerHead, true);
            SetIgnoreWater(npc, NPCID.DiggerBody, true);
            SetIgnoreWater(npc, NPCID.DiggerTail, true);
            SetIgnoreWater(npc, NPCID.DevourerHead, true);
            SetIgnoreWater(npc, NPCID.DevourerBody, true);
            SetIgnoreWater(npc, NPCID.DevourerTail, true);
        }
    }
}
