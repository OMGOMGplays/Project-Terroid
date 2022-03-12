using Sandbox;
using PTR.UI;
using System;
using System.Linq;

namespace PTR
{
	public partial class PTRPlayer : Player
	{
		public override void Respawn()
		{
			base.Respawn();

			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new PTRPlayerMovement();
			Animator = new PTRPlayerAnimator();
			CameraMode = new PTRCamera();

			EnableDrawing = true;
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );
		}
	}
}
