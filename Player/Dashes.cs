using CalamityMod.Enums;
using Origins.Core;
using Origins.Items.Accessories;
using Origins.Items.Armor.Riptide;
using Origins.Items.Other.Consumables;
using Origins.Items.Other.Dyes;
using Origins.Reflection;
using PegasusLib;
using PegasusLib.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins {
	public record class Dash_Action(Player Player, int DashDirection, int DashDirectionY) : SyncedAction {
		public Dash_Action() : this(default, default, default) { }
		public override SyncedAction NetReceive(BinaryReader reader) => this with {
			Player = Main.player[reader.ReadByte()],
			DashDirection = reader.ReadSByte(),
			DashDirectionY = reader.ReadSByte(),
		};
		public override void NetSend(BinaryWriter writer) {
			writer.Write((byte)Player.whoAmI);
			writer.Write((sbyte)DashDirection);
			writer.Write((sbyte)DashDirectionY);
		}
		protected override void Perform() {
			OriginPlayer originPlayer = Player.OriginPlayer();
			originPlayer.dashDirection = DashDirection;
			originPlayer.dashDirectionY = DashDirectionY;
		}
	}
}
