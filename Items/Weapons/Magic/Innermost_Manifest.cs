using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.NPCs;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Magic {
	public class Innermost_Manifest : ModItem, ICustomWikiStat, ITornSource {
		public static float TornSeverity => 0.25f;
		float ITornSource.Severity => TornSeverity;
		public string[] Categories => [
			WikiCategories.UsesBookcase,
			WikiCategories.SpellBook
		];
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RubyStaff);
			Item.damage = 8;
			Item.DamageType = DamageClass.Magic;
			Item.noMelee = true;
			Item.width = 28;
			Item.height = 30;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.mana = 8;
			Item.shoot = ModContent.ProjectileType<Innermost_Manifest_P>();
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.SoulofNight, 15)
			.AddIngredient(ItemID.SpellTome)
			.AddIngredient(ModContent.ItemType<Alkahest>(), 20)
			.AddTile(TileID.Bookcases)
			.Register();
		}
	}
	public class Innermost_Manifest_P : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 2;
			ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RubyBolt);
			Projectile.extraUpdates = 1;
			Projectile.penetrate = 10;
			Projectile.hide = true;
			Projectile.alpha = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 60;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[0] = -1;
		}
		public override void AI() {
			Dust dust = Dust.NewDustDirect(Projectile.Center, -11, 0, DustID.Ichor, 0, 0, 255, new Color(220, 220, 200), 0.8f);
			dust.noGravity = false;
			dust.velocity *= 1.2f;
			if (Projectile.ai[0] >= 0) {
				NPC embedTarget = Main.npc[(int)Projectile.ai[0]];
				if (embedTarget.active) {
					Projectile.Center = embedTarget.Center;
				} else {
					Projectile.Kill();
				}
			} else {
				Projectile.rotation = Projectile.velocity.ToRotation();
				if (++Projectile.frameCounter >= 5) {
					Projectile.frame ^= 1;
					Projectile.frameCounter = 0;
				}
			}
		}
		public override bool? CanHitNPC(NPC target) {
			return Projectile.ai[0] >= 0 ? target.whoAmI == (int)Projectile.ai[0] : null;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.ai[0] < 0) {
				Projectile.ai[0] = target.whoAmI;
				Projectile.ArmorPenetration = (target.defense + 2);
				Projectile.damage = 2;
				Projectile.knockBack = 0;
			}
			OriginGlobalNPC.InflictTorn(target, 60, 120, Innermost_Manifest.TornSeverity, source: Main.player[Projectile.owner].GetModPlayer<OriginPlayer>());
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCs.Add(index);
		}
	}
}
