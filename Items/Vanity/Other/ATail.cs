using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Origins.Items.Vanity.Other; 
public abstract class ATail : ModItem {
	public Tail_Layer DrawInLayer { get; private set; }
	public virtual int Length => 10;
	public override ModItem NewInstance(Item entity) {
		ATail tail = (ATail)base.NewInstance(entity);
		tail.DrawInLayer = DrawInLayer;
		return tail;
	}
	public override void Load() {
		DrawInLayer = Tail_Layer.Register(Mod, new PlayerDrawLayer.AfterParent(PlayerDrawLayers.Tails));
	}
	public override void UpdateAccessory(Player player, bool hideVisual) {
		if (!hideVisual) UpdateVanity(player);
	}
	public override void UpdateVanity(Player player) {
		OriginPlayer originPlayer = player.OriginPlayer();
		originPlayer.vanityTail = this;
		UpdateTail(player, originPlayer.vanityTailSegments);
	}
	public virtual void UpdateTail(Player player, List<TailSegment> tailSegments) {
		if (tailSegments.Count > Length) tailSegments.RemoveRange(Length - 1, tailSegments.Count - Length);
		else if (tailSegments.Count < Length) tailSegments.Insert(Math.Min(1, tailSegments.Count), new() { 
			position = player.MountedCenter
		});
		for (int i = 0; i < tailSegments.Count; i++) {
			UpdateTailSegment(player, tailSegments, i);
		}
	}
	public virtual void OnKilled(Player player, List<TailSegment> tailSegments, PlayerDeathReason damageSource, double dmg, int hitDirection, bool pvp = false) {
		for (int i = 0; i < tailSegments.Count; i++) {
			tailSegments[i].velocity = new(
				Main.rand.Next(-20, 21) * 0.1f + (2 * hitDirection),
				Main.rand.Next(-40, -10) * 0.1f
			);
		}
	}
	public virtual void UpdateTailDead(Player player, List<TailSegment> tailSegments) {
		for (int i = 0; i < tailSegments.Count; i++) {
			TailSegment segment = tailSegments[i];
			segment.position += segment.velocity;
			segment.rotation += segment.velocity.X * 0.1f;
			segment.velocity.Y += 0.1f;
			segment.velocity.X *= 0.99f;
		}
	}
	public abstract void UpdateTailSegment(Player player, IReadOnlyList<TailSegment> segments, int index);
	public virtual void DrawTail(ref PlayerDrawSet drawInfo, List<TailSegment> tailSegments) {
		int dataCount = drawInfo.DrawDataCache.Count;
		for (int i = 0; i < tailSegments.Count; i++) {
			DrawTailSegment(ref drawInfo, tailSegments, i);
		}
		for (int i = dataCount; i < drawInfo.DrawDataCache.Count; i++) {
			DrawData data = drawInfo.DrawDataCache[i];
			data.rotation -= drawInfo.rotation;
			data.position += (drawInfo.Position - drawInfo.drawPlayer.position);
			data.position = data.position.RotatedBy(-drawInfo.rotation, drawInfo.Position + drawInfo.rotationOrigin - Main.screenPosition);
			drawInfo.DrawDataCache[i] = data;
		}
	}
	public abstract void DrawTailSegment(ref PlayerDrawSet drawInfo, IReadOnlyList<TailSegment> segments, int index);
	public static void DoBasicUpdate(Player player, IReadOnlyList<TailSegment> segments, int index, float parentDistance) {
		TailSegment segment = segments[index];
		if (index == 0) {
			segment.position = player.MountedCenter + player.Directions(-10, 10);
			segment.rotation = player.fullRotation + MathHelper.PiOver2 * player.direction;
			return;
		}
		TailSegment parent = segments[index - 1];
		Vector2 parentCenter = parent.position;
		float parentRotation = parent.rotation;

		Vector2 offset = parentCenter - segment.position;
		if (parentRotation != segment.rotation) {
			float angleDiff = MathHelper.WrapAngle(parentRotation - segment.rotation);
			offset = offset.RotatedBy(angleDiff * 0.1f);
		}

		segment.rotation = offset.ToRotation() + MathHelper.PiOver2;

		if (offset != Vector2.Zero)
			segment.position = parentCenter - Vector2.Normalize(offset) * parentDistance;
		segment.effects = (Math.Abs(offset.X) > Math.Abs(offset.Y) ? offset.X : player.direction) > 0f ? SpriteEffects.FlipVertically : SpriteEffects.None;
	}
}
[Autoload(false)]
public sealed class Tail_Layer : PlayerDrawLayer {
	readonly static Dictionary<Position, Tail_Layer> byPosition = new();
	readonly Position position;
	Tail_Layer(Position position) => this.position = position;
	public static Tail_Layer Register(Mod mod, Position position) {
		if (position is not AfterParent and not BeforeParent and not Between) throw new ArgumentException($"Invalid position {position}, supported position types are AfterParent, BeforeParent, and Between", nameof(position));
		if (!byPosition.TryGetValue(position, out Tail_Layer layer)) {
			mod.AddContent(layer = new Tail_Layer(position));
		}
		return layer;
	}
	public override Position GetDefaultPosition() => position;
	public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.OriginPlayer().vanityTail is ATail tail && tail.DrawInLayer == this;
	protected override void Draw(ref PlayerDrawSet drawInfo) {
		if (drawInfo.drawPlayer.OriginPlayer() is not OriginPlayer originPlayer) return;
		if (originPlayer.vanityTail is not ATail tail) return;
		tail.DrawTail(ref drawInfo, originPlayer.vanityTailSegments);
	}
	struct PositionComparer : IEqualityComparer<Position> {
		public readonly bool Equals(Position x, Position y) {
			if (x.GetType() != y.GetType()) return false;
			switch ((x, y)) {
				case (AfterParent a, AfterParent b):
				return a.Parent == b.Parent;

				case (BeforeParent a, BeforeParent b):
				return a.Parent == b.Parent;

				case (Between a, Between b):
				return a.Layer1 == b.Layer1 && a.Layer2 == b.Layer2;
			}
			return false;
		}
		public readonly int GetHashCode([DisallowNull] Position obj) {
			HashCode hash = new();
			hash.Add(obj.GetType());
			switch (obj) {
				case AfterParent position:
				hash.Add(position.Parent);
				break;
				case BeforeParent position:
				hash.Add(position.Parent);
				break;
				case Between position:
				hash.Add(position.Layer1);
				hash.Add(position.Layer2);
				break;
			}
			return hash.ToHashCode();
		}
	}
}
public class TailSegment {
	public Vector2 position;
	/// <summary>
	/// Only used when dead by default
	/// </summary>
	public Vector2 velocity;
	public SpriteEffects effects;
	public float rotation;
}