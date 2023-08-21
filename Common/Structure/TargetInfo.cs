using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Alteria.Common.Structure
{
    public struct TargetInfo
    {
        public TargetInfo()
        {
            Position = Vector2.Zero;
            aggro = 0;
            faction = Faction.UnivNoFac;
        }
        public TargetInfo(byte aggro, Vector2 position, Faction fac)
        {
            this.aggro = aggro;
            Position = position;
            faction = fac;
            Initiated = true;
        }

        public void SetVoid(byte aggro = 0, Vector2 position = default, Faction fac = Faction.UnivNoFac)
        {
            this.aggro = aggro;
            Position = position;
            faction = fac;
            Initiated = true;
        }
        public TargetInfo Set(byte aggro = 0, Vector2 position = default, Faction fac = Faction.UnivNoFac)
        {
            this.SetVoid(aggro, Position, fac);
            return this;
        }

        public Vector2 Position { get; set; }
        public byte aggro;
        public Faction faction;
        public string? context = null;

        public bool Initiated
        {
            get;
            private set;
        } = false;

        internal static void NetSend(TargetInfo info, System.IO.BinaryWriter writer)
        {
            writer.Write(info.aggro);
            writer.Write(info.Position.X);
            writer.Write(info.Position.Y);
            writer.Write(info.context);
        }
        internal static TargetInfo NetRec(System.IO.BinaryReader reader)
        {
            TargetInfo info = new TargetInfo();
            info.aggro = reader.ReadByte();
            info.Position = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            info.context = reader.ReadString();
            return info;
        }
    }
}
