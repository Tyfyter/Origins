using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
    public class Avulsion : ModItem {
        static short glowmask;
        public string[] Categories => new string[] {
            "Torn",
            "TornSource"
        };
        public override void SetStaticDefaults() {
            Item.staff[Item.type] = true;
            glowmask = Origins.AddGlowMask(this);
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RubyStaff);
			Item.damage = 20;
            Item.noMelee = true;
            Item.width = 44;
            Item.height = 44;
            Item.useTime = 37;
			Item.useAnimation = 37;
			Item.shoot = ModContent.ProjectileType<Avulsion_P>();
			Item.shootSpeed = 0.1f;
			Item.mana = 13;
			Item.knockBack = 3f;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item67;
			Item.autoReuse = true;
            Item.glowMask = glowmask;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Encrusted_Bar>(), 5);
            recipe.AddIngredient(ModContent.ItemType<Riven_Carapace>(), 15);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
	public class Avulsion_P : ModProjectile {
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.SpectreWrath);
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = 2;
			Projectile.timeLeft = 600;
			Projectile.ignoreWater = true;
		}
        public override void AI() {
            Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.BlueFairy, 0, 0, 200, new Color(30, 150, 255), 0.4f);
            dust.noGravity = false;
            dust.velocity *= 2f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            OriginGlobalNPC.InflictTorn(target, 480, source: Main.player[Projectile.owner].GetModPlayer<OriginPlayer>());
        }
    }
}
