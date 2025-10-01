using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using Origins.Journal;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Items.Weapons.Ranged {
	public class Sixth_Spline : ModItem, ICustomWikiStat, IJournalEntrySource {
		static short glowmask;
		public static WeightedRandom<Sixth_Spline_Projectile> Projectiles { get; private set; }  = new();
        public string[] Categories => [
            "Gun"
        ];
		public string EntryName => "Origins/" + typeof(Fifth_Spline_Entry).Name;
		public class Fifth_Spline_Entry : JournalEntry {
			public override string TextKey => "Fifth_Spline";
			public override JournalSortIndex SortIndex => new("Mechanicus_Sovereignty", 6);
		}
		public static int ID { get; set; }
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [Slow_Debuff.ID, BuffID.OnFire3, BuffID.Bleeding, Static_Shock_Debuff.ID];
			ID = Type;
		}
		public override void SetDefaults() {
			Item.DefaultToRangedWeapon(ProjectileID.Bullet, ModContent.ItemType<Scrap>(), 10, 5);
			Item.damage = 60;
			Item.crit = -4;
			Item.width = 86;
			Item.height = 22;
			Item.autoReuse = true;
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item11;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddRecipeGroup("AdamantiteBars", 10)
			.AddIngredient(ModContent.ItemType<Phoenum>(), 18)
			.AddIngredient(ModContent.ItemType<Scrap>(), 12)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override Vector2? HoldoutOffset() => Vector2.Zero;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			Sixth_Spline_Projectile proj = Projectiles.Get();
			type = proj.Type;
			damage = (int)(damage * proj.DamageMult);
			knockback *= proj.KnockbackMult;
			velocity = velocity.RotatedByRandom(0.1f);
		}
	}
	public record struct Sixth_Spline_Projectile(int Type, float DamageMult, float KnockbackMult);
	public class Sixth_Spline_Nut : ModProjectile {
		public override void SetStaticDefaults() {
			Sixth_Spline.Projectiles.Add(new(Type, 1, 1), 1.2f);
		}
		public override void SetDefaults() {
			Projectile.width = Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 1;
		}
		public override void AI() {
			Projectile.rotation += Projectile.direction * 0.5f;
			Projectile.velocity.Y += 0.03f;
		}
	}
	public class Sixth_Spline_Wrench : Sixth_Spline_Nut {
		public override void SetStaticDefaults() {
			Sixth_Spline.Projectiles.Add(new(Type, 0.9f, 0.9f));
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Slow_Debuff.ID, 60);
		}
	}
	public class Sixth_Spline_Piston : Sixth_Spline_Nut {
		public override void SetStaticDefaults() {
			Sixth_Spline.Projectiles.Add(new(Type, 0.95f, 2f));
		}
	}
	public class Sixth_Spline_Soldering_Iron : Sixth_Spline_Nut {
		public override void SetStaticDefaults() {
			Sixth_Spline.Projectiles.Add(new(Type, 0.9f, 0.9f), 0.75f);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire3, 300);
			target.AddBuff(BuffID.Bleeding, 300);
		}
	}
	public class Sixth_Spline_Battery : Sixth_Spline_Nut {
		public override string Texture => typeof(Power_Core).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Sixth_Spline.Projectiles.Add(new(Type, 1.2f, 0.6f), 0.5f);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Static_Shock_Debuff.Inflict(target, 300);
		}
	}
}
