using Sandbox;

namespace PTR 
{
	public class PTRCamera : CameraMode 
	{
		[ConVar.Replicated]
		public static bool no_collision {get; set;} = false;

		// public static PTRPlayer Pawn = Local.Pawn as PTRPlayer;

		public float CamDistance = 200.0f;

		public override void Update()
		{
			var pawn = Local.Pawn as PTRPlayer;
			var client = Local.Client;

			if (pawn == null)
				return;

			Position = pawn.Position + Vector3.Up * 50;
			Vector3 targetPos;

			// float distance = 200.0f * pawn.Scale;
			// targetPos = Position + ((pawn.CollisionBounds.Maxs.x + 15) * pawn.Scale);
			targetPos = Position + Rotation.Backward * CamDistance;

			var center = pawn.Position + Vector3.Up * 360;


			// Position = targetPos;

			var tr = Trace.Ray(Position, targetPos)
				.Ignore(pawn)
				.Radius(8)
				.Run();

			Position = tr.EndPosition;
			Rotation = Rotation.FromAxis(Vector3.Up, 40);
			Rotation += Rotation.FromPitch(40);
			Rotation -= Rotation.FromRoll(15);

			// Position = targetPos;

			// Position += Input.MouseWheel;

			CamDistance += -Input.MouseWheel * 4;
			CamDistance = CamDistance.Clamp(150, 500);

			FieldOfView = 70;
			Viewer = null;
		}
	}
}
