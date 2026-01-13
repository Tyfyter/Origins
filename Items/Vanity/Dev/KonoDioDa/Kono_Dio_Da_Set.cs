using Origins.Dev;
using Origins.Items.Armor;
using Origins.Items.Vanity.Dev.PlagueTexan;
using Origins.Items.Pets;
using Origins.Items.Vanity.Dev;
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

namespace Origins.Items.Vanity.Dev.KonoDioDa {
	public class Kono_Dio_Da_Set : DevSet<Kono_Dio_Da_Wings> {
		public override IEnumerable<ItemTypeDropRuleWrapper> GetDrops() {
			yield return ModContent.ItemType<Dio_Helmet>();
			yield return ModContent.ItemType<Dio_Breastplate>();
			yield return ModContent.ItemType<Dio_Greaves>();
			yield return new(ItemDropRule.ByCondition(DropConditions.HardmodeBossBag, ModContent.ItemType<Kono_Dio_Da_Wings>()));
		}
	}
	[AutoloadEquip(EquipType.Head)]
	public class Dio_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public string ArmorSetName => "Plague_Texan_Vanity";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Dio_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Dio_Greaves>();
		public override void SetDefaults() {
			Item.vanity = true;
			Item.rare = ItemRarityID.Cyan;
			Item.value = Item.sellPrice(gold: 5);
		}
	}
	[AutoloadEquip(EquipType.Body)]
	public class Dio_Breastplate : ModItem, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			ArmorIDs.Body.Sets.HidesHands[Item.bodySlot] = false;
		}
		public override void SetDefaults() {
			Item.vanity = true;
			Item.rare = ItemRarityID.Cyan;
			Item.value = Item.sellPrice(gold: 5);
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Dio_Greaves : ModItem, INoSeperateWikiPage {

		public override void SetDefaults() {
			Item.vanity = true;
			Item.rare = ItemRarityID.Cyan;
			Item.value = Item.sellPrice(gold: 5);
		}
	}
	[AutoloadEquip(EquipType.Neck)]
	public class Kono_Dio_Da_Wings : ModItem {
		public override string Texture => "Origins/Items/Vanity/Dev/KonoDioDa/Dio_Scarf";
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
			Item.rare = ItemRarityID.Cyan;
			Item.value = Item.sellPrice(gold: 8);
		}
		public override bool WingUpdate(Player player, bool inUse) => true;
		public override void EquipFrameEffects(Player player, EquipType type) {
			if (player.controlJump && type == EquipType.Wings) {
				player.OriginPlayer().upsideDown ^= true;
				player.bodyFrame.Y = 280;
			}
		}
	}
}
