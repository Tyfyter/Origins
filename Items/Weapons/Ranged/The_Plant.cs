using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dusts;
using Origins.Projectiles;
using Origins.Reflection;
using Origins.UI;
using PegasusLib;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI;
using Terraria.UI.Chat;

namespace Origins.Items.Weapons.Ranged {
	[ReinitializeDuringResizeArrays]
	public class The_Plant : ModItem, ICustomDrawItem {
		public static PlantAmmoType[] ModesByAmmo { get; } = ItemID.Sets.Factory.CreateCustomSet<PlantAmmoType>(null);
		public static int[] AliasedAmmo { get; } = ItemID.Sets.Factory.CreateNamedSet(nameof(AliasedAmmo))
		.RegisterIntSet(-1,
			ItemID.EndlessMusketPouch, ItemID.MusketBall,
			ItemID.EndlessQuiver, ItemID.WoodenArrow
		);
		public int ammoCount = 0;
		public int ammoTimer = 0;
		public int mode = -1;
		public PlantAmmoType SelectedMode => ModesByAmmo.GetIfInRange(mode);
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(int.MaxValue, 5));
		}
		public override void SetDefaults() {
			Item.damage = 10;
			Item.DamageType = DamageClass.Ranged;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 5;
			Item.noMelee = true;
			Item.useTime = 6;
			Item.useAnimation = 6;
			Item.width = 50;
			Item.height = 10;
			Item.shoot = ProjectileID.Bullet;
			Item.shootSpeed = 8;
			Item.UseSound = Origins.Sounds.HeavyCannon.WithPitch(2f);
			Item.value = Item.sellPrice(gold: 6);
			Item.rare = ItemRarityID.Lime;
		}
		public override void UpdateInventory(Player player) {
			if (++ammoTimer > Item.useAnimation * 30 * CombinedHooks.TotalUseAnimationMultiplier(player, Item)) {
				ammoTimer = 0;
				ammoCount += 3 + ammoCount / 40;
				Min(ref ammoCount, 999);
				List<int> unlockedPlantModes = player.OriginPlayer().unlockedPlantModes;
				for (int i = 0; i < player.inventory.Length; i++) {
					Item ammo = player.inventory[i];
					if (ammo is null || ammo.IsAir) continue;
					int ammoType = ammo.type;
					if (AliasedAmmo[ammo.type] != -1) ammoType = AliasedAmmo[ammo.type];
					if (ModesByAmmo[ammoType] is not null && !unlockedPlantModes.Contains(ammoType)) {
						unlockedPlantModes.InsertOrdered(ammoType);
					}
				}
				if (mode == -1 && unlockedPlantModes.Count > 0) mode = unlockedPlantModes[0];
			}
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			if (SelectedMode is null) return;
			tooltips.Add(new(Mod, "Ammo", $"[i:{mode}]{Lang.GetItemNameValue(mode)}"));
			tooltips.Add(new(Mod, "AmmoDescription", SelectedMode.Description.Value));
		}
		public override bool CanUseItem(Player player) => SelectedMode is not null && (!ItemLoader.NeedsAmmo(Item, player) || ammoCount > 0);
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) => damage.Base += SelectedMode?.Damage ?? 0;
		public override void ModifyWeaponKnockback(Player player, ref StatModifier knockback) => knockback.Base += SelectedMode?.Knockback ?? 0;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			type = SelectedMode.ProjectileType;
			Vector2 unit = velocity.Normalized(out _);
			velocity += unit * SelectedMode.ShootSpeed;
			if (ammoCount > 0 && CombinedHooks.CanConsumeAmmo(player, Item, Item)) ammoCount--;
			ammoTimer = 0;
			position += new Vector2(unit.Y, -unit.X) * player.direction * 4 * (Math.Abs(unit.X) - Math.Max(unit.Y, 0));
			if (OriginsSets.Projectiles.DontPushBulletForward[type]) return;
			Vector2 barrelPos = position + unit * CollisionExt.Raymarch(position, unit, 32);
			foreach (NPC npc in Main.ActiveNPCs) {
				if (!npc.friendly && !npc.dontTakeDamage && Collision.CheckAABBvLineCollision(npc.position, npc.Size, position, barrelPos)) return;
			}
			position = barrelPos;
		}
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (Main.gameMenu) return;
			float inventoryScale = Main.inventoryScale;
			ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				FontAssets.ItemStack.Value,
				Math.Max(ammoCount, 0).ToString(),
				position + origin * scale * new Vector2(-1f, 0.4f),
				drawColor,
				0f,
				Vector2.Zero,
				new Vector2(inventoryScale),
				-1f,
				inventoryScale
			);
		}
		public override void AddRecipes() {
			for (int i = 0; i < ItemLoader.ItemCount; i++) {
				Item item = ContentSamples.ItemsByType[i];
				if (item.ammo != AmmoID.Bullet || AliasedAmmo[item.type] != -1 || ModesByAmmo[item.type] is not null) continue;
				Main.instance.LoadItem(item.type);
				Main.QueueMainThreadAction(() => {
					Asset<Texture2D> texture = TextureAssets.Item[item.type];
					texture.Wait();
					Color[] colors = new Color[texture.Width() * texture.Height()];
					texture.Value.GetData(colors);
					Color color = default;
					int mostSaturated = 0;
					for (int j = 0; j < colors.Length; j++) {
						if (colors[j].A == 0) continue;
						int cMin = colors[j].R;
						int cMid = colors[j].G;
						int cMax = colors[j].B;
						MinMax(ref cMin, ref cMid);
						MinMax(ref cMid, ref cMax);
						MinMax(ref cMin, ref cMid);
						if (mostSaturated < cMax - cMin) {
							mostSaturated = cMax - cMin;
							color = colors[j];
						}
					}
					RegisterAmmoType(item.type, item.shoot, item.damage, item.knockBack, item.shootSpeed, color, texture);
				});
			}
		}
		public override void SaveData(TagCompound tag) {
			tag[nameof(ammoCount)] = ammoCount;
			tag[nameof(mode)] = new ItemDefinition(mode);
		}
		public override void LoadData(TagCompound tag) {
			tag.TryGet(nameof(ammoCount), out ammoCount);
			if (tag.TryGet(nameof(mode), out ItemDefinition ammoType) && !ammoType.IsUnloaded) mode = ammoType.Type;
		}
		public static void RegisterAmmoType(int itemType, int projectileType, float damage, float knockback, float shootSpeed, Color color, Asset<Texture2D> texture = null, LocalizedText Description = null) {
			if (texture is null) Main.instance.LoadItem(itemType);
			ModesByAmmo[itemType] = new(
				itemType,
				projectileType,
				damage,
				knockback,
				shootSpeed,
				color,
				texture ?? TextureAssets.Item[itemType],
				Description ?? LocalizationMethods._text.GetValue(Lang.GetTooltip(itemType))
			);
		}
		public static LocalizedText GetBulletTypeDescription(string name) {
			return Language.GetOrRegister($"Mods.Origins.Items.{nameof(The_Plant)}.Description.{name.Replace("The_Plant_", "")}");
		}

		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Player drawPlayer = drawInfo.drawPlayer;
			Vector2 itemPos = Main.DrawPlayerItemPos(drawPlayer.gravDir, Type);
			itemPos.X -= 8;
			itemPos.Y -= 8;
			DrawAnimation animation = Main.itemAnimations[Type];
			Rectangle frame = animation.GetFrame(itemTexture);
			drawOrigin = new Vector2(-itemPos.X, frame.Height / 2f);
			if (drawPlayer.direction == -1) {
				drawOrigin = new Vector2(itemTexture.Width + itemPos.X, frame.Height / 2f);
			}
			float itemScale = drawPlayer.GetAdjustedItemScale(Item);
			itemPos = drawInfo.ItemLocation + itemPos * Vector2.UnitY - Main.screenPosition;
			frame.Y = (frame.Height + 2) * (int)(animation.FrameCount * (drawPlayer.itemAnimation / (drawPlayer.itemAnimationMax + 1f)));
			drawInfo.DrawDataCache.Add(new DrawData(
				itemTexture,
				itemPos,
				frame,
				Item.GetAlpha(lightColor),
				drawPlayer.itemRotation,
				drawOrigin,
				itemScale,
				drawInfo.itemEffect
			));
			drawInfo.DrawDataCache.Add(new DrawData(
				TextureAssets.GlowMask[Item.glowMask].Value,
				itemPos,
				frame,
				Color.White,
				drawPlayer.itemRotation,
				drawOrigin,
				itemScale,
				drawInfo.itemEffect
			));
		}
	}
	public class The_Plant_Flower : ItemModeFlowerMenu<PlantAmmoType, bool> {
		public override bool IsActive() => Main.LocalPlayer.HeldItem?.ModItem is The_Plant;
		AutoLoadingAsset<Texture2D> wireMiniIcons = "Origins/Items/Tools/Wiring/Mini_Wire_Icons";
		public override float DrawCenter() => 18.4f;
		public override bool GetData(PlantAmmoType mode) => Main.LocalPlayer.HeldItem?.ModItem is The_Plant plant && plant.SelectedMode == mode;
		public override bool GetCursorAreaTexture(PlantAmmoType mode, out Texture2D texture, out Rectangle? frame, out Color color) {
			texture = wireMiniIcons;
			frame = new Rectangle(12, 0, 10, 10);
			color = mode.Color;
			return Main.LocalPlayer.HeldItem?.ModItem is The_Plant plant && plant.mode == mode.ItemType;
		}
		public override void Click(PlantAmmoType mode) {
			if (Main.LocalPlayer.HeldItem?.ModItem is The_Plant plant) {
				plant.mode = mode.ItemType;
			}
		}
		public override IEnumerable<PlantAmmoType> GetModes() {
			List<int> unlockedPlantModes = Main.LocalPlayer.OriginPlayer().unlockedPlantModes;
			for (int i = 0; i < unlockedPlantModes.Count; i++) {
				yield return The_Plant.ModesByAmmo[unlockedPlantModes[i]];
			}
		}
	}
	public record class PlantAmmoType(int ItemType, int ProjectileType, float Damage, float Knockback, float ShootSpeed, Color Color, Asset<Texture2D> Texture, LocalizedText Description) : IFlowerMenuItem<bool> {
		public void Draw(Vector2 position, bool hovered, bool extraData) {
			Main.spriteBatch.Draw(
				Texture.Value,
				position,
				null,
				Color.White * ((hovered || extraData) ? 1 : 0.5f),
				0,
				Texture.Size() * 0.5f,
				1.15f,
				SpriteEffects.None,
			0);
			if (hovered) UICommon.TooltipMouseText(Lang.GetItemNameValue(ItemType));
		}
		public bool IsHovered(Vector2 position) => Main.MouseScreen.WithinRange(position, Math.Max(Texture.Width(), Texture.Height()) * 0.5f * 1.15f);
	}
	public class The_Plant_Chlorophyte_Bullet : ModProjectile {
		public static float MaxTargetDistance => 300;
		public static float HomingSpeed => 8;
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.ChlorophyteBullet;
		public override void SetStaticDefaults() {
			The_Plant.RegisterAmmoType(ItemID.ChlorophyteBullet, Type, 10, 5, 6, new(147, 195, 69));
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 20;
			OriginsSets.Projectiles.DontPushBulletForward[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ChlorophyteBullet);
			Projectile.aiStyle = 0;
		}
		public Entity Target {
			get {
				switch (Projectile.ai[0]) {
					case 1:
					return Main.npc[(int)Projectile.ai[1]];
					case 2:
					return Main.player[(int)Projectile.ai[1]];
				}
				return null;
			}
			set {
				Projectile.ai[1] = value?.whoAmI ?? 0;
				if (value is NPC) {
					Projectile.ai[0] = 1;
				} else if (value is Player) {
					Projectile.ai[0] = 2;
				} else {
					Projectile.ai[0] = 0;
				}
			}
		}
		public override void AI() {
			if (Projectile.alpha > 0)
				Projectile.alpha -= 25;

			if (Projectile.alpha < 0)
				Projectile.alpha = 0;

			float maxDistance = MaxTargetDistance * MaxTargetDistance;
			if (Projectile.ai[0] == 0f) {
				Main.player[Projectile.owner].DoHoming(target => {
					if (CanTarget(target)) {
						float distance = Projectile.Center.Clamp(target.Hitbox).DistanceSQ(Projectile.Center);
						if (distance < maxDistance && Collision.CanHit(Projectile.Center, 1, 1, target.position, target.width, target.height)) {
							maxDistance = distance;
							Target = target;
							return true;
						}
					}
					return false;
				});
			}
			if (Target is not null) {
				if (CanTarget(Target)) {
					Projectile.velocity = (Projectile.velocity + (Target.Center - Projectile.Center).Normalized(out _) * (HomingSpeed)).Normalized(out _) * Projectile.velocity.Length();
				} else {
					Target = null;
				}
			}
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			bool CanTarget(Entity entity) {
				if (entity is NPC npc) return npc.CanBeChasedBy(Projectile);
				if (entity is Player player) return !player.dead && player.hostile && player.team != Main.player[Projectile.owner].team;
				return false;
			}
		}
		public override void OnKill(int timeLeft) {
			if (NetmodeActive.Server) return;
			if (Projectile.owner != Main.myPlayer) {
				if (!Projectile.hide) {
					Projectile.hide = true;
					try {
						Projectile.active = true;
						Projectile.timeLeft = timeLeft;
						Projectile.Update(Projectile.whoAmI);
					} finally {
						Projectile.active = false;
						Projectile.timeLeft = 0;
					}
				}
				return;
			}
			Vector2[] oldPos = [.. Projectile.oldPos];
			float[] oldRot = [.. Projectile.oldRot];
			for (int i = 0; i < oldPos.Length; i++) {
				if (oldPos[i] == default) {
					Array.Resize(ref oldPos, i);
					Array.Resize(ref oldRot, i);
					break;
				}
				oldPos[i] += Projectile.Size * 0.5f;
				oldRot[i] += MathHelper.PiOver2;
			}
			Dust.NewDustPerfect(
				Main.LocalPlayer.Center,
				ModContent.DustType<Vertex_Trail_Dust>(),
				Vector2.Zero
			).customData = new Vertex_Trail_Dust.TrailData(oldPos, oldRot, StripColors, StripWidth, Projectile.extraUpdates);
		}
		private static readonly VertexStrip _vertexStrip = new();
		public override bool PreDraw(ref Color lightColor) {
			MiscShaderData miscShaderData = GameShaders.Misc["RainbowRod"];
			Vector2[] oldPos = [.. Projectile.oldPos];
			float[] oldRot = [.. Projectile.oldRot];
			for (int i = 0; i < oldPos.Length; i++) {
				if (oldPos[i] == default) {
					Array.Resize(ref oldPos, i);
					Array.Resize(ref oldRot, i);
					break;
				}
				oldRot[i] += MathHelper.PiOver2;
			}
			miscShaderData.UseSaturation(-2.8f);
			miscShaderData.UseOpacity(4f);
			miscShaderData.Apply();
			_vertexStrip.PrepareStripWithProceduralPadding(oldPos, oldRot, StripColors, StripWidth, -Main.screenPosition + Projectile.Size / 2f);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			return false;
		}
		Color StripColors(float progressOnStrip) {
			if (float.IsNaN(progressOnStrip)) return Color.Transparent;
			float lerpValue = 1f - Utils.GetLerpValue(0f, 0.2f, progressOnStrip * progressOnStrip, clamped: true);
			return Color.Lerp(new Color(255, 40, 12), new Color(255, 180, 12), lerpValue) * (1f - lerpValue * lerpValue) * Projectile.Opacity;
		}
		float StripWidth(float progressOnStrip) {
			float lerpValue = 1f - Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true);
			return MathHelper.Lerp(0, 12, 1f - lerpValue * lerpValue) * Projectile.Opacity;
		}
	}
	public class The_Plant_Explosive_Bullet : ModProjectile, IIsExplodingProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.ExplosiveBullet;
		public override void SetStaticDefaults() {
			The_Plant.RegisterAmmoType(ItemID.ExplodingBullet, Type, 11, 7, 5, new(196, 40, 18));
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ExplosiveBullet);
			Projectile.penetrate = 2;
			Projectile.aiStyle = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			if (Projectile.alpha > 0)
				Projectile.alpha -= 15;
			if (Projectile.alpha < 0)
				Projectile.alpha = 0;
			Projectile.BulletShimmer();
		}
		public override void OnKill(int timeLeft) {
			Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
			Explode();
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Explode();
		}
		public bool IsExploding { get; set; }
		public void Explode(int delay = 0) {
			if (IsExploding) return;
			try {
				IsExploding = true;
				int penetrate = Projectile.penetrate;
				Vector2 position = Projectile.position;
				Vector2 size = Projectile.Size;
				ExplosiveGlobalProjectile.DoExplosion(Projectile, 80, sound: SoundID.Item14, fireDustAmount: 5, smokeDustAmount: 7, smokeGoreAmount: 0);
				Gore.NewGoreDirect(null, Projectile.Center, default, Main.rand.Next(61, 64));
				Projectile.penetrate = penetrate;
				Projectile.position = position;
				Projectile.Size = size;
			} catch {
				IsExploding = false;
				throw;
			}
			IsExploding = false;
		}
	}
	public class The_Plant_Venom_Bullet : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.VenomBullet;
		public override void SetStaticDefaults() {
			The_Plant.RegisterAmmoType(ItemID.VenomBullet, Type, 15, 0, 6f, new(151, 79, 162), Description: The_Plant.GetBulletTypeDescription(Name));
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.VenomBullet);
			Projectile.aiStyle = 0;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			if (Projectile.alpha > 0)
				Projectile.alpha -= 15;
			if (Projectile.alpha < 0)
				Projectile.alpha = 0;
			Projectile.BulletShimmer();
		}
		public override void OnKill(int timeLeft) {
			Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Tetanus_Debuff.ID, 60 * 5);
		}
	}
	public class The_Plant_Nano_Bullet : ModProjectile {
		public static float MaxTargetDistance => 800;
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.NanoBullet;
		public override void SetStaticDefaults() {
			The_Plant.RegisterAmmoType(ItemID.NanoBullet, Type, 16, 4, 5, new(0, 255, 255), Description: The_Plant.GetBulletTypeDescription(Name));
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 20;
			OriginsSets.Projectiles.DontPushBulletForward[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.NanoBullet);
			Projectile.extraUpdates += 1;
			Projectile.aiStyle = 0;
		}
		public override void AI() {
			if (Projectile.alpha > 0)
				Projectile.alpha -= 25;

			if (Projectile.alpha < 0)
				Projectile.alpha = 0;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Vector2 oldDir = oldVelocity.Normalized(out float speed);
			Projectile.position += oldDir * CollisionExt.Raymarch(Projectile.velocity + oldDir * (Projectile.width * 0.5f), oldDir, speed);
			float maxDistance = MaxTargetDistance * MaxTargetDistance;
			Entity bounceTarget = null;
			Main.player[Projectile.owner].DoHoming(target => {
				if (CanTarget(target)) {
					float distance = Projectile.Center.Clamp(target.Hitbox).DistanceSQ(Projectile.Center);
					if (distance < maxDistance && Collision.CanHit(Projectile.Center, 1, 1, target.position, target.width, target.height)) {
						maxDistance = distance;
						bounceTarget = target;
						return true;
					}
				}
				return false;
			});
			if (bounceTarget is null) {
				Projectile.velocity = oldVelocity * new Vector2((Projectile.velocity.X == oldVelocity.X).ToDirectionInt(), (Projectile.velocity.Y == oldVelocity.Y).ToDirectionInt());
			} else {
				Projectile.velocity = (bounceTarget.Center - Projectile.Center).Normalized(out _) * speed;
			}
			return false;
			bool CanTarget(Entity entity) {
				if (entity is NPC npc) return npc.CanBeChasedBy(Projectile);
				if (entity is Player player) return !player.dead && player.hostile && player.team != Main.player[Projectile.owner].team;
				return false;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Choice_Paralysis_Debuff.ID, Main.rand.NextBool(3) ? 180 : 60);
		}
		public override void OnKill(int timeLeft) {
			if (NetmodeActive.Server) return;
			if (Projectile.owner != Main.myPlayer) {
				if (!Projectile.hide) {
					Projectile.hide = true;
					try {
						Projectile.active = true;
						Projectile.timeLeft = timeLeft;
						Projectile.Update(Projectile.whoAmI);
					} finally {
						Projectile.active = false;
						Projectile.timeLeft = 0;
					}
				}
				return;
			}
			Vector2[] oldPos = [.. Projectile.oldPos];
			float[] oldRot = [.. Projectile.oldRot];
			for (int i = 0; i < oldPos.Length; i++) {
				if (oldPos[i] == default) {
					Array.Resize(ref oldPos, i);
					Array.Resize(ref oldRot, i);
					break;
				}
				oldPos[i] += Projectile.Size * 0.5f;
				oldRot[i] += MathHelper.PiOver2;
			}
			Dust.NewDustPerfect(
				Main.LocalPlayer.Center,
				ModContent.DustType<Vertex_Trail_Dust>(),
				Vector2.Zero
			).customData = new Vertex_Trail_Dust.TrailData(oldPos, oldRot, StripColors, StripWidth, Projectile.extraUpdates);
		}
		private static readonly VertexStrip _vertexStrip = new();
		public override bool PreDraw(ref Color lightColor) {
			MiscShaderData miscShaderData = GameShaders.Misc["RainbowRod"];
			Vector2[] oldPos = [.. Projectile.oldPos];
			float[] oldRot = [.. Projectile.oldRot];
			for (int i = 0; i < oldPos.Length; i++) {
				if (oldPos[i] == default) {
					Array.Resize(ref oldPos, i);
					Array.Resize(ref oldRot, i);
					break;
				}
				oldRot[i] += MathHelper.PiOver2;
			}
			miscShaderData.UseSaturation(-2.8f);
			miscShaderData.UseOpacity(4f);
			miscShaderData.Apply();
			_vertexStrip.PrepareStripWithProceduralPadding(oldPos, oldRot, StripColors, StripWidth, -Main.screenPosition + Projectile.Size / 2f);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			return false;
		}
		Color StripColors(float progressOnStrip) {
			if (float.IsNaN(progressOnStrip)) return Color.Transparent;
			float lerpValue = 1f - Utils.GetLerpValue(0f, 0.2f, progressOnStrip * progressOnStrip, clamped: true);
			return Color.Lerp(new Color(255, 40, 12), new Color(0, 255, 255), lerpValue) * (1f - lerpValue * lerpValue) * Projectile.Opacity;
		}
		float StripWidth(float progressOnStrip) {
			float lerpValue = 1f - Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true);
			return MathHelper.Lerp(0, 12, 1f - lerpValue * lerpValue) * Projectile.Opacity;
		}
	}
	public class The_Plant_Luminite_Bullet : ModProjectile {
		public override void SetStaticDefaults() {
			The_Plant.RegisterAmmoType(ItemID.MoonlordBullet, Type, 21, 3.5f, 2.5f, new(34, 221, 151));
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.MoonlordBullet);
			Projectile.aiStyle = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			if (Projectile.alpha > 0)
				Projectile.alpha -= (int)(Projectile.velocity.Length() * 0.5f);

			if (Projectile.alpha < 0)
				Projectile.alpha = 0;

			Rectangle hitbox = Projectile.Hitbox;
			hitbox.Offset((int)Projectile.velocity.X, (int)Projectile.velocity.Y);
			bool willHit = false;
			foreach (NPC npc in Main.ActiveNPCs) {
				if (!npc.dontTakeDamage && npc.immune[Projectile.owner] == 0 && Projectile.localNPCImmunity[npc.whoAmI] == 0 && npc.Hitbox.Intersects(hitbox) && !npc.friendly) {
					willHit = true;
					break;
				}
			}

			if (willHit) {
				for (int i = Main.rand.Next(15, 31); i > 0; i--) {
					Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.SolarFlare, 0f, 0f, 100, default, 0.8f);
					dust.velocity *= 1.6f;
					dust.velocity.Y -= 1f;
					dust.velocity += Projectile.velocity;
					dust.noGravity = true;
				}
			}
			Projectile.BulletShimmer();
			Projectile.wet = false;
		}
		public override Color? GetAlpha(Color lightColor) => new Color(255, 200, 100, 100) * Projectile.Opacity;
		public override void OnKill(int timeLeft) {
			Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.damage = (int)(Projectile.damage * 0.98f);
		}
	}
	public class The_Plant_Crystal_Bullet : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.CrystalBullet;
		public override void SetStaticDefaults() {
			The_Plant.RegisterAmmoType(ItemID.CrystalBullet, Type, 9, 0.8f, 6, new(69, 120, 153));
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.CrystalBullet);
			Projectile.aiStyle = 0;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			if (Projectile.alpha > 0)
				Projectile.alpha -= 15;
			if (Projectile.alpha < 0)
				Projectile.alpha = 0;
			Projectile.BulletShimmer();
		}
		public override Color? GetAlpha(Color lightColor) => new Color(255, 200, 100, 100) * Projectile.Opacity;
		public override void OnKill(int timeLeft) {
			Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
			int type = ModContent.ProjectileType<Impeding_Shrapnel_Shard>();
			for (int i = Math.Min(Main.rand.Next(2, 4), Main.rand.Next(2, 4)); i > 0; i--) {
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(2, 4),
					type,
					Projectile.damage / 2,
					Projectile.knockBack
				);
			}
		}
	}
}
