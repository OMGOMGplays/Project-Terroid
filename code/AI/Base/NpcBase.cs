using Sandbox;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PTR 
{
	[Library("npc_base", Title = "Base NPC")]
	public partial class NpcBase : AnimEntity 
	{
		public float Speed = 30;

		NavPath Path = new NavPath();
		public NavSteer Steer;

		public Entity target;

		public CurrentState CurrentState = CurrentState.Wander;

		private DamageInfo LastDamage;

		public Vector3 InputVelocity;

		public float TimeUntilAttack = 0;

		public override void Spawn() 
		{
			base.Spawn();

			SetModel("models/citizen/citizen.vmdl");
			RenderColor = Color.Green;
			EyePosition = Position + Vector3.Up * 64;
			CollisionGroup = CollisionGroup.Player;
			SetupPhysicsFromCapsule(PhysicsMotionType.Keyframed, Capsule.FromHeightAndRadius(72, 8));

			EnableHitboxes = true;

			StartWander();
		}

		[Event.Tick.Server]
		public virtual void Tick()
		{
			InputVelocity = 0;

			if ( Steer != null )
			{
				//using var _b = Sandbox.Debug.Profile.Scope( "Steer" );

				Steer.Tick( Position );

				if ( !Steer.Output.Finished )
				{
					InputVelocity = Steer.Output.Direction.Normal;
					Velocity = Velocity.AddClamped( InputVelocity * Time.Delta * 500, Speed );

					// climbing stuff			
					var start = Position;
					Vector3 end = start + (Velocity.Normal) * 20;
					//end += new Vector3(0, 0, -10);

					var tr = Sandbox.Trace.Ray( start, end )
							.Size( 4, 8 )
							.HitLayer( CollisionLayer.All, false )
							.HitLayer( CollisionLayer.STATIC_LEVEL, true )
							//.HitLayer(CollisionLayer.LADDER, true)
							.Ignore( Owner )
							.Ignore( this )
							.Run();

					if ( tr.Hit )
					{
						//if (tr.Entity.IsValid())
						{
							//Velocity += new Vector3(0, 0, 20);
							Position += new Vector3( 0, 0, 10 );
						}
					}
				}
			}
		}

		public override void TakeDamage( DamageInfo info )
		{
			base.TakeDamage( info );

			target = info.Attacker;
			if (CurrentState == CurrentState.Wander) 
			{
				StartChase(target);
			}
			else 
			{
				if (Rand.Int(5) == 1)
					target = info.Attacker;
			}

			var AngerRange = 250;
			var overlaps = BasePhysics.FindInSphere(Position, AngerRange);

			foreach (var overlap in overlaps.OfType<NpcBase>().ToArray()) 
			{
				if (Rand.Int(5) == 1)
					overlap.StartChase(target);
			}

			Velocity /= 10;
		}

		public virtual IEnumerable<TraceResult> TraceBullet(Vector3 start, Vector3 end, float radius = 2.0f) 
		{
			var tr = Sandbox.Trace.Ray(start, end)
					.UseHitboxes()
					.Ignore(Owner)
					.Ignore(this)
					.Size(radius)
					.Run();

			yield return tr;
		}

		public void MeleeStrike(float damage, float force) 
		{
			SetAnimParameter("holdtype", 4);

			if (Rand.Int(1) == 1)
				SetAnimParameter("holdtype_attack", 2.0f);
			else
				SetAnimParameter("holdtype_attack", 1.0f);

			if (Rand.Int(1) == 1)
				SetAnimParameter("holdtype_handedness", 1);
			else if (Rand.Int(2) == 1)
				SetAnimParameter("holdtype_handedness", 2);
			else
				SetAnimParameter("holdtype_handedness", 0);

			PlaySound("zombie.attack");
			SetAnimParameter("b_attack", true);
			Velocity = 0;
			var forward = EyeRotation.Forward;
			forward = forward.Normal;

			var overlaps = BasePhysics.FindInSphere(Position, 80);

			foreach (var overlap in overlaps.OfType<PTRPlayer>().ToArray()) 
			{
				if (!IsServer) continue;
				using (Prediction.Off()) 
				{
					var damageInfo = DamageInfo.FromBullet(overlap.Position, forward * 100 * force, damage)
						.WithAttacker(Owner);
					overlap.TakeDamage(damageInfo);

					foreach (var tr in TraceBullet(EyePosition, EyePosition, 100f)) 
					{
						tr.Surface.DoBulletImpact(tr);
					}
				}
			}
		}

		public void StartWander() 
		{
			CurrentState = CurrentState.Wander;
			
			var wander = new Wander();
			wander.MinRadius = 50;
			wander.MaxRadius = 120;
			Steer = wander;
		}

		public void StartChase() 
		{
			if (CurrentState == CurrentState.Chase)
				return;

			SetAnimParameter("b_jump", true);
			PlaySound("zombie.found");

			CurrentState = CurrentState.Chase;

			if (!target.IsValid()) FindTarget();
			if (target.Health <= 0) FindTarget();
			Steer = new NavSteer();
			Steer.Target = target.Position;

			var tr = Sandbox.Trace.Ray(Position, Position)
					.UseHitboxes()
					.Ignore(Owner)
					.Ignore(this)
					.Size(2)
					.Run();
		}

		public void StartChase(Entity targ) 
		{
			target = targ;
			if (CurrentState == CurrentState.Chase)
				return;

			SetAnimParameter("b_jump", true);

			if (!target.IsValid()) FindTarget();
			if (target.Health <= 0) FindTarget();
			Steer = new NavSteer();
			Steer.Target = target.Position;

			var tr = Sandbox.Trace.Ray(Position, Position)
					.UseHitboxes()
					.Ignore(Owner)
					.Ignore(this)
					.Size(2)
					.Run();
		}

		public void FindTarget() 
		{
			target = Entity.All
				.OfType<Player>()
				.OrderBy(x => Guid.NewGuid())
				.FirstOrDefault();

			if (target == null) 
			{
				Log.Warning($"Couldn't find target for {this}!");
			}
		}

		protected virtual void Move(float timeDelta) 
		{
			var bbox = BBox.FromHeightAndRadius(64, 4);

			MoveHelper move = new(Position, Velocity);
			move.MaxStandableAngle = 50;
			move.Trace = move.Trace.Ignore(this).Size(bbox);

			if (!Velocity.IsNearlyZero(0.001f)) 
			{
				move.TryUnstuck();

				move.TryMoveWithStep(timeDelta, 30);
			}

			var tr = move.TraceDirection(Vector3.Down * 10.0f);

			if (move.IsFloor(tr)) 
			{
				GroundEntity = tr.Entity;

				if (!tr.StartedSolid) 
				{
					move.Position = tr.EndPosition;
				}

				if (InputVelocity.Length > 0) 
				{
					var movement = move.Velocity.Dot(InputVelocity.Normal);
					move.Velocity = move.Velocity - movement * InputVelocity.Normal;
					move.ApplyFriction(tr.Surface.Friction * 10.0f, timeDelta);
					move.Velocity += movement * InputVelocity.Normal;
				}
				else 
				{
					move.ApplyFriction(tr.Surface.Friction * 10.0f, timeDelta);
				}
			}
			else 
			{
				GroundEntity = null;
				move.Velocity += Vector3.Down * 900 * timeDelta;
			}
			Position = move.Position;
			Velocity = move.Velocity;
		}
	}

	public enum CurrentState 
	{
		Wander = 0,
		Chase
	}
}
