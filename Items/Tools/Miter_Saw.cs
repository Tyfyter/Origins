using Microsoft.Xna.Framework.Graphics;
using Origins.CrossMod;
using Origins.Dev;
using Origins.Items.Materials;
using PegasusLib;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Miter_Saw : ModItem, ICustomDrawItem, ICustomWikiStat {
		public static float MaxHitsPerAnimation => 8;
		public string[] Categories => [
			"ToolWeapon",
			"OtherMelee"
		];
		static AutoCastingAsset<Texture2D> useTexture;
		public override void SetStaticDefaults() {
			useTexture = ModContent.Request<Texture2D>(Texture + "_Use");
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
			OriginsSets.Items.ItemsThatCanChannelWithRightClick[Type] = true;
			Origins.ItemsThatAllowRemoteRightClick[Type] = true;
		}
		public override void Unload() {
			useTexture = null;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WarAxeoftheNight);
			Item.damage = 18;
			Item.DamageType = DamageClass.Melee;
			Item.axe = 10;
			Item.width = 42;
			Item.height = 38;
			Item.useTime = 5;
			Item.useAnimation = 48;
			Item.knockBack = 0.8f;
			Item.shoot = ModContent.ProjectileType<Miter_Saw_P>();
			Item.shootSpeed = 28;
			Item.value = Item.sellPrice(silver: 40);
			Item.UseSound = SoundID.Item23;
			Item.rare = ItemRarityID.Blue;
			Item.channel = true;
		}
		public override bool AltFunctionUse(Player player) => true;
		public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) {
			if (player.ItemAnimationJustStarted) {
				noHitbox = true;
				return;
			}
			float size = 14 * player.GetAdjustedItemScale(Item);
			Vector2 pos = player.GetCompositeArmPosition(false)
				+ new Vector2(24, -4 * player.direction).RotatedBy(player.compositeFrontArm.rotation + MathHelper.PiOver2 * player.gravDir)
				- new Vector2(size, size);
			size *= 2;
			hitbox = new(
				(int)pos.X,
				(int)pos.Y,
				(int)size,
				(int)size
			);
			itemHitbox = hitbox;
			int cd = Main.rand.RandomRound(player.itemAnimationMax / MaxHitsPerAnimation);
			if (player.attackCD > cd) player.attackCD = cd;
			player.ResetMeleeHitCooldowns();
		}
		Rectangle itemHitbox;
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			Rectangle overlap = Rectangle.Intersect(itemHitbox, target.Hitbox);
			Vector2 dir = new Vector2(4 * hit.HitDirection, 2).RotatedBy(player.compositeFrontArm.rotation);
			for (int i = 0; i < 4; i++) {
				Dust.NewDustPerfect(
					Main.rand.NextVector2FromRectangle(overlap),
					DustID.Torch,
					dir.RotatedByRandom(1f)
				).noGravity = true;
			}
		}
		public override void UseItemFrame(Player player) {
			float fact = (0.5f - (player.itemAnimation / (float)player.itemAnimationMax)) * 2;
			if (player.altFunctionUse == 2) {
				player.itemRotation -= player.direction * player.gravDir * (fact * (Math.Abs(fact + 0.1f) - 0.65f) + 0.2f);
			} else {
				player.itemRotation += player.direction * player.gravDir * fact * (Math.Abs(fact) - 0.5f);
			}
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.itemRotation * player.gravDir - MathHelper.PiOver2 * player.direction);
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ModContent.ItemType<Sanguinite_Bar>(), 10)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool CanShoot(Player player) => player.altFunctionUse == 2;
		public bool DrawOverHand => true;
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Player player = drawInfo.drawPlayer;
			float itemRotation = player.itemRotation - MathHelper.PiOver2 * player.gravDir;
			float direction = player.direction * player.gravDir;
			float scale = player.GetAdjustedItemScale(Item);
			Texture2D texture = useTexture.Value;
			if (drawInfo.itemEffect.HasFlag(SpriteEffects.FlipHorizontally)) {
				drawInfo.itemEffect ^= SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally;
			}

			int frameSource = player.itemAnimation;
			if (Main.projectile.GetIfInRange(player.heldProj) is Projectile heldProj) frameSource = heldProj.frameCounter;
			drawInfo.DrawDataCache.Add(new DrawData(
				texture,
				player.GetCompositeArmPosition(false) - Main.screenPosition,
				texture.Frame(verticalFrames: 2, frameY: (frameSource % 4) / 2),
				Item.GetAlpha(lightColor),
				itemRotation + (MathHelper.Pi * 0.75f) * direction,
				new Vector2(11, direction == 1 ? 27 : 7),
				scale,
				drawInfo.itemEffect,
				1
			));
		}
	}
	public class Miter_Saw_P : ModProjectile {
		public override string Texture => typeof(Miter_Saw).GetDefaultTMLName();
		public override void SetStaticDefaults() { }
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.TitaniumChainsaw);
			Projectile.friendly = false;
			Projectile.hide = true;
		}
		public override void AI() {
			Projectile.position -= Projectile.velocity.SafeNormalize(default) * 4;
			Player player = Main.player[Projectile.owner];
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.itemRotation * player.gravDir - MathHelper.PiOver2 * player.direction);
			player.heldProj = Projectile.whoAmI;
			Projectile.frameCounter = (Projectile.frameCounter + 1) % 4;
		}
	}
	public class Miter_Saw_Crit_Type : CritType<Miter_Saw>, IBrokenContent {
		public string BrokenReason => "Needs alt-function condition, Needs balancing";
		static int PrimaryCritThreshold => 3;
		static int SecondaryCritThreshold => 60;
		static int SecondaryCritMaxCharge => SecondaryCritThreshold + 30;
		public override bool CritCondition(Player player, Item item, Projectile projectile, NPC target, NPC.HitModifiers modifiers) {
			if (player.altFunctionUse != 2) {
				return player.GetModPlayer<Miter_Saw_Player>().IncrementPrimaryHit();
			} else {
				return player.GetModPlayer<Miter_Saw_Player>().CheckSecondaryHit();
			}
		}
		public override float CritMultiplier(Player player, Item item) => 1.2f;
		class Miter_Saw_Player : CritModPlayer {
			int primaryHitNumber = 0;
			float secondaryHitCharge;
			public float SecondaryHitCharge {
				get => secondaryHitCharge;
				set {
					secondaryHitCharge = Math.Max(value, 0);
				}
			}
			public override void ResetEffects() {
				if (Player.ItemAnimationEndingOrEnded || Player.HeldItem.ModItem is not Miter_Saw) {
					primaryHitNumber = 0;
					SecondaryHitCharge = 0;
				}
				if (SecondaryHitCharge > 0) SecondaryHitCharge--;
				if (SecondaryHitCharge > SecondaryCritMaxCharge) SecondaryHitCharge = SecondaryCritMaxCharge;
			}
			public bool IncrementPrimaryHit() => ++primaryHitNumber >= PrimaryCritThreshold;
			public bool CheckSecondaryHit() {
				SecondaryHitCharge += (Player.itemAnimationMax / Miter_Saw.MaxHitsPerAnimation) * 2;
				return IsSecondaryCharged;
			}
			public bool IsSecondaryCharged => SecondaryHitCharge >= SecondaryCritThreshold;
		}
	}
}
