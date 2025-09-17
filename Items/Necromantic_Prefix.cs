using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.GameContent;
using Terraria.ModLoader;
using System.Reflection;
using Origins.Dusts;
using Origins.Projectiles;
using Terraria.ID;

namespace Origins.Items {
	public class Necromantic_Prefix : MinionPrefix, IOnHitNPCPrefix, IModifyHitNPCPrefix {
		public static float MaxManaMultiplier => 4;
		public override bool HasDescription => true;
		public override PrefixCategory Category => PrefixCategory.AnyWeapon;
		public override void SetStaticDefaults() {
			Origins.SpecialPrefix[Type] = true;
		}
		public override bool CanRoll(Item item) => item.CountsAsClass(DamageClass.Summon) && !Origins.ArtifactMinion[item.shoot];
		public void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers) {
			const float max_bonus = 0.4f;
			modifiers.FinalDamage *= 1 + max_bonus - target.GetLifePercent() * max_bonus;
		}
		public static void OnHit(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.life <= 0 && projectile.owner == Main.myPlayer) {
				Projectile.NewProjectile(
					projectile.GetSource_OnHit(target),
					target.Center,
					Vector2.Zero,
					ModContent.ProjectileType<Necromantic_Prefix_Orb>(),
					0,
					0,
					ai0: target.lifeMax * 0.75f
				);
			} else {
				Rectangle spawnArea;
				if (projectile.Hitbox.Intersects(target.Hitbox)) {
					spawnArea = Rectangle.Intersect(projectile.Hitbox, target.Hitbox);
				} else {
					Vector2 pos = projectile.Center.Clamp(target.Hitbox);
					spawnArea = new((int)pos.X, (int)pos.Y, 0, 0);
				}
				for (int i = 0; i < 9 + (spawnArea.Width * spawnArea.Height) * 0.015f; i++) {
					Dust dust = Dust.NewDustDirect(spawnArea.TopLeft(), spawnArea.Width, spawnArea.Height, ModContent.DustType<Solution_D>(), 0f, 0f, 40, new(0, 100, 125), 1.5f);
					dust.noGravity = true;
					dust.velocity += (dust.position - spawnArea.Center()).SafeNormalize(default) * 4;
				}
			}
		}
		public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
			OnHit(projectile, target, hit, damageDone);
		}
		public override void ModifyValue(ref float valueMult) {
			valueMult *= 1.25f;
		}
	}
	public class Necromantic_Artifact_Prefix : ArtifactPrefixVariant<Necromantic_Prefix>, IOnHitNPCPrefix, IModifyHitNPCPrefix {
		public override bool HasDescription => true;
		public override PrefixCategory Category => PrefixCategory.AnyWeapon;
		public override void SetStaticDefaults() {
			Origins.SpecialPrefix[Type] = true;
		}
		public override bool CanRoll(Item item) => item.CountsAsClass(DamageClass.Summon) && Origins.ArtifactMinion[item.shoot];
		public void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers) {
			const float max_bonus = 0.6f;
			modifiers.FinalDamage *= 1 + max_bonus - target.GetLifePercent() * max_bonus;
		}
		public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
			Necromantic_Prefix.OnHit(projectile, target, hit, damageDone);
			if (target.life <= 0) {
				Projectile ownerMinion = MinionGlobalProjectile.GetOwnerMinion(projectile);
				if (ownerMinion?.ModProjectile is IArtifactMinion artifactMinion && artifactMinion.Life < artifactMinion.MaxLife) {
					if (artifactMinion.Life < 0 && !artifactMinion.CanDie) artifactMinion.Life = 0;
					float oldHealth = artifactMinion.Life;
					artifactMinion.Life += Math.Min(target.lifeMax / 4, target.lifeMax);
					if (artifactMinion.Life > artifactMinion.MaxLife) artifactMinion.Life = artifactMinion.MaxLife;
					CombatText.NewText(ownerMinion.Hitbox, new(0, 110, 88), (int)Math.Round(artifactMinion.Life - oldHealth), true, dot: true);
				}
			}
		}
		public override void ModifyValue(ref float valueMult) {
			valueMult *= 1.25f;
		}
	}
	public class Necromantic_Prefix_Orb : ModProjectile {
		public override string Texture => "Terraria/Images/NPC_0";
		public override void SetDefaults() {
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.alpha = 255;
			Projectile.tileCollide = false;
			Projectile.extraUpdates = 10;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (player.dead || !player.active) {
				Projectile.Kill();
				return;
			}
			Vector2 direction = player.Center - Projectile.Center;
			float dist = direction.LengthSquared();
			if (dist < 50f && Projectile.Hitbox.Intersects(player.Hitbox)) {
				if (Projectile.owner == Main.myPlayer) {
					CombatText.NewText(player.Hitbox, new(0, 88, 110), Main.rand.RandomRound(Projectile.ai[0]));
					player.OriginPlayer().necromancyPrefixMana += Projectile.ai[0];
				}
				Projectile.Kill();
			} else {
				direction *= 4 / MathF.Sqrt(dist);
				Projectile.velocity = (Projectile.velocity * 15f + direction) / 16f;
				for (int i = 0; i < 3; i++) {
					Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Solution_D>(), 0f, 0f, 20, new(0, 88, 110), 1.1f);
					dust.noGravity = true;
					dust.velocity *= 0f;
					dust.position.X -= Projectile.velocity.X * 0.334f * i;
					dust.position.Y -= Projectile.velocity.Y * -0.334f * i;
				}
			}
		}
	}
	public class Necromantic_Mana_Display : ModResourceOverlay {
		public delegate void DeathDrawer(ResourceOverlayDrawContext context);
		public delegate void DeathBarDrawer(PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet);
		public class DeathBarDrawData(int offsetFromText) {
			public int OffsetFromText => offsetFromText;
			bool soughtDrawerMethods = false;
			public ResourceDrawSettings.TextureGetter ManaPanelDrawer { get; private set; }
			public ResourceDrawSettings.TextureGetter ManaFillingDrawer { get; private set; }
			public bool TryFindDrawMethods(IPlayerResourcesDisplaySet displaySet) {
				if (!soughtDrawerMethods) {
					soughtDrawerMethods = true;
					try {
						ManaPanelDrawer = displaySet.GetType().GetMethod(nameof(ManaPanelDrawer), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).CreateDelegate<ResourceDrawSettings.TextureGetter>(displaySet);
						ManaFillingDrawer = displaySet.GetType().GetMethod(nameof(ManaFillingDrawer), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).CreateDelegate<ResourceDrawSettings.TextureGetter>(displaySet);
					} catch (Exception) {
						throw;
					}
				}
				return ManaPanelDrawer is not null && ManaFillingDrawer is not null;
			}
			public void Draw(PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet) {
				void ManaFillingDrawer(int elementIndex, int firstElementIndex, int lastElementIndex, out Asset<Texture2D> sprite, out Vector2 offset, out float drawScale, out Rectangle? sourceRect) {
					float mana = Main.LocalPlayer.OriginPlayer().necromancyPrefixMana;
					offset = Vector2.Zero;
					sprite = barTextures[0];
					drawScale = 1f;
					sourceRect = mana >= snapshot.ManaMax * Necromantic_Prefix.MaxManaMultiplier ? null : new Rectangle(0, 0, sprite.Width(), 0);
					if (elementIndex == lastElementIndex) {
						sprite = barTextures[2];
						offset = new Vector2(-22f, -6f);
						sourceRect = null;
					} else if (elementIndex != firstElementIndex) {
						sprite = barTextures[1];
						int drawIndexOffset = lastElementIndex - (elementIndex - firstElementIndex) - elementIndex;
						offset.X = drawIndexOffset * sprite.Width() - 6;
						float segmentAmount = 1f / snapshot.AmountOfManaStars;
						float thisSegmentAmount = Utils.GetLerpValue(segmentAmount * (elementIndex - 1), segmentAmount * elementIndex, mana / (snapshot.ManaMax * Necromantic_Prefix.MaxManaMultiplier), clamped: true);
						Rectangle frame = sprite.Frame();
						int xOffset = (int)(frame.Width * (1f - thisSegmentAmount));
						offset.X += xOffset;
						frame.X += xOffset;
						frame.Width -= xOffset;
						sourceRect = frame;
					}
				}
				Vector2 vector = new(Main.screenWidth - 300 - 22 + 16 - 10, 18 + offsetFromText + 24);
				vector.X += (20 - snapshot.AmountOfManaStars) * barTextures[1].Width();
				bool isHovered = false;
				ResourceDrawSettings resourceDrawSettings = default;
				resourceDrawSettings.ElementCount = snapshot.AmountOfManaStars + 2;
				resourceDrawSettings.ElementIndexOffset = 0;
				resourceDrawSettings.TopLeftAnchor = vector;
				resourceDrawSettings.GetTextureMethod = ManaFillingDrawer;
				resourceDrawSettings.OffsetPerDraw = new Vector2(12, 0f);
				resourceDrawSettings.OffsetPerDrawByTexturePercentile = Vector2.Zero;
				resourceDrawSettings.OffsetSpriteAnchor = Vector2.Zero;
				resourceDrawSettings.OffsetSpriteAnchorByTexturePercentile = Vector2.Zero;
				resourceDrawSettings.StatsSnapshot = snapshot;
				resourceDrawSettings.DisplaySet = displaySet;
				resourceDrawSettings.ResourceIndexOffset = -1;
				resourceDrawSettings.Draw(Main.spriteBatch, ref isHovered);
			}
		}
		public static Dictionary<IPlayerResourcesDisplaySet, DeathDrawer> DeathDrawers { get; private set; } = [];
		public static Dictionary<IPlayerResourcesDisplaySet, DeathBarDrawData> DeathBarDrawers { get; private set; } = [];
		Asset<Texture2D> normalStar;
		static Asset<Texture2D>[] barTextures;
		Asset<Texture2D>[] coolStars;
		public override void Load() {
			normalStar = ModContent.Request<Texture2D>("Origins/UI/Mana_Necromancer");
			barTextures = [
				ModContent.Request<Texture2D>("Origins/UI/Necromana_Panel_Left"),
				ModContent.Request<Texture2D>("Origins/UI/Necromana_Panel_Middle"),
				ModContent.Request<Texture2D>("Origins/UI/Necromana_Panel_Right")
			];
			coolStars = [
				ModContent.Request<Texture2D>("Origins/UI/Star_Necromancer_A"),
				ModContent.Request<Texture2D>("Origins/UI/Star_Necromancer_B"),
				ModContent.Request<Texture2D>("Origins/UI/Star_Necromancer_C"),
				ModContent.Request<Texture2D>("Origins/UI/Star_Necromancer_Single"),
			];
			DeathDrawers.Add(Main.ResourceSetsManager.GetDisplaySet("Default"), DoDeathStarDraw);

			DeathDrawers.Add(Main.ResourceSetsManager.GetDisplaySet("New"), DoCoolDeathStarDraw);
			DeathDrawers.Add(Main.ResourceSetsManager.GetDisplaySet("NewWithText"), DoCoolDeathStarDraw);

			DeathBarDrawers.Add(Main.ResourceSetsManager.GetDisplaySet("HorizontalBarsWithFullText"), new DeathBarDrawData(2));
			DeathBarDrawers.Add(Main.ResourceSetsManager.GetDisplaySet("HorizontalBarsWithText"), new DeathBarDrawData(4));
			DeathBarDrawers.Add(Main.ResourceSetsManager.GetDisplaySet("HorizontalBars"), new DeathBarDrawData(0));
		}
		void DoCoolDeathStarDraw(ResourceOverlayDrawContext context) {
			switch (context.texture.Name.Split('\\')[^1]) {
				case "Star_A":
				context.texture = coolStars[0];
				break;
				case "Star_B":
				context.texture = coolStars[1];
				break;
				case "Star_C":
				context.texture = coolStars[2];
				break;
				case "Star_Single":
				context.texture = coolStars[3];
				break;
				default:
				return;
			}
			float displayMana = Main.LocalPlayer.OriginPlayer().necromancyPrefixMana;
			float manaPerSegment = context.snapshot.ManaPerSegment * Necromantic_Prefix.MaxManaMultiplier;
			for (int i = 0; i < context.resourceNumber; i++) {
				displayMana -= manaPerSegment;
				if (displayMana <= 0) return;
			}
			float manaPart = displayMana / manaPerSegment;
			if (manaPart > 1) manaPart = 1;
			context.source = new Rectangle(0, 0, context.texture.Width(), (int)(context.texture.Height() * manaPart));
			context.Draw();
		}
		void DoDeathStarDraw(ResourceOverlayDrawContext context) {
			if (context.texture != TextureAssets.Mana) return;
			float displayHealth = Main.LocalPlayer.OriginPlayer().necromancyPrefixMana;
			float manaPerSegment = context.snapshot.ManaPerSegment * Necromantic_Prefix.MaxManaMultiplier;
			for (int i = 0; i < context.resourceNumber; i++) {
				displayHealth -= manaPerSegment;
				if (displayHealth <= 0) return;
			}
			float mult = 255 / context.color.A;
			if (displayHealth < manaPerSegment) {
				mult *= displayHealth / manaPerSegment;
			}
			context.color *= mult;
			context.texture = normalStar;
			context.Draw();
		}
		public override void Unload() => DeathDrawers = null;
		public override void PostDrawResource(ResourceOverlayDrawContext context) {
			float mana = Main.LocalPlayer.OriginPlayer().necromancyPrefixMana;
			if (mana > 0 && DeathDrawers.TryGetValue(Main.ResourceSetsManager.ActiveSet, out DeathDrawer deathDrawer)) {
				deathDrawer(context);
			}
		}
		public override bool PreDrawResourceDisplay(PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife, ref Color textColor, out bool drawText) {
			float mana = Main.LocalPlayer.OriginPlayer().necromancyPrefixMana;
			if (!drawingLife && mana > 0 && DeathBarDrawers.TryGetValue(Main.ResourceSetsManager.ActiveSet, out DeathBarDrawData deathBarDrawer) && deathBarDrawer.TryFindDrawMethods(displaySet)) {
				bool isHovered = false;
				int num3 = Main.screenWidth - 300 - 22 + 16;
				Vector2 vector2 = new(num3 - 10, deathBarDrawer.OffsetFromText + 18 + 24);
				vector2.X += (20 - snapshot.AmountOfManaStars) * 12;
				ResourceDrawSettings resourceDrawSettings = default;
				resourceDrawSettings.ElementCount = snapshot.AmountOfManaStars + 2;
				resourceDrawSettings.ElementIndexOffset = 0;
				resourceDrawSettings.TopLeftAnchor = vector2;
				resourceDrawSettings.GetTextureMethod = deathBarDrawer.ManaPanelDrawer;
				resourceDrawSettings.OffsetPerDraw = Vector2.Zero;
				resourceDrawSettings.OffsetPerDrawByTexturePercentile = Vector2.UnitX;
				resourceDrawSettings.OffsetSpriteAnchor = Vector2.Zero;
				resourceDrawSettings.OffsetSpriteAnchorByTexturePercentile = Vector2.Zero;
				resourceDrawSettings.StatsSnapshot = snapshot;
				resourceDrawSettings.DisplaySet = displaySet;
				resourceDrawSettings.ResourceIndexOffset = -1;
				resourceDrawSettings.Draw(Main.spriteBatch, ref isHovered);

				deathBarDrawer.Draw(snapshot, displaySet);

				resourceDrawSettings = default;
				resourceDrawSettings.ElementCount = snapshot.AmountOfManaStars;
				resourceDrawSettings.ElementIndexOffset = 0;
				resourceDrawSettings.TopLeftAnchor = vector2 + new Vector2(6f, 6f);
				resourceDrawSettings.GetTextureMethod = deathBarDrawer.ManaFillingDrawer;
				resourceDrawSettings.OffsetPerDraw = new Vector2(12, 0f);
				resourceDrawSettings.OffsetPerDrawByTexturePercentile = Vector2.Zero;
				resourceDrawSettings.OffsetSpriteAnchor = Vector2.Zero;
				resourceDrawSettings.OffsetSpriteAnchorByTexturePercentile = Vector2.Zero;
				resourceDrawSettings.StatsSnapshot = snapshot;
				resourceDrawSettings.DisplaySet = displaySet;
				resourceDrawSettings.Draw(Main.spriteBatch, ref isHovered);
				if (isHovered && ResourceOverlayLoader.DisplayHoverText(snapshot, displaySet, drawingLife: false)) {
					Player localPlayer = Main.LocalPlayer;
					localPlayer.cursorItemIconEnabled = false;
					string text = $"{snapshot.Mana}/{snapshot.ManaMax}\n[c/00586e:{mana}/{snapshot.ManaMax * Necromantic_Prefix.MaxManaMultiplier}]";
					Main.instance.MouseTextHackZoom(text);
					Main.mouseText = true;
				}
				drawText = true;
				return false;
			}
			drawText = true;
			return true;
		}
		public override bool DisplayHoverText(PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife) {
			if (drawingLife) return true;
			float mana = Main.LocalPlayer.OriginPlayer().necromancyPrefixMana;
			if (mana < 1) return true;
			Player localPlayer = Main.LocalPlayer;
			localPlayer.cursorItemIconEnabled = false;
			string text = $"{snapshot.Mana}/{snapshot.ManaMax}\n[c/00586e:{mana}/{snapshot.ManaMax * Necromantic_Prefix.MaxManaMultiplier}]";
			Main.instance.MouseTextHackZoom(text);
			Main.mouseText = true;
			return false;
		}
	}
}
