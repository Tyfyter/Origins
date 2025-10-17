using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Projectiles;
using Origins.Tiles.Defiled;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class Monolith_Rod : ModItem {
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			Item.staff[Type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 100;
			Item.DamageType = DamageClass.Summon;
			Item.crit = -4;
			Item.mana = 12;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item44;
			Item.shoot = ModContent.ProjectileType<Minions.Friendly_Monolith>();
			Item.shootSpeed = 1;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.sentry = true;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			for (int i = tooltips.Count - 1; i >= 0; i--) {
				switch (tooltips[i].Name) {
					case "Damage":
					if (int.TryParse(tooltips[i].Text.Split(' ')[0], out int strength)) {
						tooltips[i] = new TooltipLine(Mod, "Strength", this.GetLocalization("StrengthTooltip").Format(strength * 0.01f));
					}
					int realCrit = Main.LocalPlayer.GetWeaponCrit(Item);
					if (realCrit > 0) tooltips.Insert(i + 1, new TooltipLine(Mod, "CritChance", this.GetLocalization("CritTooltip").Format(realCrit / 100f)));
					break;
					case "PrefixDamage":
					tooltips[i] = new TooltipLine(Mod, "Strength", this.GetLocalization("StrengthPrefixTooltip").Format(tooltips[i].Text.Split(' ')[0])) {
						IsModifier = true,
						IsModifierBad = tooltips[i].IsModifierBad
					};
					break;
					case "Knockback":
					tooltips.RemoveAt(i);
					break;
				}
			}
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse != 2) {
				Projectile.NewProjectile(source, Main.MouseWorld, default, type, Item.damage, Item.knockBack, player.whoAmI);
				player.UpdateMaxTurrets();
			}
			return false;
		}
	}
}

namespace Origins.Items.Weapons.Summoner.Minions {
	public class Friendly_Monolith : ModProjectile {
		static readonly AutoLoadingAsset<Texture2D> glowTexture = typeof(Friendly_Monolith).GetDefaultTMLName() + "_Glow";
		public override void SetStaticDefaults() {
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 10;
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
		}

		public override void SetDefaults() {
			//Projectile.CloneDefaults(ProjectileID.FrostHydra);
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 32;
			Projectile.height = 58;
			Projectile.tileCollide = true;
			Projectile.friendly = false;
			Projectile.sentry = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = Projectile.SentryLifeTime;
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];

			#region General behavior
			Vector2 idlePosition = player.Bottom;

			// die if distance is too big
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f) {
				Projectile.Kill();
				return;
			}
			Projectile.velocity *= 0.7f;
			Projectile.velocity.Y = MathF.Sin(Projectile.timeLeft * 0.02f) * 0.3f;
			foreach (Projectile other in Main.ActiveProjectiles) {
				if (other.type == Type && other.owner == Projectile.owner && other.Hitbox.Intersects(Projectile.Hitbox)) {
					Projectile.velocity += (Projectile.position - other.position).SafeNormalize(default) * 0.1f;
				}
			}
			#endregion

			#region Find target
			int signalType = ModContent.ProjectileType<Friendly_Relay_Signal>();
			if (Projectile.ai[2] != -1) {
				if (Projectile.GetRelatedProjectile_Depreciated(2) is not Projectile proj || !(proj.active && proj.type == signalType)) {
					Projectile.ai[2] = -1;
				}
			}
			if (Projectile.ai[0] == 0) {
				int? target = null;
				float priority = 0;
				foreach (Projectile other in Main.ActiveProjectiles) {
					TryTargetMinion(other, ref target, ref priority);
				}
				foreach (Player other in Main.ActivePlayers) {
					TryTargetPlayer(other, ref target, ref priority);
				}
				if (target.HasValue) {
					Projectile.ai[0] = 1;
					Projectile.ai[1] = target.Value;
					Projectile.netUpdate = true;
				}
			} else if (Projectile.ai[0] > 0 || Projectile.ai[2] == -1) {
				Projectile.ai[0]++;
			}
			#endregion

			#region Animation and visuals
			if (Projectile.ai[0] > 0) {
				Projectile.frame = (int)(Projectile.ai[0] / 5);
				if (Projectile.frame >= Main.projFrames[Type]) {
					Projectile.frame = 0;
					Projectile.ai[0] = -40;
				}
				if (Projectile.ai[2] == -1 && Projectile.frame == 8 && Projectile.owner == Main.myPlayer) {
					Projectile.ai[2] = Projectile.NewProjectileDirect(
						Projectile.GetSource_FromAI(),
						Projectile.Center,
						Projectile.DirectionTo(GetTargetEntity((int)Projectile.ai[1]).Center),
						signalType,
						Projectile.damage,
						0,
						ai0: Projectile.ai[1],
						ai1: Projectile.identity
					).identity;
					Projectile.netUpdate = true;
				}
			} else {
				if (++Projectile.frameCounter >= 5) {
					Projectile.frameCounter = 0;
					if (++Projectile.frame >= 6) Projectile.frame = 0;
				}
			}

			// Some visuals here
			float factor = Defiled_Relay.GetGlowValue(Projectile.frame);
			Lighting.AddLight(Projectile.Center, factor, factor, factor);
			#endregion
		}
		public void TryTargetMinion(Projectile newTarget, ref int? target, ref float priority) {
			Player player = Main.player[Projectile.owner];
			if (newTarget.type == Type) return;
			if (!newTarget.minion && !newTarget.sentry) return;
			if ((player.team ^ Main.player[newTarget.owner].team) != 0) return;

			float newPriority = newTarget.owner == Projectile.owner ? 2 : 1;
			newPriority *= OriginsSets.Projectiles.MinionBuffReceiverPriority[newTarget.type];
			if (newPriority == 0) return;

			int currentStrength = newTarget.TryGetGlobalProjectile(out MinionGlobalProjectile minionGlobal) ? minionGlobal.relayRodStrength : int.MaxValue;
			if (currentStrength > Projectile.damage) return;
			if (currentStrength == Projectile.damage) newPriority *= 0.5f;

			if (priority < newPriority) {
				target = newTarget.identity;
				priority = newPriority;
			}
		}
		public void TryTargetPlayer(Player newTarget, ref int? target, ref float priority) {
			Player player = Main.player[Projectile.owner];
			if ((player.team ^ newTarget.team) != 0) return;

			float newPriority = newTarget.whoAmI == Projectile.owner ? 2 : 1;
			int currentStrength = newTarget.OriginPlayer().relayRodStrength;
			if (currentStrength > Projectile.damage) return;
			if (currentStrength == Projectile.damage) newPriority *= 0.5f;

			if (priority < newPriority) {
				target = (-1) - newTarget.whoAmI;
				priority = newPriority;
			}
		}
		public static Entity GetTargetEntity(int index) {
			if (index >= 0) {
				return Main.projectile.FirstOrDefault(x => x.identity == index);
			} else {
				return Main.player[(-1) - index];
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D baseTexture = TextureAssets.Projectile[Type].Value;
			Vector2 basePosition = Projectile.position - Main.screenPosition + new Vector2(0, 2);
			Rectangle baseFrame = baseTexture.Frame(verticalFrames: Main.projFrames[Projectile.type], frameY: Projectile.frame);
			SpriteEffects baseEffects = Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(
				baseTexture,
				basePosition,
				baseFrame,
				lightColor,
				0,
				default,
				Projectile.scale,
				baseEffects
			);
			Main.EntitySpriteDraw(
				glowTexture,
				basePosition,
				baseFrame,
				Color.White,
				0,
				default,
				Projectile.scale,
				baseEffects
			);
			return false;
		}
	}
	public class Friendly_Relay_Signal : ModProjectile {
		public override string Texture => "Origins/Projectiles/Pixel";
		public override void SetDefaults() {
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 600;
			Projectile.extraUpdates = 0;
			Projectile.width = Projectile.height = 2;
			Projectile.penetrate = -1;
			Projectile.light = 0;
			Projectile.tileCollide = false;
			Projectile.hide = true;
		}
		bool isCritical = false;
		public override void OnSpawn(IEntitySource source) {
			isCritical = Main.rand.NextFloat(100) < Projectile.CritChance;
		}
		public override void AI() {
			int x = (int)(Projectile.position.X / 16);
			int y = (int)(Projectile.position.Y / 16);
			WorldGen.TileFrame(x, y, true);
			Framing.WallFrame(x, y, true);
			Lighting.AddLight(Projectile.Center, 0.45f, 0.45f, 0.45f);
			Dust dust = Dust.NewDustDirect(Projectile.position, -0, 0, DustID.AncientLight, 0, 0, 125, new Color(80, 80, 80));
			dust.noGravity = true;
			if (isCritical) {
				dust.fadeIn = 1.5f;
				Projectile.extraUpdates = 1;
			} else {
				Projectile.extraUpdates = 0;
			}

			bool returning = Projectile.ai[2] != 0;
			Entity targetEntity = Friendly_Monolith.GetTargetEntity((int)Projectile.ai[0]);
			Entity moveTarget = returning ? Main.projectile.FirstOrDefault(x => x.identity == Projectile.ai[1]) : targetEntity;

			if (!moveTarget.active) Projectile.Kill();
			if (!moveTarget.active) Projectile.Kill();
			if (Projectile.Hitbox.Intersects(moveTarget.Hitbox)) {
				if (returning) {
					ApplyBuff(targetEntity);
					Projectile.Kill();
				} else {
					Projectile.ai[2] = 1;
				}
			}
			Projectile.velocity = (moveTarget.Center - Projectile.Center).WithMaxLength(12);
		}
		void ApplyBuff(Entity entity) {
			const int duration = 300;
			int strength = Projectile.damage;
			if (isCritical) strength *= 2;
			if (entity is Player targetPlayer) {
				targetPlayer.OriginPlayer().relayRodStrength = strength;
				targetPlayer.AddBuff(ModContent.BuffType<Relay_Rod_Buff>(), duration);
			} else if (entity is Projectile targetMinion && targetMinion.TryGetGlobalProjectile(out MinionGlobalProjectile minionGlobal)) {
				minionGlobal.relayRodStrength = strength;
				minionGlobal.relayRodTime = duration;
				if (targetMinion.ModProjectile is IArtifactMinion artifactMinion && artifactMinion.Life < artifactMinion.MaxLife) {
					float oldHealth = artifactMinion.Life;
					// heal 10% + damage bonuses
					artifactMinion.Life += strength * 0.01f * 0.1f * artifactMinion.MaxLife;
					if (artifactMinion.Life > artifactMinion.MaxLife) artifactMinion.Life = artifactMinion.MaxLife;
					CombatText.NewText(entity.Hitbox, CombatText.HealLife, (int)Math.Round(artifactMinion.Life - oldHealth), true, dot: true);
				}
			}
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(isCritical);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			isCritical = reader.ReadBoolean();
		}
	}
	public class Relay_Rod_Buff : ModBuff {
		public override string Texture => "Origins/Buffs/Relay_Rod_Buff";
		public override void Update(Player player, ref int buffIndex) {
			float strength = player.OriginPlayer().relayRodStrength * 0.01f;
			player.GetDamage<No_Summon_Inherit>() += 0.1f * strength;
			player.GetAttackSpeed(DamageClass.Generic) += 0.1f * strength;
			player.lifeRegenCount += Main.rand.RandomRound(6 * strength);
		}
	}
	public class FloatCurve(float minY, float maxY, float min = 0, float max = 1) : Curve<float>(min, max) {
		public override float Interpolate(float a, float b, float progress) => Utils.Remap(progress, 0, 1, a, b);
		public override void Draw(Rectangle bounds) {
			Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, bounds, Color.Black);
			Rectangle area = bounds;
			area.Width = 1;
			for (int i = 0; i < bounds.Width; i++) {
				area.X = bounds.X + i;
				float position = Utils.Remap(i / (float)bounds.Width, 0, 1, Min, Max);
				float value = this[position];
				area.Height = (int)Utils.Remap(value, minY, maxY, 0, bounds.Height);
				area.Y = bounds.Bottom - area.Height;
				Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, area, Color.White);
			}
		}
		public override string NodeToString(CurveNode node) => $"{{x:{node.x}, y:{node.y}}}";
	}
	public abstract class Curve<T>(float min = 0, float max = 1) {
		public readonly List<CurveNode> nodes = [];
		public float Min { get; } = min;
		public float Max { get; } = max;
		public T this[float position] {
			get {
				CurveNode prevNode = default;
				CurveNode currentNode = default;
				for (int i = 0; i < nodes.Count; i++) {
					if (nodes[i].x == position) return nodes[i].y;
					prevNode = currentNode;
					currentNode = nodes[i];
					if (currentNode.x > position) break;
				}
				if (currentNode == null) return default;
				//if (position < currentNode.x) return currentNode.y;
				return Interpolate(prevNode.y, currentNode.y, currentNode.GetProgress(position, prevNode.x, currentNode.x));
			}
		}
		public abstract void Draw(Rectangle bounds);
		public void Add(CurveNode node) {
			nodes.InsertOrdered(node);
		}
		public abstract T Interpolate(T a, T b, float progress);
		public abstract string NodeToString(CurveNode node);
		public class LinearNode : CurveNode {
			public override float GetProgress(float x, float aX, float bX) => Utils.Remap(x, aX, bX, 0, 1);
		}
		public abstract class CurveNode : IComparable<CurveNode> {
			public bool Locked { get; init; } = false;
			public float x;
			public T y;
			public abstract float GetProgress(float aX, float bX, float x);
			public int CompareTo(CurveNode other) => x.CompareTo(other.x);
		}
	}
}
