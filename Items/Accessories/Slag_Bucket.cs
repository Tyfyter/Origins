using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Origins.Dev;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Melee;
using Origins.Layers;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Origins.Dev.WikiPageExporter;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Shield)]
	public class Slag_Bucket : ModItem, ICustomWikiStat, ICustomLinkFormat {
		static short glowmask;
		public string[] Categories => [
			WikiCategories.Combat
		];
		void ICustomWikiStat.ModifyWikiStats(JObject data) {
			data["Name"] = ModContent.GetInstance<ItemWikiProvider>().PageName(this).Replace("_", " ");
		}
		WikiLinkFormatter ICustomLinkFormat.CustomFormatter => new LinkInfo(
			Name: ModContent.GetInstance<ItemWikiProvider>().PageName(this).Replace("_", " "),
			Image: LinkInfo.FromStats)
			.Formatter();
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Accessory_Glow_Layer.AddGlowMask<Shield_Glow_Layer>(Item.shieldSlot, Texture + "_Shield_Glow");
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(48, 36);
			Item.rare = CursedRarity.ID;
			Item.value = Item.sellPrice(gold: 2);
			Item.shoot = ModContent.ProjectileType<Slag_Bucket_Projectile>();
			Item.defense = 3;
			Item.glowMask = glowmask;
		}
		public override void UpdateEquip(Player player) {
			player.noKnockback = true;
			player.fireWalk = true;
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.slagBucketCursed = true;
			originPlayer.slagBucket = true;
			originPlayer.retributionShield = true;
			originPlayer.retributionShieldItem = Item;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Scrap_Barrier>())
			.AddIngredient(ModContent.ItemType<Shield_of_Retribution>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Shield)]
	public class Slag_Bucket_Uncursed : Uncursed_Cursed_Item<Slag_Bucket>, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat
		];
		public override bool HasOwnTexture => true;
		public override string Texture => typeof(Slag_Bucket_Uncursed).GetDefaultTMLName();
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Accessory_Glow_Layer.AddGlowMask<Shield_Glow_Layer>(Item.shieldSlot, Texture + "_Shield_Glow");
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.rare = ItemRarityID.LightRed;
			Item.defense = 6;
			Item.glowMask = glowmask;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.OriginPlayer();
			player.noKnockback = true;
			player.fireWalk = true;
			player.endurance += (1 - player.endurance) * 0.15f;
			player.lifeRegen += 4;
			originPlayer.slagBucket = true;
			originPlayer.retributionShield = true;
			originPlayer.retributionShieldItem = Item;
		}
		public override void AddRecipes() {
			base.AddRecipes();
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Scrap_Barrier_Uncursed>())
			.AddIngredient(ModContent.ItemType<Shield_of_Retribution>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
	public class Slag_Bucket_Projectile : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.ExplosiveBullet;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.BulletHighVelocity);
			Projectile.DamageType = DamageClass.Generic;
			Projectile.penetrate = 1;
			Projectile.alpha = 0;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire, Main.rand.Next(240, 360));
		}
	}
	public class Slag_Bucket_Debuff : ModBuff {
		public override string Texture => "Origins/Buffs/Slag_Bucket_Debuff";
		public override LocalizedText DisplayName => Language.GetOrRegister($"Mods.Origins.Buffs.{nameof(Scrap_Barrier_Debuff)}.{nameof(DisplayName)}");
		public override LocalizedText Description => Language.GetOrRegister($"Mods.Origins.Buffs.{nameof(Scrap_Barrier_Debuff)}.{nameof(Description)}");
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}
		static bool HasScrapBarrierDebuff(Player player, int buffIndex, out int scrapBarrierIndex) {
			if (buffIndex > 0 && player.buffType[buffIndex - 1] == Scrap_Barrier_Debuff.ID) {
				scrapBarrierIndex = buffIndex - 1;
				return true;
			}
			if (buffIndex < player.buffType.Length - 1 && player.buffType[buffIndex + 1] == Scrap_Barrier_Debuff.ID) {
				scrapBarrierIndex = buffIndex + 1;
				return true;
			}
			scrapBarrierIndex = -1;
			return false;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.OriginPlayer().scrapBarrierDebuff = true;
			if (HasScrapBarrierDebuff(player, buffIndex, out int scrapBarrierIndex)) {
				if (player.buffTime[scrapBarrierIndex] % 2 == 0) player.buffTime[buffIndex]++;
			}
		}
		public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams) {
			float buffAlpha = Main.buffAlpha[buffIndex];
			Color color = new(buffAlpha, buffAlpha, buffAlpha, buffAlpha);
			Vector2 textPosition = drawParams.TextPosition;
			if (HasScrapBarrierDebuff(Main.LocalPlayer, buffIndex, out _)) {
				color = color.MultiplyRGBA(new(200, 0, 200));
				textPosition.X += MathF.Sin((float)Main.timeForVisualEffects * 0.05f) * 1;
				textPosition.Y += MathF.Sin((float)Main.timeForVisualEffects * 0.04f) * 0.7f;
			}
			spriteBatch.DrawString(
				FontAssets.ItemStack.Value,
				Lang.LocalizedDuration(new TimeSpan(0, 0, Main.LocalPlayer.buffTime[buffIndex] / 60), abbreviated: true, showAllAvailableUnits: false),
				textPosition,
				color,
				0f,
				default,
				0.8f,
				SpriteEffects.None,
			0f);
		}
	}
}
