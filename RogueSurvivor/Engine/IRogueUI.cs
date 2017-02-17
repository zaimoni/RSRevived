// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.IRogueUI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System.Drawing;
using System.Windows.Forms;
using System.Security.Permissions;
using ColorString = System.Collections.Generic.KeyValuePair<System.Drawing.Color, string>;

namespace djack.RogueSurvivor.Engine
{
  /// <summary>
  /// Provides UI functionalities to a Rogue game.
  /// </summary>
  internal interface IRogueUI
  {
#region Input
    KeyEventArgs UI_WaitKey();
    KeyEventArgs UI_PeekKey();
    void UI_PostKey(KeyEventArgs e);

    Point UI_GetMousePosition();
    MouseButtons? UI_PeekMouseButtons();
    void UI_PostMouseButtons(MouseButtons buttons);

    void UI_SetCursor(Cursor cursor);
#endregion

    void UI_Wait(int msecs);

#region Canvas Painting
    void UI_Repaint();
    void UI_Clear(Color clearColor);
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
    void UI_DrawStringBold(Color color, string text, int gx, int gy, Color? shadowColor = null);
    void UI_DrawStringBold(ColorString text, int gx, int gy, Color? shadowColor = null);
    void UI_DrawPopup(string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, int gx, int gy);

#region Minimap painting
    void UI_ClearMinimap(Color color);
    void UI_SetMinimapColor(int x, int y, Color color);
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    void UI_DrawMinimap(int gx, int gy);
#endregion
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
