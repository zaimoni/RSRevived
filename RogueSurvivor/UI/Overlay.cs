using GDI_Point = Zaimoni.Data.Vector2D<int>;

namespace djack.RogueSurvivor.UI
{
    public abstract class Overlay
    {
        public readonly GDI_Point ScreenPosition;

        protected Overlay(GDI_Point screenPosition) { ScreenPosition = screenPosition; }

        public abstract void Draw();
    }

    public sealed class OverlayImage : Overlay
    {
        public readonly string ImageID;

        public OverlayImage(GDI_Point screenPosition, string imageID) : base(screenPosition)
        {
            ImageID = imageID;
        }

        public override void Draw()
        {
            var ui = IRogueUI.UI;
            ui.UI_DrawImage(ImageID, ScreenPosition.X, ScreenPosition.Y);
        }
    }

    public sealed class OverlayTransparentImage : Overlay
    {
        public readonly float Alpha;
        public readonly string ImageID;

        public OverlayTransparentImage(float alpha, GDI_Point screenPosition, string imageID) : base(screenPosition)
        {
            Alpha = alpha;
            ImageID = imageID;
        }

        public override void Draw()
        {
            var ui = IRogueUI.UI;
            ui.UI_DrawTransparentImage(Alpha, ImageID, ScreenPosition.X, ScreenPosition.Y);
        }
    }
}
