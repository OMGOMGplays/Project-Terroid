using Sandbox;

namespace PTR 
{
	public class PTRCamera : CameraMode 
	{
		public override void Update()
		{
			var pawn = Local.Pawn as PTRPlayer;
			var client = Local.Client;

			if (pawn == null)
				return;

			Position = pawn.Position + Vector3.Up * 50;
			Vector3 targetPos;

			var center = pawn.Position + Vector3.Up * 360;

			Position = center += (Vector3.Backward * 100 + Vector3.Right * 100);
			Rotation = Rotation.FromAxis(Vector3.Up, 40);
			Rotation += Rotation.FromPitch(40);
			Rotation -= Rotation.FromRoll(12);

			float distance = 100.0f * pawn.Scale;
			targetPos = Position + ((pawn.CollisionBounds.Maxs.x + 15) * pawn.Scale);
			targetPos -= distance;

			Position = targetPos;

			FieldOfView = 70;
			Viewer = null;
		}
	}
}
