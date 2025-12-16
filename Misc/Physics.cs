using Microsoft.Xna.Framework;
using System;
using System.Drawing;
using Terraria;

namespace Origins.Misc {
	public static class Physics {
		public abstract class Gravity {
			public abstract Vector2 Acceleration { get; }
			public static ConstantGravity NormalGravity => new(new Vector2(0, 0.06f));
		}
		public class ConstantGravity(Vector2 acceleration) : Gravity {
			public Vector2 acceleration = acceleration;
			public override Vector2 Acceleration => acceleration;
		}
		public class EntityDirectionGravity(Vector2 acceleration, Entity entity) : Gravity {
			public Vector2 acceleration = acceleration;
			public Entity entity = entity;
			public override Vector2 Acceleration => acceleration * new Vector2(entity.direction, 1);
		}
		public abstract class AnchorPoint {
			public abstract Vector2 WorldPosition { get; }
		}
		public class WorldAnchorPoint(Vector2 position) : AnchorPoint {
			public override Vector2 WorldPosition { get; } = position;
		}
		public class EntityAnchorPoint(Entity entity = null, Vector2 offset = default) : AnchorPoint {
			public override Vector2 WorldPosition {
				get => entity.Center + offset * new Vector2(entity.direction, 1);
			}
			public Entity entity = entity;
			public Vector2 offset = offset;
		}
		public class NPCAnchorPoint : AnchorPoint {
			public override Vector2 WorldPosition {
				get => npc.Center + (offset * new Vector2(npc.direction, 1)).RotatedBy(npc.rotation);
			}
			public NPC npc;
			public Vector2 offset;
		}
		public class Chain {
			public AnchorPoint anchor;
			public Link[] links;
			public Vector2[] Update() {
				Vector2[] delta = new Vector2[links.Length];
				Vector2 lastPosition = anchor.WorldPosition;
				bool[] linksCanStretch = new bool[links.Length + 1];
				void UpdateLink(Link currentLink, int i) {
					Vector2 oldVelocity = currentLink.velocity;
					Vector2 diff = currentLink.position - lastPosition;
					float distSQ = diff.LengthSquared();
					if (distSQ > currentLink.length * currentLink.length) {
						float dist = MathF.Sqrt(distSQ);
						Vector2 direction = diff.SafeNormalize(default);
						float distDiff = dist - currentLink.length;
						if (linksCanStretch[i]) {
							currentLink.velocity -= direction * distDiff * 0.5f;
							currentLink.velocity *= 0.95f;
							float linkVelLenSQ = currentLink.velocity.LengthSquared();
							const float maxSegVelLen = 24;
							if (linkVelLenSQ > maxSegVelLen) {
								Vector2 newVel = currentLink.velocity.WithMaxLength(maxSegVelLen);
								currentLink.position += currentLink.velocity - newVel;
								currentLink.velocity = newVel;
							}
							Vector2 lastChainMovement = direction * distDiff * 0.25f;
							links[i - 1].position += lastChainMovement * 2;
							links[i - 1].velocity += lastChainMovement * 2;
							if (i > 0) UpdateLink(links[i - 1], i - 1);
						} else {
							currentLink.velocity -= direction * distDiff * 0.5f;
							currentLink.velocity *= currentLink.spring;
						}
						linksCanStretch[i + 1] = true;
					} else {
						linksCanStretch[i + 1] = true;
					}
					delta[i] += currentLink.velocity - oldVelocity;
					currentLink.position += currentLink.velocity;
				}
				for (int i = 0; i < links.Length; i++) {
					Link currentLink = links[i];
					currentLink.velocity *= currentLink.drag;
					for (int j = 0; j < currentLink.gravity.Length; j++) {
						currentLink.velocity += currentLink.gravity[j].Acceleration;
					}
					UpdateLink(currentLink, i);
					lastPosition = currentLink.position;
				}
				return delta;
			}
			public Vector2[] UpdateWithCollision() {
				Vector2[] delta = new Vector2[links.Length];
				Vector2 lastPosition = anchor.WorldPosition;
				bool[] linksCanStretch = new bool[links.Length + 1];
				void UpdateLink(Link currentLink, int i) {
					Vector2 oldVelocity = currentLink.velocity;
					Vector2 diff = currentLink.position - lastPosition;
					float distSQ = diff.LengthSquared();
					currentLink.velocity = Collision.AnyCollision(currentLink.position - new Vector2(2), currentLink.velocity, 4, 4);
					if (distSQ > currentLink.length * currentLink.length) {
						float dist = MathF.Sqrt(distSQ);
						Vector2 direction = diff.SafeNormalize(default);
						float distDiff = dist - currentLink.length;
						if (linksCanStretch[i]) {
							currentLink.velocity -= direction * distDiff * 0.5f;
							currentLink.velocity *= 0.95f;
							float linkVelLenSQ = currentLink.velocity.LengthSquared();
							const float maxSegVelLen = 24;
							if (linkVelLenSQ > maxSegVelLen) {
								Vector2 newVel = currentLink.velocity.WithMaxLength(maxSegVelLen);
								currentLink.position += currentLink.velocity - newVel;
								currentLink.velocity = newVel;
							}
							Vector2 lastChainTryMovement = direction * distDiff * 0.5f;
							Link lastLink = links[i - 1];
							Vector2 lastChainMovement;// = lastLink.size == 0 ? lastChainTryMovement : Collision.AnyCollision(lastLink.position - new Vector2(lastLink.size * 0.5f), lastChainTryMovement, lastLink.size, lastLink.size);
							if (lastLink.size == 0) {
								lastChainMovement = lastChainTryMovement;
							} else {
								Vector2 halfSize = new(lastLink.size * 0.5f);
								Vector4 slopeCollision = Collision.SlopeCollision(links[i - 1].position - halfSize, lastChainTryMovement, lastLink.size, lastLink.size);
								links[i - 1].position = slopeCollision.XY() + halfSize;
								lastChainMovement = slopeCollision.ZW();
								lastChainMovement = Collision.TileCollision(links[i - 1].position - halfSize, lastChainMovement, lastLink.size, lastLink.size);
							}
							links[i - 1].position += lastChainMovement;
							links[i - 1].velocity += lastChainMovement;
							currentLink.position += lastChainTryMovement - lastChainMovement;
							currentLink.velocity += lastChainTryMovement - lastChainMovement;
							if (i > 0) UpdateLink(links[i - 1], i - 1);
						} else {
							currentLink.velocity -= direction * distDiff * 0.5f;
							currentLink.velocity *= currentLink.spring;
						}
						linksCanStretch[i + 1] = true;
					} else {
						linksCanStretch[i + 1] = true;
					}
					delta[i] += currentLink.velocity - oldVelocity;
					currentLink.position += currentLink.velocity;
				}
				for (int i = 0; i < links.Length; i++) {
					Link currentLink = links[i];
					currentLink.velocity *= currentLink.drag;
					for (int j = 0; j < currentLink.gravity.Length; j++) {
						currentLink.velocity += currentLink.gravity[j].Acceleration;
					}
					UpdateLink(currentLink, i);
					lastPosition = currentLink.position;
				}
				return delta;
			}
			public class Link(Vector2 position, Vector2 velocity, float length, Physics.Gravity[] gravity = null, float drag = 0.97f, float spring = 0.95f) {
				public Vector2 position = position;
				public Vector2 velocity = velocity;
				public Gravity[] gravity = gravity ?? [Gravity.NormalGravity];
				public float length = length;
				public float drag = drag;
				public float spring = spring;
				public int size = 4;
			}
		}
	}
}
