using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back)]
	public class Superjump_Cape : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Movement
		];
		public static int BackSlot { get; private set; }
		public override void Load() {
			On_Player.UpdatePettingAnimal += On_Player_UpdatePettingAnimal;
		}
		static void On_Player_UpdatePettingAnimal(On_Player.orig_UpdatePettingAnimal orig, Player self) {
			orig(self);
			if (self.OriginPlayer().superJump) {
				int jumpTime = Player.jumpHeight - self.jump;
				if (jumpTime >= 5) Player.jumpSpeed += 2;
				if (jumpTime >= 10) Player.jumpSpeed += 2;
				if (jumpTime >= 15) Player.jumpSpeed += 1;
			}
		}
		public override void SetStaticDefaults() {
			BackSlot = Item.backSlot;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			player.OriginPlayer().superJump = true;
			player.jumpSpeedBoost += 2;
			Player.jumpHeight += 10;
			player.noFallDmg = true;
		}
	}
}
