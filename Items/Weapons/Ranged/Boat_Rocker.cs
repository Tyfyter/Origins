using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Ammo;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Ranged {
    public class Boat_Rocker : Harpoon_Gun, ICustomWikiStat {
        public new string[] Categories => [
            "HarpoonGun"
        ];
        public override void SetDefaults() {
			Item.damage = 48;
			Item.DamageType = DamageClass.Ranged;
			Item.knockBack = 4;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.useAnimation = 3;
			Item.useTime = 3;
			Item.reuseDelay = 2;
			Item.width = 48;
			Item.height = 22;
			Item.useAmmo = Harpoon.ID;
			Item.shoot = Harpoon_P.ID;
			Item.shootSpeed = 14f;
			Item.UseSound = SoundID.Item11;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
			Item.autoReuse = true;
		}
		public override Vector2? HoldoutOffset() => new Vector2(-8, 0);
		public override void OnConsumeAmmo(Item ammo, Player player) {
			if (!Main.rand.NextBool(4)) {
				ammo.stack++;
			} else {
				consume = true;
			}
		}
		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			if (player.controlUseTile && player.releaseUseTile) {
				player.GetModPlayer<OriginPlayer>().boatRockerAltUse = true;
			}
		}
	}
}
