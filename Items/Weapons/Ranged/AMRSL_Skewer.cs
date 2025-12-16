using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Ammo;
using Origins.NPCs;
using Origins.UI;
using PegasusLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI;
using Terraria.UI.Chat;

namespace Origins.Items.Weapons.Ranged {
	[ReinitializeDuringResizeArrays]
	public class AMRSL_Skewer : ModItem, ICustomDrawItem {
		public static float[] AmmoCount { get; } = ItemID.Sets.Factory.CreateFloatSet(defaultState: 0,
			ItemID.IronOre, 1f / 10,
			ItemID.LeadOre, 1f / 10,
			ItemID.IronBar, 1f / 5,
			ItemID.LeadBar, 1f / 5,
			ItemID.MusketBall, 1f / 25
		);
		public static int[] AmmoProjectile { get; } = ItemID.Sets.Factory.CreateIntSet();
		public static int ActualUseTime => 30 * 2;
		public static int DefaultAmmoMax => 20;
		public int ammoMax = DefaultAmmoMax;
		public int ammoCount = 0;
		public SkewerAmmoList ammoTypes = new();
		int animationFrame = 0;
		int animationCounter = 0;
		public override void Load() {
			On_Item.Prefix += static (orig, self, prefixWeWant) => {
				bool retValue = orig(self, prefixWeWant);
				if (prefixWeWant != -3 && self.ModItem is AMRSL_Skewer skewer) {
					skewer.ammoMax = DefaultAmmoMax - (self.useTime - DefaultAmmoMax);
					Min(ref skewer.ammoCount, skewer.ammoMax);
				}
				return retValue;
			};
		}
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(int.MaxValue, 19));
			ItemID.Sets.SkipsInitialUseSound[Type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 178;
			Item.crit += 10;
			Item.DamageType = DamageClass.Ranged;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 5;
			Item.noMelee = true;
			Item.useAnimation = Item.useTime = DefaultAmmoMax;
			Item.width = 50;
			Item.height = 10;
			Item.shoot = ModContent.ProjectileType<AMRSL_Skewer_Sabot>();
			Item.shootSpeed = 20;
			Item.UseSound = Origins.Sounds.HeavyCannon.WithPitch(-0.2f);
			Item.value = Item.sellPrice(gold: 6);
			Item.rare = ItemRarityID.Lime;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-18, -3);
		}
		public override bool AltFunctionUse(Player player) => false;
		public override bool? UseItem(Player player) {
			if (player.altFunctionUse == 2) {
				player.itemRotation = 0;
				if (animationFrame == 1 && player.ItemAnimationJustStarted) animationCounter = 1;
			} else {
				SoundEngine.PlaySound(Item.UseSound, player.itemLocation);
				SoundEngine.PlaySound(Origins.Sounds.Krunch.WithPitch(2f), player.itemLocation);
				animationCounter = 1;
			}
			return base.UseItem(player);
		}
		public override void UpdateInventory(Player player) {
			switch (animationFrame) {
				case 1:
				if (animationCounter > 0) goto default;
				break;
				case 2:
				if (ammoCount > 0) goto default;
				break;
				default:
				if (++animationCounter >= 2) {
					animationCounter = 0;
					if (++animationFrame >= 19) animationFrame = 0;
					if (animationFrame >= 8) {
						if (Main.rand.NextBool(8)) SoundEngine.PlaySound(SoundID.DrumClosedHiHat, player.itemLocation);
						if (Main.rand.NextBool(8)) SoundEngine.PlaySound(SoundID.DrumFloorTom, player.itemLocation);
						if (Main.rand.NextBool(8)) SoundEngine.PlaySound(SoundID.Item149.WithPitch(1.5f).WithVolume(0.5f), player.itemLocation);
						//SoundEngine.PlaySound(Origins.Sounds.ShrapnelFest.WithPitch(0.4f).WithVolume(0.4f), player.itemLocation);
						SoundEngine.PlaySound(SoundID.Item74.WithVolume(2f), player.itemLocation);
						SoundEngine.PlaySound(SoundID.Item116.WithPitch(1.1f), player.itemLocation);
					}
				}
				break;
			}
			Min(ref ammoCount, ammoMax);
		}
		public override float UseSpeedMultiplier(Player player) => DefaultAmmoMax / (float)ActualUseTime;
		public override bool CanUseItem(Player player) {
			if (player.altFunctionUse == 2) return animationFrame is 1 or 2;
			return animationFrame == 1 && (!ItemLoader.NeedsAmmo(Item, player) || ammoCount > 0);
		}
		public override void UseItemFrame(Player player) {
			if (player.itemTime < 2 && animationFrame is not 1 and not 2) {
				player.SetDummyItemTime(2);
			}
		}
		public override bool CanShoot(Player player) => player.altFunctionUse != 2;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			Vector2 unit = velocity.Normalized(out _);
			if (ammoCount > 0 && CombinedHooks.CanConsumeAmmo(player, Item, Item)) {
				foreach ((int ammoType, float count) in ammoTypes) {
					if (count * 2 >= ammoCount) {
						if (ammoType < 0) {
							type = -ammoType;
							break;
						}
						if (AmmoProjectile[ammoType] != -1) type = AmmoProjectile[ammoType];
						break;
					}
				}
				ammoTypes.Multiply((ammoCount - 1) / (float)ammoCount);
				ammoCount--;
			}
			position += new Vector2(unit.Y, -unit.X) * player.direction * 4 * (Math.Abs(unit.X) - Math.Max(unit.Y, 0));
			if (OriginsSets.Projectiles.DontPushBulletForward[type]) return;
			Vector2 barrelPos = position + unit * CollisionExt.Raymarch(position, unit, 32);
			foreach (NPC npc in Main.ActiveNPCs) {
				if (!npc.friendly && !npc.dontTakeDamage && Collision.CheckAABBvLineCollision(npc.position, npc.Size, position, barrelPos)) return;
			}
			position = barrelPos;
		}
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			spriteBatch.Draw(TextureAssets.Item[Item.type].Value, position, GetFrame(), drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
			return false;
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
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			for (int i = 0; i < tooltips.Count; i++) {
				if (tooltips[i].Name == "PrefixSpeed") {
					TooltipLine line = new(Mod, "PrefixAmmo", this.GetLocalization("PrefixAmmo").Format(ammoMax - DefaultAmmoMax)) {
						IsModifier = true,
						IsModifierBad = ammoMax < DefaultAmmoMax
					};
					if (tooltips[i].IsModifierBad) {
						tooltips.Insert(++i, line);
					} else {
						tooltips[i] = line;
					}
					break;
				}
			}
		}
		public override void SaveData(TagCompound tag) {
			tag[nameof(ammoCount)] = ammoCount;
			int type = -1;
			foreach ((int ammoType, float count) in ammoTypes) {
				if (count * 2 >= ammoCount) {
					if (ammoType < 0) {
						type = -ammoType;
					} else {
						type = AmmoProjectile[ammoType];
					}
					break;
				}
			}
			if (type != -1) tag["ammoType"] = new ProjectileDefinition(type);
		}
		public override void LoadData(TagCompound tag) {
			tag.TryGet(nameof(ammoCount), out ammoCount);
			ammoTypes.Multiply(0);
			if (tag.TryGet("ammoType", out ProjectileDefinition projectile) && !projectile.IsUnloaded) ammoTypes[-projectile.Type] = ammoCount;
		}
		public Rectangle GetFrame() {
			DrawAnimation animation = Main.itemAnimations[Type];
			Rectangle frame = animation.GetFrame(TextureAssets.Item[Item.type].Value);
			frame.Y = (frame.Height + 2) * animationFrame;
			return frame;
		}
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Player drawPlayer = drawInfo.drawPlayer;
			Vector2 itemPos = Main.DrawPlayerItemPos(drawPlayer.gravDir, Type);
			itemPos.X -= 18;
			itemPos.Y -= 16;
			Rectangle frame = GetFrame();
			drawOrigin = new Vector2(-itemPos.X, frame.Height / 2f);
			if (drawPlayer.direction == -1) {
				drawOrigin = new Vector2(itemTexture.Width + itemPos.X, frame.Height / 2f);
			}
			float itemScale = drawPlayer.GetAdjustedItemScale(Item);
			itemPos = drawInfo.ItemLocation + itemPos * Vector2.UnitY - Main.screenPosition;
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
		internal static void SetupAmmoCounts() {
			bool changed;
			do {
				changed = false;
				SkewerAmmoList ammoTypes = new();
				for (int i = 0; i < Main.recipe.Length; i++) {
					Recipe recipe = Main.recipe[i];
					if (recipe.createItem.stack <= 0) continue;
					ammoTypes.Multiply(0);
					float count = 0;
					int junkCount = 0;
					for (int j = 0; j < recipe.requiredItem.Count; j++) {
						Item item = recipe.requiredItem[j];
						float ammoCount = item.stack * AmmoCount[item.type];
						count += ammoCount;
						if (AmmoCount[item.type] == 0) {
							junkCount += item.stack;
						} else {
							ammoTypes[AmmoProjectile[item.type]] += ammoCount;
						}
					}
					if (junkCount > count) continue;
					count /= recipe.createItem.stack;
					if (AmmoCount[recipe.createItem.type] < count) {
						AmmoCount[recipe.createItem.type] = count;
						foreach ((int ammoType, float ammoCount) in ammoTypes) {
							if (ammoCount * 2 >= count) {
								if (ammoType != -1) AmmoProjectile[recipe.createItem.type] = ammoType;
								break;
							}
						}
						changed = true;
					}
				}
			} while (changed);
			AmmoCount[ItemID.IronBar] *= 2;
			AmmoCount[ItemID.LeadBar] *= 2;
		}
	}
	public class Skewer_Forge_Menu : ItemModeFlowerMenu<SkewerAmmoOption, bool> {
		public override bool IsActive() => Main.LocalPlayer.HeldItem?.ModItem is AMRSL_Skewer skewer && skewer.ammoCount < skewer.ammoMax;
		public override float DrawCenter() => ModeCount <= 1 ? 0 : 20;
		public override bool GetData(SkewerAmmoOption mode) => default;
		public override bool GetCursorAreaTexture(SkewerAmmoOption mode, out Texture2D texture, out Rectangle? frame, out Color color) {
			texture = default;
			frame = default;
			color = default;
			return false;
		}
		public override void Click(SkewerAmmoOption option) {
			Player player = Main.LocalPlayer;
			if (player.HeldItem?.ModItem is AMRSL_Skewer skewer) {
				bool consumeAny = true;
				float ammoPerItem = option.AmmoPerItem;
				if (option.item.ammo != AmmoID.None && !option.item.consumable) {
					consumeAny = false;
					ammoPerItem = 1;
				}
				int neededAmmo = skewer.ammoMax - skewer.ammoCount;
				if (neededAmmo <= 0) return;
				if (RightClicked) neededAmmo = 1;

				int consume = (int)float.Ceiling(neededAmmo / ammoPerItem);
				if (consumeAny) {
					Min(ref consume, (int)(float.Floor(option.item.stack * ammoPerItem) / ammoPerItem));
					Max(ref consume, 1);
					option.item.stack -= consume;
				}
				int newAmmo = Main.rand.RandomRound(consume * ammoPerItem);
				skewer.ammoCount += newAmmo;
				skewer.ammoTypes[option.item.type] += newAmmo;

				player.altFunctionUse = 2;
				player.ApplyItemAnimation(player.HeldItem);
				player.itemAnimation++;
			}
		}
		public override IEnumerable<SkewerAmmoOption> GetModes() {
			Item[] inventory = Main.LocalPlayer.inventory;
			for (int i = 0; i < inventory.Length; i++) {
				Item item = inventory[i];
				if (item.IsAir) continue;
				if (item.favorited) continue;
				if (AMRSL_Skewer.AmmoCount[item.type] * item.stack < 1) continue;
				yield return inventory[i];
			}
		}
	}
	public readonly struct SkewerAmmoOption(Item item) : IFlowerMenuItem<bool> {
		public readonly Item item = item;
		public void Draw(Vector2 position, bool hovered, bool extraData) {
			Main.instance.LoadItem(item.type);
			Texture2D texture = TextureAssets.Item[item.type].Value;
			float scale = GetScale(out Rectangle frame);
			Main.spriteBatch.Draw(
				texture,
				position,
				frame,
				Color.White * (hovered ? 1 : 0.5f),
				0,
				frame.Size() * 0.5f,
				scale,
				SpriteEffects.None,
			0);
			string displayText = AmmoCountText;
			ChatManager.DrawColorCodedString(
				Main.spriteBatch,
				FontAssets.ItemStack.Value,
				displayText,
				position + frame.Size() * 0.5f * scale,
				Color.White,
				0f,
				FontAssets.ItemStack.Value.MeasureString(displayText) * 0.5f,
				Vector2.One,
				-1f
			);
			if (hovered) UICommon.TooltipMouseText($"{item.Name} ({item.stack})\nx{AmmoPerItem:0.##}");
		}
		float GetScale(out Rectangle frame) {
			Main.GetItemDrawFrame(item.type, out _, out frame);
			float scale = 1.15f;
			Min(ref scale, frame.Width / 20f);
			Min(ref scale, frame.Height / 20f);
			return scale;
		}
		public float AmmoPerItem => AMRSL_Skewer.AmmoCount[item.type];
		public string AmmoCountText {
			get {
				if (item.ammo != AmmoID.None && !item.consumable) return "\u221E";
				return ((int)(AMRSL_Skewer.AmmoCount[item.type] * item.stack)).ToString();
			}
		}
		public bool IsHovered(Vector2 position) => Main.MouseScreen.IsWithinRectangular(position, TextureAssets.Item[item.type].Size() * 0.5f * GetScale(out _));
		public static implicit operator SkewerAmmoOption(Item item) => new(item);
		public override int GetHashCode() => item.GetHashCode();
		public override bool Equals([NotNullWhen(true)] object obj) => obj is SkewerAmmoOption other && other.item.Equals(item);
		public static bool operator ==(SkewerAmmoOption left, SkewerAmmoOption right) => left.Equals(right);
		public static bool operator !=(SkewerAmmoOption left, SkewerAmmoOption right) => !(left == right);
	}
	public class AMRSL_Skewer_Sabot : ModProjectile {
		public override string Texture => typeof(AMRSL_Skewer_Sabot).GetDefaultTMLName();
		static AutoLoadingAsset<Texture2D> moltenTexture = typeof(AMRSL_Skewer_Sabot).GetDefaultTMLName("_Molten");
		public virtual int DustType => DustID.Iron;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ExplosiveBullet);
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.aiStyle = 0;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.friendly = true;
			Projectile.penetrate = 7;
			Projectile.timeLeft = 900;
			Projectile.alpha = 0;
			Projectile.hide = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.extraUpdates = 3;
		}
		public override void AI() {
			if (Projectile.ai[0] != 0) {
				if (Projectile.ai[1] < 20) {
					Projectile.ai[1] += Projectile.ai[0];
				} else {
					Projectile.ai[1] = 20;
				}
				Projectile.velocity = Vector2.Zero;
				Projectile.ai[2] += 1f / 180;
				if (Projectile.ai[2] >= 2) Projectile.Kill();
			} else {
				Projectile.rotation = Projectile.velocity.ToRotation();
				Projectile.velocity.Y += 0.01f;
			}
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}
		public override bool? CanDamage() {
			if (Projectile.ai[0] != 0) return false;
			return base.CanDamage();
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.ai[0] == 0) {
				Projectile.ai[0] = oldVelocity.Length() * 0.95f;
			}
			return false;
		}
		public override void OnKill(int timeLeft) {
			Vector2 unitBack = Projectile.rotation.ToRotationVector2();
			Vector2 unitSide = new(unitBack.Y, -unitBack.X);
			for (int i = -3; i < 2; i++) {
				Dust dust = Dust.NewDustPerfect(
					Projectile.Center - unitBack * 10 * (i + Main.rand.NextFloatDirection() * 0.333f) + unitSide * 3 * Main.rand.NextFloatDirection(),
					DustType
				);
				dust.velocity *= 0.25f;
				dust.velocity += Projectile.velocity;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			DrawData data = new(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation,
				new Vector2(25 - Projectile.ai[1], 3),
				Projectile.scale,
				SpriteEffects.None
			);
			Main.EntitySpriteDraw(data);
			float alpha = 1 - Projectile.ai[2];
			if (alpha < 0) return false;
			data.texture = moltenTexture;
			data.color = Color.White * alpha;
			data.color.A = 0;
			Main.EntitySpriteDraw(data);
			return false;
		}
	}
	public class AMRSL_Skewer_Sabot_Scrap : AMRSL_Skewer_Sabot {
		public override void SetStaticDefaults() {
			AMRSL_Skewer.AmmoProjectile[ModContent.ItemType<Scrap>()] = Type;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.penetrate = 1;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OriginGlobalNPC.InflictImpedingShrapnel(target, 300);
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.velocity = oldVelocity;
			return true;
		}
	}
	public readonly struct SkewerAmmoList() : IEnumerable<(int type, float count)> {
		readonly List<(int type, float count)> innerList = [];
		public float this[int type] {
			get {
				int index = GetIndex(type);
				if (index < 0) return 0;
				return innerList[index].count;
			}
			set {
				int index = GetIndex(type);
				if (value <= 0) {
					if (index >= 0) innerList.RemoveAt(index);
					return;
				}
				if (index >= 0) {
					innerList[index] = (type, value);
				} else {
					innerList.Add((type, value));
				}
			}
		}
		int GetIndex(int type) {
			for (int i = 0; i < innerList.Count; i++) {
				if (innerList[i].type == type) return i;
			}
			return -1;
		}
		public void Multiply(float multiplier) {
			if (multiplier <= 0 || float.IsSubnormal(multiplier)) {
				innerList.Clear();
				return;
			}
			for (int i = innerList.Count - 1; i >= 0; i--) {
				(int type, float value) = innerList[i];
				innerList[i] = (type, value * multiplier);
			}
		}
		public IEnumerator<(int type, float count)> GetEnumerator() => ((IEnumerable<(int type, float count)>)innerList).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)innerList).GetEnumerator();
	}
}
