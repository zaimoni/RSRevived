using System.Drawing;

using GDI_Point = Zaimoni.Data.Vector2D<int>;
using GDI_Rectangle = Zaimoni.Data.Box2D<int>;

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
                ui.UI_DrawString(ShadowColor.Value, Text, ScreenPosition.X + 1, ScreenPosition.Y + 1);
            ui.UI_DrawString(Color, Text, ScreenPosition.X, ScreenPosition.Y);
        }
    }

    // cf competing implementation : GameImages::MonochromeBorderTile class and image caching
    public sealed class OverlayRect : Overlay
    {
        public readonly GDI_Rectangle Rectangle;
        public readonly Color Color;

        public OverlayRect(Color color, GDI_Rectangle rect) : base(rect.Location)
        {
            Rectangle = rect;
            Color = color;
        }

        public override void Draw()
        {
            var ui = IRogueUI.UI;
            ui.UI_DrawRect(Color, Rectangle);
        }
    }

    public class OverlayPopup : Overlay
    {
        public readonly Color TextColor;
        public readonly Color BoxBorderColor;
        public readonly Color BoxFillColor;
        public readonly string[] Lines;  // Cf. Advisor: need array entries to be writeable

        public OverlayPopup(string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, GDI_Point screenPos) : base(screenPos)
        {
            TextColor = textColor;
            BoxBorderColor = boxBorderColor;
            BoxFillColor = boxFillColor;
            Lines = lines;
        }

        public OverlayPopup(string line, Color textColor, Color boxBorderColor, Color boxFillColor, GDI_Point screenPos) : base(screenPos)
        {
            TextColor = textColor;
            BoxBorderColor = boxBorderColor;
            BoxFillColor = boxFillColor;
            Lines = new string[] { line };
        }

        public override void Draw()
        {
            var ui = IRogueUI.UI;
            ui.UI_DrawPopup(Lines, TextColor, BoxBorderColor, BoxFillColor, ScreenPosition.X, ScreenPosition.Y);
        }
    }

    // alpha10
    public sealed class OverlayPopupTitle : Overlay
    {
        public readonly string Title;
        public readonly Color TitleColor;
        public readonly string[] Lines;
        public readonly Color TextColor;
        public readonly Color BoxBorderColor;
        public readonly Color BoxFillColor;

        public OverlayPopupTitle(string title, Color titleColor, string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, GDI_Point screenPos) : base(screenPos)
        {
            Title = title;
            TitleColor = titleColor;
            TextColor = textColor;
            BoxBorderColor = boxBorderColor;
            BoxFillColor = boxFillColor;
            Lines = lines;
        }

        public override void Draw()
        {
            var ui = IRogueUI.UI;
            ui.UI_DrawPopupTitle(Title, TitleColor, Lines, TextColor, BoxBorderColor, BoxFillColor, ScreenPosition.X, ScreenPosition.Y);
        }
    }

#if PROTOTYPE
    public sealed class OverlayPopupTitleColors : Overlay
    {
            public string Title { get; set; }
            public Color TitleColor { get; set; }
            public string[] Lines { get; set; }
            public Color[] Colors { get; set; }
            public Color BoxBorderColor { get; set; }
            public Color BoxFillColor { get; set; }

            public OverlayPopupTitleColors(string title, Color titleColor, string[] lines, Color[] colors, Color boxBorderColor, Color boxFillColor, Point screenPos) : base(new GDI_Point(screenPos.X, screenPos.Y))
            {
                this.Title = title;
                this.TitleColor = titleColor;
                this.Colors = colors;
                this.BoxBorderColor = boxBorderColor;
                this.BoxFillColor = boxFillColor;
                this.Lines = lines;
            }

            public override void Draw()
            {
                var ui = IRogueUI.UI;
                ui.UI_DrawPopupTitleColors(Title, TitleColor, Lines, Colors, BoxBorderColor, BoxFillColor, ScreenPosition.X, ScreenPosition.Y);
            }
    }
#endif
}
