using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Origins.Items.Weapons.Explosives;
using Origins.Projectiles;
using Origins.Dusts;
using Terraria.GameContent.Creative;

namespace Origins.Items.Weapons.Ammo {
	public class Thermite_Canister : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Thermite Canister");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
			//Tooltip.SetDefault();
		}
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.RocketI);
            Item.damage = 30;
            Item.shoot = ModContent.ProjectileType<Thermite_Canister_P>();
            Item.ammo = Item.type;
        }
    }
    public class White_Solution : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("White Solution");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
            //Tooltip.SetDefault();
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.GreenSolution);
            Item.shoot = ModContent.ProjectileType<White_Solution_P>()-ProjectileID.PureSpray;
        }
    }
    public class White_Solution_P : ModProjectile {
        public override string Texture => "Origins/Projectiles/Pixel";
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.PureSpray);
            Projectile.aiStyle = 0;
        }
        public override void AI() {
            OriginGlobalProj.ClentaminatorAI(Projectile, OriginSystem.origin_conversion_type, ModContent.DustType<Solution_D>(), Color.GhostWhite);
        }
    }
}
