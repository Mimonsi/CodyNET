using Avalonia;
using Avalonia.Media;
using AvaloniaEdit.Rendering;

namespace CodyNET.Frontend.Controls;

/// <summary>
/// Draws a yellow highlight behind the source line that corresponds to the
/// CPU's current program counter.  Register it via
/// TextArea.TextView.BackgroundRenderers.Add(renderer).
/// </summary>
public class CurrentLineRenderer : IBackgroundRenderer
{
    private static readonly IBrush HighlightBrush =
        new SolidColorBrush(Color.FromArgb(55, 255, 220, 0));   // semi-transparent yellow

    /// <summary>1-based editor line number to highlight, or null to show nothing.</summary>
    public int? CurrentLine { get; set; }

    public KnownLayer Layer => KnownLayer.Background;

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (CurrentLine is null || !textView.VisualLinesValid) return;

        foreach (var vl in textView.VisualLines)
        {
            if (vl.FirstDocumentLine.LineNumber != CurrentLine.Value) continue;

            var y    = vl.VisualTop - textView.ScrollOffset.Y;
            var rect = new Rect(0, y, textView.Bounds.Width, vl.Height);
            drawingContext.FillRectangle(HighlightBrush, rect);
            break;
        }
    }
}
