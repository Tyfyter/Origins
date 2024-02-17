using Origins.Dev;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
    public class Pincushion : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Misc",
			"Explosive"
		};
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.EncumberingStone);
			Item.accessory = false;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void UpdateInventory(Player player) {
			player.GetModPlayer<OriginPlayer>().pincushion = true;
		}
		public override bool CanRightClick() {
			if (Terraria.GameInput.PlayerInput.Triggers.Old.MouseRight) {
				return false;
			}
			Item.ChangeItemType(ModContent.ItemType<Pincushion_Inactive>());
			SoundEngine.PlaySound(SoundID.Grab);
			return false;
		}
	}
	public class Pincushion_Inactive : ModItem, ICustomWikiStat {
		public bool ShouldHavePage => false;
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.UncumberingStone);
            Item.accessory = false;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override bool CanRightClick() {
			if (Terraria.GameInput.PlayerInput.Triggers.Old.MouseRight) {
				return false;
			}
			Item.ChangeItemType(ModContent.ItemType<Pincushion>());
			SoundEngine.PlaySound(SoundID.Grab);
			return false;
		}
	}
}
