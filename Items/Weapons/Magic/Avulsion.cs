using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.NPCs;
using PegasusLib;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
	public class Avulsion : ModItem, ICustomWikiStat, ITornSource {
		public static float TornSeverity => 0.3f;
		float ITornSource.Severity => TornSeverity;
		static short glowmask;
        public string[] Categories => [
            "Torn",
            "TornSource",
            "Wand"
        ];
        public override void SetStaticDefaults() {
            Item.staff[Item.type] = true;
            glowmask = Origins.AddGlowMask(this);
        }
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RubyStaff);
			Item.damage = 28;
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
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			Vector2 vel = velocity.SafeNormalize(default);
			if (vel != default) {
				position += vel * CollisionExt.Raymarch(position, vel, 56);
			}
		}
		public override void AddRecipes() {
            Recipe.Create(Type)
            .AddIngredient(ModContent.ItemType<Encrusted_Bar>(), 8)
            .AddTile(TileID.Anvils)
            .Register();
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
            OriginGlobalNPC.InflictTorn(target, 480, targetSeverity: Avulsion.TornSeverity, source: Main.player[Projectile.owner].GetModPlayer<OriginPlayer>());
        }
    }
}
