using Origins.Items.Armor.Vanity.Dev.PlagueTexan;
using Origins.Items.Pets;
using Origins.Items.Weapons.Melee;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Vanity.Dev.KonoDioDa {
	public class Kono_Dio_Da_Set : DevSet<Kono_Dio_Da_Wings> {
		public override IEnumerable<ItemTypeDropRuleWrapper> GetDrops() {
			yield return new(ItemDropRule.ByCondition(DropConditions.HardmodeBossBag, ModContent.ItemType<Kono_Dio_Da_Wings>()));
		}
	}
	public class Kono_Dio_Da_Wings : ModItem {
		public override string Texture => "Terraria/Images/Item_17";
		public static int WingsID { get; private set; }
		public override void Load() {
			WingsID = EquipLoader.AddEquipTexture(Mod, $"{GetType().GetDefaultTMLName()}_{EquipType.Wings}", EquipType.Wings, this);
			On_LegacyPlayerRenderer.DrawPlayerFull += On_LegacyPlayerRenderer_DrawPlayerFull;
		}

		private void On_LegacyPlayerRenderer_DrawPlayerFull(On_LegacyPlayerRenderer.orig_DrawPlayerFull orig, LegacyPlayerRenderer self, Terraria.Graphics.Camera camera, Player drawPlayer) {
			bool flip = drawPlayer.OriginPlayer().upsideDown;
			try {
				if (flip) drawPlayer.gravDir *= -1;
				orig(self, camera, drawPlayer);
			} finally {
				if (flip) drawPlayer.gravDir *= -1;
			}
		}

		public override void SetStaticDefaults() {
			ArmorIDs.Wing.Sets.Stats[WingsID] = new(150, 7f);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory();
			Item.wingSlot = WingsID;
		}
		public override bool WingUpdate(Player player, bool inUse) {
			player.OriginPlayer().upsideDown ^= inUse;
			return true;
		}
	}
}
