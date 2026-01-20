using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Items.Tools {
	public class Adjusto_Hook : ModItem {
		public override void SetStaticDefaults() {
			foreach (ControlSetting setting in Enum.GetValues<ControlSetting>()) {
				this.GetLocalization($"ControlTootlip{setting}", () => "");
			}
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.AmethystHook);
			Item.shootSpeed = 16f;
			Item.shoot = ProjectileType<Adjusto_Hook_P>();
			Item.value = Item.sellPrice(silver: 8);
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			for (int i = 0; i < tooltips.Count; i++) {
				if (tooltips[i].Text == "ControlsOnThisLine") {
					tooltips[i].Text = this.GetLocalization($"ControlTootlip{OriginClientConfig.Instance.adjustoHookControlSetting}").Value;
					break;
				}
			}
		}
		public enum ControlSetting {
			Default,
			Inverted,
			World
		}
	}
	public class Adjusto_Hook_P : ModProjectile {
		AutoLoadingAsset<Texture2D> chain = typeof(Adjusto_Hook_P).GetDefaultTMLName() + "_Chain";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.GemHookAmethyst);
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.netImportant = true;
		}
		public static int NodeCount => 14;
		public override float GrappleRange() => 30 * 16;
		public override void NumGrappleHooks(Player player, ref int numHooks) => numHooks = 1;
		public override void GrappleRetreatSpeed(Player player, ref float speed) => speed = 18f;
		public override void GrapplePullSpeed(Player player, ref float speed) => speed = 8f;
		Vector2[] nodes = null;
		float[] nodeWeights = null;
		Vector2[] GetNodes() {
			nodes ??= new Vector2[NodeCount + 1];
			nodeWeights ??= new float[NodeCount + 1];
			Vector2 position = Projectile.Center;
			Vector2 offset = -Projectile.ai[1].ToRotationVector2();
			float length = GrappleRange() / NodeCount;
			bool canBend = true;
			Rectangle check = new(0, 0, 10, 10);
			for (int i = 0; i < nodes.Length; i++) {
				nodes[i] = position;
				position += offset * length;
				if (i > Projectile.ai[2]) {
					nodeWeights[i] = 0;
				} else {
					if (canBend) {
						canBend = MathUtils.LinearSmoothing(ref nodeWeights[i], 1, 1f / 9);
					}
				}
				if (check.Recentered(position).OverlapsAnyTiles()) {
					MathUtils.LinearSmoothing(ref nodeWeights[i], -1, 1f / 5);
					canBend = true;
				}
				if (nodeWeights[i] != 0) {
					offset.Y += nodeWeights[i] * 0.1f;
					offset.Normalize();
				}
			}
			return nodes;
		}
		public override void GrappleTargetPoint(Player player, ref float grappleX, ref float grappleY) {
			(grappleX, grappleY) = GetNodes().GetIfInRange((int)Projectile.ai[2], new(grappleX, grappleY));
		}
		public override void AI() {
			if (Projectile.velocity != default) {
				Projectile.ai[1] = Projectile.rotation - MathHelper.PiOver2;
			} else {
				Player player = Main.player[Projectile.owner];
				if (nodes is null) {
					GetNodes();
					SetToNearestNode(player, true);
				}
				if (Projectile.IsLocallyOwned() && Projectile.localAI[0] <= 0) {
					float oldValue = Projectile.ai[2];

					Projectile.ai[2] += PickControlDirection();

					Max(ref Projectile.ai[2], Projectile.localAI[1]);
					Min(ref Projectile.ai[2], Projectile.localAI[2]);
					if (Projectile.ai[2] != oldValue) Projectile.localAI[0] = GrappleRange() / NodeCount;
					Projectile.netUpdate = true;
				}
				float speed = 11;
				ProjectileLoader.GrapplePullSpeed(Projectile, player, ref speed);
				if (Projectile.localAI[0].Cooldown(0, speed)) {
					SetToNearestNode(player);
				}
			}
			Projectile.rotation = Projectile.ai[1] + MathHelper.PiOver2;
		}
		void SetToNearestNode(Player player, bool initialize = false) {
			float distSQ = float.PositiveInfinity;
			Vector2 pos = player.MountedCenter;
			int selected = (int)Projectile.ai[2];
			for (int i = 0; i < nodes.Length; i++) {
				float dist = pos.DistanceSQ(nodes[i]);
				if (i == selected && nodes[i].Clamp(player.Hitbox).WithinRange(nodes[i], 8)) dist = 0;
				if (distSQ > dist) {
					distSQ = dist;
					Projectile.ai[2] = i;
				}
			}
			if (initialize) {
				Projectile.localAI[1] = 0;
				Projectile.localAI[2] = NodeCount;
				return;
			}
			if (Projectile.ai[2] > selected) Projectile.localAI[1] = Projectile.ai[2];
			if (Projectile.ai[2] < selected) Projectile.localAI[2] = Projectile.ai[2];
		}
		public override bool PreDrawExtras() {
			Rectangle frame = chain.Value.Frame(verticalFrames: 2, frameY: 1);
			if (nodes is null) {
				chain.Value.DrawChain(Projectile.Center, Main.player[Projectile.owner].MountedCenter, frame, 10);
				return false;
			}
			Vector2 origin = frame.Size() * 0.5f;
			origin.Y -= 1;
			for (int i = 0; i < nodes.Length - 1; i++) {
				Vector2 end = nodes[i + 1];
				bool lastLink = i >= (Projectile.ai[2] - 1);
				if (lastLink) end = Main.player[Projectile.owner].MountedCenter;
				chain.Value.DrawChain(nodes[i], end, frame, 10);
				Main.EntitySpriteDraw(
					chain,
					end - Main.screenPosition,
					frame,
					Color.White,
					0,
					origin,
					Vector2.One,
					SpriteEffects.None
				);
				if (lastLink) break;
			}
			return false;
		}
		public int PickControlDirection() {
			Player player = Main.player[Projectile.owner];
			switch (OriginClientConfig.Instance.adjustoHookControlSetting) {
				case Adjusto_Hook.ControlSetting.Default:
				default:
				return player.controlDown.ToInt() - player.controlUp.ToInt();
				case Adjusto_Hook.ControlSetting.Inverted:
				return player.controlUp.ToInt() - player.controlDown.ToInt();
				case Adjusto_Hook.ControlSetting.World: {
					Vector2 controlDirection = new(player.controlRight.ToInt() - player.controlLeft.ToInt(), (player.controlDown.ToInt() - player.controlUp.ToInt()) * player.gravDir);
					if (controlDirection == default) return 0;
					controlDirection.Normalize();
					Vector2 current = GetNodes().GetIfInRange((int)Projectile.ai[2]);
					Vector2 higher = GetNodes().GetIfInRange((int)Projectile.ai[2] + 1);
					Vector2 lower = GetNodes().GetIfInRange((int)Projectile.ai[2] - 1);
					float bestQuality = 0.01f;
					int bestDir = 0;
					if (higher != default && Maximize(ref bestQuality, Vector2.Dot(current.DirectionTo(higher), controlDirection))) {
						bestDir = 1;
					}
					if (lower != default && Maximize(ref bestQuality, Vector2.Dot(current.DirectionTo(lower), controlDirection))) {
						bestDir = -1;
					}
					return bestDir;
				}
			}
		}
	}
}
