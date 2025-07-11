using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOn)]
	public class Gun_Glove : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat"
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ItemID.FeralClaws] = ModContent.ItemType<Gun_Glove>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Gun_Glove>()] = ItemID.FeralClaws;
            glowmask = Origins.AddGlowMask(this);
			On_Player.ItemCheck_UseArtisanLoaf += On_Player_ItemCheck_UseArtisanLoaf;

		}
		private void On_Player_ItemCheck_UseArtisanLoaf(On_Player.orig_ItemCheck_UseArtisanLoaf orig, Player self, Item sItem) {
			orig(self, sItem);
			try {
				if (self.ItemAnimationActive && OriginsSets.Items.SwungNoMeleeMelees[sItem.type]) self.OriginPlayer().DoGunGlove();
			} catch { }
		}

		static short glowmask;
        public override void SetDefaults() {
			Item.DefaultToAccessory(24, 18);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Green;
			Item.damage = 10;
			Item.DamageType = DamageClass.Ranged;
			Item.useTime = 5;
			Item.useAnimation = 5;
			Item.shootSpeed = 5;
			Item.useAmmo = AmmoID.Bullet;
			Item.UseSound = SoundID.Item10;
            Item.glowMask = glowmask;
        }
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.gunGlove = true;
			originPlayer.gunGloveItem = Item;
		}
		public override bool RangedPrefix() => false;
	}
}
