using Sandbox;
using Sandbox.UI;
using PTR.UI;

namespace PTR 
{
	partial class PTRPlayer 
	{
		public ShoutText ShoutText;

		public TimeSince TimeSincePanelCreated;

		[ClientRpc]
		public void Shout() 
		{
			TimeSincePanelCreated = 0;
			ShoutText = new ShoutText();
			var transform = new Transform(Position + Vector3.Up * 64 + Vector3.Right * 10);
			ShoutText.Transform = transform;
		}

		public override void FrameSimulate( Client cl )
		{
			base.FrameSimulate( cl );

			if (ShoutText == null)
				return;

			var transform = new Transform(Position + Vector3.Up * 58 + Vector3.Right * 10);

			ShoutText.Transform = transform;
			ShoutText.Rotation = Rotation;

			if (TimeSincePanelCreated >= 5f) 
			{
				ShoutText.Delete();
				ShoutText = null;
			}
		}
	}
}
