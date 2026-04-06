using GDI_Point = Zaimoni.Data.Vector2D<int>;

namespace djack.RogueSurvivor.UI
{
    public abstract class Overlay
    {
        public readonly GDI_Point ScreenPosition;

        protected Overlay(GDI_Point screenPosition) { ScreenPosition = screenPosition; }

        public abstract void Draw();
    }
}
