using Sandbox;
using System;
using System.Linq;

namespace PTR
{
	partial class PTRPlayer : Player
	{

		// public override void Spawn()
		// {
		// 	base.Spawn();
		// }

		public override void Respawn()
		{
			base.Respawn();

			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new WalkController();
			Animator = new StandardPlayerAnimator();
			CameraMode = new ThirdPersonCamera();

			EnableDrawing = true;
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );
		}
	}
}
