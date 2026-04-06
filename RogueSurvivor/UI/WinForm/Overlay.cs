using System.Drawing;

using GDI_Point = Zaimoni.Data.Vector2D<int>;

// these classes should be partial classes with the GUI-independent parts one level up

namespace djack.RogueSurvivor.UI
{
    public sealed class OverlayText : Overlay
    {
        public readonly string Text;
        public readonly Color Color;
        public readonly Color? ShadowColor;

        public OverlayText(GDI_Point screenPosition, Color color, string text, Color? shadowColor = null) : base(screenPosition)
        {
            Color = color;
            ShadowColor = shadowColor;
            Text = text;
        }

        public override void Draw()
        {
            var ui = IRogueUI.UI;
            if (ShadowColor.HasValue)
                ui.UI_DrawString(ShadowColor.Value, Text, ScreenPosition.X + 1, ScreenPosition.Y + 1, new Color?());
            ui.UI_DrawString(Color, Text, ScreenPosition.X, ScreenPosition.Y, new Color?());
        }
    }
}
