using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace Origins.Misc {
	public static class Physics {
		public abstract class Gravity {
			public abstract Vector2 Acceleration { get; }
			public static ConstantGravity NormalGravity => new ConstantGravity(new Vector2(0, 0.06f));
		}
		public class ConstantGravity : Gravity {
			public Vector2 acceleration;
			public override Vector2 Acceleration => acceleration;
			public ConstantGravity(Vector2 acceleration) {
				this.acceleration = acceleration;
			}
		}
		public class EntityDirectionGravity : Gravity {
			public Vector2 acceleration;
			public Entity entity;
			public override Vector2 Acceleration => acceleration * new Vector2(entity.direction, 1);
			public EntityDirectionGravity(Vector2 acceleration, Entity entity) {
				this.acceleration = acceleration;
				this.entity = entity;
			}
		}
		public abstract class AnchorPoint {
			public abstract Vector2 WorldPosition { get; }
		}
		public class EntityAnchorPoint : AnchorPoint {
			public override Vector2 WorldPosition {
				get => entity.Center + offset * new Vector2(entity.direction, 1);
			}
			public Entity entity;
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
							Vector2 lastChainMovement = Collision.AnyCollision(links[i - 1].position - new Vector2(2), lastChainTryMovement, 4, 4);
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
			public class Link {
				public Vector2 position;
				public Vector2 velocity;
				public Gravity[] gravity;
				public float length;
				public float drag;
				public float spring;
				public Link(Vector2 position, Vector2 velocity, float length, Gravity[] gravity = null, float drag = 0.97f, float spring = 0.95f) {
					this.position = position;
					this.velocity = velocity;
					this.gravity = gravity ?? new Gravity[] { Gravity.NormalGravity };
					this.length = length;
					this.drag = drag;
					this.spring = spring;
				}
			}
		}
	}
}
