namespace HLab.Geo.Avalonia;

public static class RectExtensions
{
   public static global::Avalonia.Rect ToAvalonia(this HLab.Geo.Rect r) => new (r.Left, r.Top, r.Width, r.Height);
   public static Rect ToHLab(this global::Avalonia.Rect r) => new(r.X, r.Y, r.Width, r.Height);

}