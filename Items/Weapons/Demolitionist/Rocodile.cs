using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Rocodile : ModItem, ICustomWikiStat {
		static short glowmask;
        public string[] Categories => [
            "Launcher"
        ];
        public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[Type] = AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.RocketLauncher];
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ProximityMineLauncher);
			Item.damage = 100;
			Item.useTime = 50;
			Item.useAnimation = 50;
			Item.shoot = ProjectileID.RocketI;
			Item.value = Item.sellPrice(gold: 7);
			Item.rare = ItemRarityID.Lime;
			Item.autoReuse = true;
			Item.glowMask = glowmask;
            Item.ArmorPenetration += 1;
        }
		public override Vector2? HoldoutOffset() {
			return new Vector2(-8f, 0);
		}
	}
}
