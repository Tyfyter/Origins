using Microsoft.Xna.Framework.Graphics;
using Origins.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools.Wiring {
	public class Ashen_Wrench : ModItem, IWireTool {
		public override string Texture => "Origins/Items/Tools/Wiring/Ashen_Wires";
		public IEnumerable<WireMode> Modes => WireModeLoader.GetSorted(WireMode.Sets.AshenWires);
		public override void Load() {
			Ashen_Wire_Data.Load();
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Actuator);
		}
		public static bool PlaceWire(int i, int j, int wireType) {
			if (!Main.tile[i, j].Get<Ashen_Wire_Data>().GetWire(wireType)) {
				SoundEngine.PlaySound(SoundID.Dig, new(i * 16, j * 16));
				Ashen_Wire_Data.SetWire(i, j, wireType, true);
				return true;
			}
			return false;
		}
		public override bool? UseItem(Player player) {
			if (player.altFunctionUse == 2) {
				return false;
			}
			return PlaceWire(Player.tileTargetX, Player.tileTargetY, 0);
		}
	}
}
