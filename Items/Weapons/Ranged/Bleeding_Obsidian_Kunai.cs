using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.NPCs;
using Origins.Tiles.Dusk;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
    public class Bleeding_Obsidian_Kunai : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Torn",
			"TornSource"
		};
        public override void SetStaticDefaults() {
            ItemID.Sets.ShimmerTransformToItem[ItemID.MagicDagger] = ModContent.ItemType<Bleeding_Obsidian_Kunai>();
            ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Bleeding_Obsidian_Kunai>()] = ItemID.MagicDagger;
        }
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ThrowingKnife);
			Item.damage = 38;
			Item.useTime = 7;
			Item.useAnimation = 7;
			Item.shootSpeed = 13;
			Item.autoReuse = true;
			Item.consumable = false;
			Item.maxStack = 8;
			Item.shoot = ModContent.ProjectileType<Bleeding_Obsidian_Kunai_P>();
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Pink;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Item>(), 3);
			recipe.AddIngredient(ModContent.ItemType<Silicon>(), 6);
			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();
		}
		public override bool CanUseItem(Player player) {
			return player.ownedProjectileCounts[Item.shoot] < Item.stack;
		}
	}
	public class Bleeding_Obsidian_Kunai_P : ModProjectile {
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PoisonDart);
			Projectile.penetrate = 1;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.trap = false;
			Projectile.timeLeft = 90;
		}
		public override void AI() {
			Dust dust = Main.dust[Terraria.Dust.NewDust(Projectile.Center, 0, 0, DustID.GemAmethyst, 0f, 0f, 125, new Color(200, 0, 200), 0.36f)];
			dust.noGravity = true;
			dust.velocity *= 1.5f;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Bleeding, 300);
			target.AddBuff(BuffID.CursedInferno, 120);
			target.AddBuff(BuffID.Ichor, 180);
			OriginGlobalNPC.InflictTorn(target, 300, 180, 0.1f, source: Main.player[Projectile.owner].GetModPlayer<OriginPlayer>());
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			target.AddBuff(BuffID.Bleeding, 300);
			target.AddBuff(BuffID.CursedInferno, 300);
			target.AddBuff(BuffID.Ichor, 300);
			target.AddBuff(BuffID.OnFire, 300);
			OriginPlayer.InflictTorn(target, 300, targetSeverity: 1f - 0.9f);
			/*PlayerDeathReason reason = new PlayerDeathReason();
			reason.SourceCustomReason = target.name + " cut themselves on broken glass";*/
		}
	}
}
