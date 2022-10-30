// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.IRogueUI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;

// type leakage from System.Drawing, System.Windows.Forms
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using Cursor = System.Windows.Forms.Cursor;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using MouseButtons = System.Windows.Forms.MouseButtons;
using ColorString = System.Collections.Generic.KeyValuePair<System.Drawing.Color, string>;

namespace djack.RogueSurvivor.UI
{
  /// <summary>
  /// Provides UI functionalities to a Rogue game.
  /// </summary>
  public interface IRogueUI
  {
#nullable enable
    // C# 8.0: cooperative almost-singleton now possible for interfaces
    static private IRogueUI? s_ooao = null;

    static public IRogueUI UI {
        get { return s_ooao!; }
        protected set {
            if (null != s_ooao) throw new InvalidOperationException("cannot cooperative-set singleton twice");
            s_ooao = value;
        }
    }

    static public bool IsConstructed { get { return null != s_ooao; } }
#nullable restore

    IEnumerable<string> Mods { get; }

#region Input
#nullable enable
    KeyEventArgs UI_WaitKey();
    KeyValuePair<KeyValuePair<KeyEventArgs?, MouseButtons?>, KeyValuePair<int, int>> WaitKeyOrMouse();
#nullable restore

    void UI_PostKey(KeyEventArgs e);
    void UI_PostMouseButtons(MouseButtons buttons);

    // mouse event support
    // formally should be Point, but we don't want to leak System.Drawing types more than we already are
    void Add(Action<int, int> op);
    void Add(Func<int, int, bool> op);

    void UI_SetCursor(Cursor cursor);

    void WaitEnter();
    void WaitEscape();
    bool WaitEscape(Predicate<KeyEventArgs> ok);
#nullable enable
    T? WaitEscape<T>(Func<KeyEventArgs, T?> ok) where T:class;
#nullable restore
    bool WaitYesOrNo();

    bool Modal(Func<KeyEventArgs, bool?> ok);
    T? Modal<T>(Func<KeyEventArgs, T?> ok) where T:struct;
#endregion

    void UI_Wait(int msecs);

#region Canvas Painting
    void UI_Repaint();
    void ClearScreen();
    void UI_DrawImage(string imageID, int gx, int gy);
    void UI_DrawImage(string imageID, int gx, int gy, Color tint);
    void UI_DrawImageTransform(string imageID, int gx, int gy, float rotation, float scale);
    void UI_DrawGrayLevelImage(string imageID, int gx, int gy);
    void UI_DrawTransparentImage(float alpha, string imageID, int gx, int gy);
    void UI_DrawPoint(Color color, int gx, int gy);
    void UI_DrawLine(Color color, int gxFrom, int gyFrom, int gxTo, int gyTo);
    void UI_DrawRect(Color color, Rectangle rect);
    void UI_FillRect(Color color, Rectangle rect);
    void UI_DrawString(Color color, string text, int gx, int gy, Color? shadowColor = null);
    void UI_DrawString(ColorString text, int gx, int gy, Color? shadowColor = null);
    void UI_DrawStringBold(Color color, string text, int gx, int gy, Color? shadowColor = null);
    void UI_DrawStringBold(ColorString text, int gx, int gy, Color? shadowColor = null);
    void UI_DrawPopup(string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, int gx, int gy);
    // alpha10
    void UI_DrawPopupTitle(string title, Color titleColor, string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, int gx, int gy);
    void UI_DrawPopupTitleColors(string title, Color titleColor, string[] lines, Color[] colors, Color boxBorderColor, Color boxFillColor, int gx, int gy);

#region Minimap painting
    void UI_ClearMinimap(Color color);
    void UI_SetMinimapColor(int x, int y, Color color);
    void UI_DrawMinimap(int gx, int gy);
#endregion
#endregion

#region pre-rendered tiles
    void AddTile(string imageID);
    void AddTile(string imageID, Color color);
    void AppendTile(string imageID);
    void AppendTile(string imageID, Color color);
    void AppendTile(Color color, string text, Font font, int x, int y);
    void DrawTile(int x, int y);
#endregion

#region Canvas scaling - to convert mouse coordinates to canvas coordinates.
    float UI_GetCanvasScaleX();
    float UI_GetCanvasScaleY();
#endregion

#region Screenshots
    /// <param name="filePath">file path without the extension, eg: c:\screenshot100</param>
    string UI_SaveScreenshot(string filePath);

    /// <summary>
    /// Extension without the point eg: "png".
    /// </summary>
    string UI_ScreenshotExtension();
#endregion

    void UI_DoQuit();
  }
}
