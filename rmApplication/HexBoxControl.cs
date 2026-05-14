using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Windows.Forms.VisualStyles;

namespace rmApplication
{
    // ---------------------------------------------------------------------------
    // HexCasing
    // ---------------------------------------------------------------------------
    /// <summary>
    /// Controls whether hex digits are rendered in upper-case ('A'–'F')
    /// or lower-case ('a'–'f').
    /// </summary>
    public enum HexCasing
    {
        Upper = 0,
        Lower = 1,
    }

    // ---------------------------------------------------------------------------
    // HexBoxControl
    // ---------------------------------------------------------------------------
    /// <summary>
    /// A read-only hex-viewer <see cref="UserControl"/>.
    ///
    /// <para><b>Responsibilities (this class):</b> rendering (GDI+), resize handling,
    /// mouse-wheel forwarding, and WinForms property plumbing.</para>
    ///
    /// <para>Layout arithmetic is encapsulated in <see cref="LayoutMetrics"/>;
    /// scroll state is encapsulated in <see cref="ScrollController"/>.</para>
    ///
    /// <para>When <see cref="BaseAddress"/> is not aligned to a row boundary,
    /// the leading cells of the first row are left blank so that byte addresses
    /// visually correspond to the original address space.</para>
    ///
    /// <para><b>Typical usage:</b></para>
    /// <code>
    /// hexBox.BaseAddress = 0x0023;
    /// hexBox.Write(myByteArray);   // display data
    /// hexBox.Clear();              // reset to blank state
    /// </code>
    /// </summary>
    public partial class HexBoxControl : UserControl
    {
        // ── Inner types ────────────────────────────────────────────────────────

        /// <summary>
        /// Holds all pre-computed geometry values that depend on font metrics,
        /// control size, and property settings.
        ///
        /// <para>Call <see cref="Recalculate"/> whenever the font, size, or any
        /// layout-affecting property changes.  All other code should treat these
        /// fields as read-only snapshots.</para>
        /// </summary>
        private sealed class LayoutMetrics
        {
            // Character cell size (ceiling of measured glyph, in pixels).
            public SizeF CharSize;

            // The usable drawing area inside the border.
            public Rectangle ContentBounds;

            // Left column: byte-address labels ("00000010 …").
            public Rectangle LineInfoBounds;

            // Top row: column-index header ("00 01 02 …").
            public Rectangle ColumnInfoBounds;

            // Main hex grid area.
            public Rectangle HexGridBounds;

            // How many bytes fit horizontally / vertically / in total.
            public int BytesPerRow;       // columns
            public int VisibleRowCount;   // rows
            public int VisibleByteCount;  // BytesPerRow * VisibleRowCount

            /// <summary>
            /// Re-computes every field.  <paramref name="control"/> is the owning
            /// <see cref="HexBoxControl"/>; its current properties are read directly.
            /// </summary>
            public void Recalculate(HexBoxControl control)
            {
                // ── 1. Measure a representative character ──────────────────────
                using (var gfx = control.CreateGraphics())
                {
                    SizeF raw = gfx.MeasureString("A", control.Font, 100, control._stringFormat);
                    CharSize = new SizeF(
                        (float)Math.Ceiling(raw.Width),
                        (float)Math.Ceiling(raw.Height));
                }

                // ── 2. Content area (client rect minus border) ─────────────────
                var border = control._borderThickness;
                ContentBounds = new Rectangle(
                    control.ClientRectangle.X + border.Left,
                    control.ClientRectangle.Y + border.Top,
                    control.ClientRectangle.Width - border.Left - border.Right,
                    control.ClientRectangle.Height - border.Top - border.Bottom);

                // ── 3. Reserve space for the vertical scroll-bar ───────────────
                if (control.VScrollBarVisible)
                {
                    ContentBounds.Width -= control._scrollBar.Width;
                    control._scrollBar.Left = ContentBounds.Right;
                    control._scrollBar.Top = ContentBounds.Top;
                    control._scrollBar.Height = ContentBounds.Height;
                }

                const int LeftMargin = 4; // px gap between border and content

                // ── 4. Line-info column (address labels on the left) ───────────
                if (control.LineInfoVisible)
                {
                    LineInfoBounds = new Rectangle(
                        ContentBounds.X + LeftMargin,
                        ContentBounds.Y,
                        (int)(CharSize.Width * 10),   // "00000000" = 8 hex + 2 padding
                        ContentBounds.Height);
                }
                else
                {
                    // Still track X so the hex grid starts at the right offset.
                    LineInfoBounds = new Rectangle(LeftMargin, ContentBounds.Y, 0, 0);
                }

                // ── 5. Column-info row (header across the top) ─────────────────
                int headerHeight = control.ColumnInfoVisible
                    ? (int)CharSize.Height + 4
                    : 0;
                ColumnInfoBounds = new Rectangle(
                    LineInfoBounds.Right,
                    ContentBounds.Y,
                    ContentBounds.Width - LineInfoBounds.Width,
                    headerHeight);

                // Push the line-info column down by the header height.
                LineInfoBounds.Y += headerHeight;
                LineInfoBounds.Height -= headerHeight;

                // ── 6. Hex grid ────────────────────────────────────────────────
                HexGridBounds = new Rectangle(
                    LineInfoBounds.Right,
                    LineInfoBounds.Y,
                    ContentBounds.Width - LineInfoBounds.Width,
                    ContentBounds.Height - headerHeight);

                // ── 7. Bytes-per-row ───────────────────────────────────────────
                if (control.UseFixedBytesPerLine)
                {
                    BytesPerRow = control.BytesPerLine;
                    // Shrink the hex grid to exactly fit the fixed column count.
                    HexGridBounds.Width = (int)Math.Floor(
                        BytesPerRow * CharSize.Width * 3 + 2 * CharSize.Width);
                }
                else
                {
                    // Each hex byte occupies 3 character widths ("FF ").
                    int availableChars = (int)Math.Floor(HexGridBounds.Width / CharSize.Width);
                    BytesPerRow = availableChars > 1
                        ? (int)Math.Floor(availableChars / 3.0)
                        : 1;
                    HexGridBounds.Width = (int)Math.Floor(
                        BytesPerRow * CharSize.Width * 3 + 2 * CharSize.Width);
                }

                VisibleRowCount = (int)Math.Floor(HexGridBounds.Height / (double)CharSize.Height);
                VisibleByteCount = BytesPerRow * VisibleRowCount;
            }

            // ── Coordinate helpers ─────────────────────────────────────────────

            /// <summary>
            /// Converts a flat virtual-index offset (relative to the visible window's
            /// first virtual slot) to a <c>(column, row)</c> grid cell.
            /// </summary>
            public Point ByteOffsetToGridCell(long byteOffset)
            {
                int row = (int)Math.Floor((double)byteOffset / BytesPerRow);
                int column = (int)(byteOffset - (long)row * BytesPerRow);
                return new Point(column, row);
            }

            /// <summary>Returns the top-left pixel of a grid cell in the hex area.</summary>
            public PointF GridCellToPixel(Point cell)
            {
                float x = 3 * CharSize.Width * cell.X + HexGridBounds.X;
                float y = cell.Y * CharSize.Height + HexGridBounds.Y;
                return new PointF(x, y);
            }

            /// <summary>Returns the top-left pixel of column <paramref name="col"/>
            /// in the column-info header row.</summary>
            public PointF ColumnIndexToHeaderPixel(int col)
            {
                Point cell = ByteOffsetToGridCell(col);
                return new PointF(
                    3 * CharSize.Width * cell.X + ColumnInfoBounds.X,
                    ColumnInfoBounds.Y);
            }
        }

        // -----------------------------------------------------------------------

        /// <summary>
        /// Owns all scroll-position state and the mapping between logical line
        /// positions and the Win32 scroll-bar integer range.
        ///
        /// <para>The scroll-bar API uses a 16-bit integer range (0–65535).
        /// When the data exceeds 65535 lines the values are scaled proportionally;
        /// <see cref="ToScrollBarValue"/> and <see cref="FromScrollBarValue"/>
        /// perform the conversion.</para>
        /// </summary>
        private sealed class ScrollController
        {
            // Logical scroll range (line indices, not pixel offsets).
            public long Min;    // always 0
            public long Max;
            public long Position;

            private const int ScrollBarMaxValue = 65535;

            // ── Range / position management ────────────────────────────────────

            /// <summary>
            /// Clamps <paramref name="newPosition"/> into [<see cref="Min"/>,
            /// <see cref="Max"/>] and updates <see cref="Position"/> if the value
            /// actually changes.
            /// </summary>
            /// <returns><c>true</c> if the position changed.</returns>
            public bool TryScrollToLine(long newPosition)
            {
                long clamped = Math.Max(Min, Math.Min(Max, newPosition));
                if (clamped == Position) return false;
                Position = clamped;
                return true;
            }

            // ── Scroll-bar value conversion ────────────────────────────────────

            /// <summary>
            /// Maps a logical line position to the nearest scroll-bar integer value,
            /// applying proportional scaling when <see cref="Max"/> exceeds 65535.
            /// </summary>
            public int ToScrollBarValue(long linePosition)
            {
                if (Max <= ScrollBarMaxValue) return (int)linePosition;
                double percent = (double)linePosition / Max * 100.0;
                int raw = (int)Math.Floor(ScrollBarMaxValue / 100.0 * percent);
                return Math.Max(0, Math.Min(ScrollBarMaxValue, raw));
            }

            /// <summary>
            /// Inverse of <see cref="ToScrollBarValue"/>: maps a scroll-bar integer
            /// back to a logical line position.
            /// </summary>
            public long FromScrollBarValue(int scrollBarValue)
            {
                if (Max <= ScrollBarMaxValue) return scrollBarValue;
                double percent = (double)scrollBarValue / ScrollBarMaxValue * 100.0;
                return (long)Math.Floor(Max / 100.0 * percent);
            }

            /// <summary>
            /// Caps <paramref name="logicalMax"/> at 65535 for passing to
            /// <see cref="VScrollBar.Maximum"/>.
            /// </summary>
            public int ToScrollBarMax(long logicalMax) =>
                logicalMax > ScrollBarMaxValue ? ScrollBarMaxValue : (int)logicalMax;

            // ── ThumbTrack correction ──────────────────────────────────────────

            /// <summary>
            /// Snaps a thumb-track position to the true maximum when it is within
            /// the dead zone caused by integer rounding at large scroll ranges.
            ///
            /// <para>Without this correction, dragging the thumb all the way to the
            /// bottom lands a few lines short of the actual end of data.</para>
            /// </summary>
            public long CorrectThumbPosition(long rawPosition)
            {
                // The rounding error grows to ~10 lines when Max > 65535.
                int deadZone = Max > ScrollBarMaxValue ? 10 : 9;
                return ToScrollBarValue(rawPosition) >= ToScrollBarMax(Max) - deadZone
                    ? Max
                    : rawPosition;
            }
        }

        // ── Fields ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Current geometry snapshot.  Rebuilt by <see cref="RebuildLayout"/>
        /// on every resize, font change, or layout-affecting property change.
        /// </summary>
        private readonly LayoutMetrics _layout = new LayoutMetrics();

        /// <summary>
        /// Scroll state.  Updated by <see cref="RebuildScrollRange"/> and
        /// all PerformScroll* methods.
        /// </summary>
        private readonly ScrollController _scroll = new ScrollController();

        /// <summary>
        /// StringFormat used for all GDI+ text drawing.
        /// <c>MeasureTrailingSpaces</c> is required so that character widths
        /// remain consistent with <see cref="LayoutMetrics.CharSize"/>.
        /// </summary>
        private readonly StringFormat _stringFormat;

        /// <summary>The vertical scroll-bar control (declared in the Designer file).</summary>
        private VScrollBar _scrollBar;

        /// <summary>
        /// Debounce timer for <see cref="ScrollEventType.ThumbTrack"/> events.
        /// Rapid drag events are batched; only the last position within
        /// <see cref="ThumbTrackDebounceMs"/> is applied.
        /// </summary>
        private readonly Timer _thumbTrackDebounceTimer = new Timer();
        private long _pendingThumbTrackPosition;
        private int _lastThumbTrackTickCount;
        private const int ThumbTrackDebounceMs = 50;

        /// <summary>
        /// Pixel widths/heights of the border on each side.
        /// Updated whenever <see cref="BorderStyle"/> changes.
        /// </summary>
        private (int Left, int Top, int Right, int Bottom) _borderThickness;

        /// <summary>
        /// Byte index (into <see cref="Data"/>) of the first visible byte.
        /// Recomputed by <see cref="UpdateVisibleByteRange"/>.
        /// </summary>
        private long _firstVisibleByte;

        /// <summary>
        /// Byte index (into <see cref="Data"/>) of the last visible byte (inclusive).
        /// </summary>
        private long _lastVisibleByte;

        /// <summary>
        /// Format string passed to <see cref="byte.ToString(string)"/>.
        /// "X" → upper-case hex; "x" → lower-case hex.
        /// </summary>
        private string _hexFormat = "X";

        /// <summary>The byte array currently displayed, or <c>null</c> when cleared.</summary>
        private byte[] _data;

        // ── Constructor ────────────────────────────────────────────────────────

        /// <summary>Initializes the control and wires internal event handlers.</summary>
        public HexBoxControl()
        {
            InitializeComponent();
            _scrollBar = vScrollBar;   // alias to the Designer-generated field
            _scrollBar.Scroll += OnScrollBarScroll;

            // Default visuals.
            BackColor = Color.White;
            Font = SystemFonts.MessageBoxFont;

            // MeasureTrailingSpaces keeps character widths consistent with layout.
            _stringFormat = new StringFormat(StringFormat.GenericTypographic)
            {
                FormatFlags = StringFormatFlags.MeasureTrailingSpaces,
            };

            // Painting flags: owner-draw, double-buffered, no background flicker.
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, false);

            // Initial border thickness (matches BorderStyle.Fixed3D default).
            _borderThickness = (
                SystemInformation.Border3DSize.Width,
                SystemInformation.Border3DSize.Height,
                SystemInformation.Border3DSize.Width,
                SystemInformation.Border3DSize.Height);

            _thumbTrackDebounceTimer.Interval = ThumbTrackDebounceMs;
            _thumbTrackDebounceTimer.Tick += OnThumbTrackDebounceElapsed;
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>The byte array currently on display, or <c>null</c> after <see cref="Clear"/>.</summary>
        [Browsable(false)]
        public byte[] Data => _data;

        /// <summary>
        /// Loads <paramref name="data"/> into the control and repaints.
        /// Passing <c>null</c> is treated the same as an empty array.
        /// </summary>
        public void Write(byte[] data)
        {
            _data = data ?? Array.Empty<byte>();
            UpdateVisibleByteRange();
            RebuildLayout();
            Invalidate();
        }

        /// <summary>Clears the display and resets the scroll position to zero.</summary>
        public void Clear()
        {
            _data = null;
            _scroll.Position = 0;
            SyncScrollBarToState();
            Invalidate();
        }

        // ── Designer-visible properties ────────────────────────────────────────

        /// <summary>Background color used when the control is disabled.</summary>
        [Category("Appearance"), DefaultValue(typeof(Color), "WhiteSmoke")]
        public Color BackColorDisabled
        {
            get => _backColorDisabled;
            set => _backColorDisabled = value;
        }
        private Color _backColorDisabled = Color.FromName("WhiteSmoke");

        /// <summary>
        /// Number of bytes displayed per row when <see cref="UseFixedBytesPerLine"/> is <c>true</c>.
        /// </summary>
        [DefaultValue(16), Category("Hex")]
        public int BytesPerLine
        {
            get => _bytesPerLine;
            set
            {
                if (_bytesPerLine == value) return;
                _bytesPerLine = value;
                RebuildLayout();
                Invalidate();
            }
        }
        private int _bytesPerLine = 16;

        /// <summary>
        /// Number of bytes per separator group when <see cref="GroupSeparatorVisible"/> is <c>true</c>.
        /// </summary>
        [DefaultValue(4), Category("Hex")]
        public int GroupSize
        {
            get => _groupSize;
            set
            {
                if (_groupSize == value) return;
                _groupSize = value;
                RebuildLayout();
                Invalidate();
            }
        }
        private int _groupSize = 4;

        /// <summary>
        /// When <c>true</c>, the column count is fixed to <see cref="BytesPerLine"/>
        /// regardless of the control's width.
        /// </summary>
        [DefaultValue(false), Category("Hex")]
        public bool UseFixedBytesPerLine
        {
            get => _useFixedBytesPerLine;
            set
            {
                if (_useFixedBytesPerLine == value) return;
                _useFixedBytesPerLine = value;
                RebuildLayout();
                Invalidate();
            }
        }
        private bool _useFixedBytesPerLine;

        /// <summary>Shows or hides the vertical scroll-bar.</summary>
        [DefaultValue(false), Category("Hex")]
        public bool VScrollBarVisible
        {
            get => _vScrollBarVisible;
            set
            {
                if (_vScrollBarVisible == value) return;
                _vScrollBarVisible = value;
                if (_vScrollBarVisible) Controls.Add(_scrollBar);
                else Controls.Remove(_scrollBar);
                RebuildLayout();
                RebuildScrollRange();
            }
        }
        private bool _vScrollBarVisible;

        /// <summary>Shows a vertical line between every <see cref="GroupSize"/> columns.</summary>
        [DefaultValue(false), Category("Hex")]
        public bool GroupSeparatorVisible
        {
            get => _groupSeparatorVisible;
            set
            {
                if (_groupSeparatorVisible == value) return;
                _groupSeparatorVisible = value;
                RebuildLayout();
                Invalidate();
            }
        }
        private bool _groupSeparatorVisible;

        /// <summary>Shows the column-index header row above the hex grid.</summary>
        [DefaultValue(false), Category("Hex")]
        public bool ColumnInfoVisible
        {
            get => _columnInfoVisible;
            set
            {
                if (_columnInfoVisible == value) return;
                _columnInfoVisible = value;
                RebuildLayout();
                Invalidate();
            }
        }
        private bool _columnInfoVisible;

        /// <summary>Shows the byte-address column to the left of the hex grid.</summary>
        [DefaultValue(false), Category("Hex")]
        public bool LineInfoVisible
        {
            get => _lineInfoVisible;
            set
            {
                if (_lineInfoVisible == value) return;
                _lineInfoVisible = value;
                RebuildLayout();
                Invalidate();
            }
        }
        private bool _lineInfoVisible;

        /// <summary>
        /// The absolute address of the first byte in <see cref="Data"/>.
        /// When this value is not aligned to a row boundary, the leading cells
        /// of the first row are left blank so that the grid reflects the layout
        /// of the original address space.
        /// </summary>
        /// <example>
        /// If <c>BaseAddress = 0x0023</c> and <c>BytesPerRow = 16</c>, the first
        /// row starts at <c>0x0020</c> with columns 0–2 empty, and actual data
        /// begins at column 3.
        /// </example>
        [DefaultValue((long)0), Category("Hex")]
        public long BaseAddress
        {
            get => _baseAddress;
            set
            {
                if (_baseAddress == value) return;
                _baseAddress = value;
                RebuildLayout();
                Invalidate();
            }
        }
        private long _baseAddress;

        /// <summary>
        /// Border style of the control.
        /// Intentionally hides <see cref="Control.BorderStyle"/> to expose
        /// a setter that also updates the internal border-thickness cache
        /// used by layout calculations.
        /// </summary>
        [DefaultValue(typeof(BorderStyle), "Fixed3D"), Category("Hex")]
        public new BorderStyle BorderStyle
        {
            get => _borderStyle;
            set
            {
                if (_borderStyle == value) return;
                _borderStyle = value;
                switch (_borderStyle)
                {
                    case BorderStyle.None:
                        _borderThickness = (0, 0, 0, 0);
                        break;
                    case BorderStyle.Fixed3D:
                        _borderThickness = (
                            SystemInformation.Border3DSize.Width,
                            SystemInformation.Border3DSize.Height,
                            SystemInformation.Border3DSize.Width,
                            SystemInformation.Border3DSize.Height);
                        break;
                    case BorderStyle.FixedSingle:
                        _borderThickness = (1, 1, 1, 1);
                        break;
                }
                RebuildLayout();
            }
        }
        private BorderStyle _borderStyle = BorderStyle.Fixed3D;

        /// <summary>Whether hex digits are upper-case or lower-case.</summary>
        [DefaultValue(typeof(HexCasing), "Upper"), Category("Hex")]
        public HexCasing HexCasing
        {
            get => _hexFormat == "X" ? HexCasing.Upper : HexCasing.Lower;
            set
            {
                string newFormat = value == HexCasing.Upper ? "X" : "x";
                if (_hexFormat == newFormat) return;
                _hexFormat = newFormat;
                Invalidate();
            }
        }

        /// <summary>Foreground color for address labels and column headers.</summary>
        [DefaultValue(typeof(Color), "Gray"), Category("Hex")]
        public Color InfoForeColor
        {
            get => _infoForeColor;
            set { _infoForeColor = value; Invalidate(); }
        }
        private Color _infoForeColor = Color.Gray;

        /// <inheritdoc/>
        public override Font Font
        {
            get => base.Font;
            set
            {
                if (value == null) return;
                base.Font = value;
                RebuildLayout();
                Invalidate();
            }
        }

        // ── Scroll event handling ──────────────────────────────────────────────

        /// <summary>
        /// Handles all <see cref="VScrollBar.Scroll"/> event types and maps them
        /// to the appropriate logical scroll operation.
        /// </summary>
        private void OnScrollBarScroll(object sender, ScrollEventArgs e)
        {
            switch (e.Type)
            {
                case ScrollEventType.SmallIncrement:
                    PerformScrollByLines(1);
                    break;
                case ScrollEventType.SmallDecrement:
                    PerformScrollByLines(-1);
                    break;
                case ScrollEventType.LargeIncrement:
                    PerformScrollByLines(_layout.VisibleRowCount);
                    break;
                case ScrollEventType.LargeDecrement:
                    PerformScrollByLines(-_layout.VisibleRowCount);
                    break;
                case ScrollEventType.ThumbPosition:
                    // ThumbPosition fires once when the user releases the thumb.
                    PerformScrollToThumbPosition(_scroll.FromScrollBarValue(e.NewValue));
                    break;
                case ScrollEventType.ThumbTrack:
                    // ThumbTrack fires continuously while dragging.
                    // Debounce: apply immediately if the last event was long ago;
                    // otherwise restart the timer so only the final position is applied.
                    _thumbTrackDebounceTimer.Stop();
                    int now = Environment.TickCount;
                    if (now - _lastThumbTrackTickCount > ThumbTrackDebounceMs)
                        OnThumbTrackDebounceElapsed(null, null);
                    else
                    {
                        _pendingThumbTrackPosition = _scroll.FromScrollBarValue(e.NewValue);
                        _thumbTrackDebounceTimer.Start();
                    }
                    break;
            }

            // Keep the scroll-bar thumb in sync with the logical position
            // (important when the value was clamped or adjusted).
            e.NewValue = _scroll.ToScrollBarValue(_scroll.Position);
        }

        /// <summary>
        /// Called when the debounce timer fires: applies the last buffered
        /// thumb-track position.
        /// </summary>
        private void OnThumbTrackDebounceElapsed(object sender, EventArgs e)
        {
            _thumbTrackDebounceTimer.Stop();
            PerformScrollToThumbPosition(_pendingThumbTrackPosition);
            _lastThumbTrackTickCount = Environment.TickCount;
        }

        // ── Scroll logic ───────────────────────────────────────────────────────

        /// <summary>
        /// Scrolls by <paramref name="lineCount"/> lines (positive = down, negative = up).
        /// </summary>
        private void PerformScrollByLines(int lineCount)
        {
            if (lineCount == 0) return;
            PerformScrollToLine(_scroll.Position + lineCount);
        }

        /// <summary>
        /// Scrolls to an absolute line index, clamping to the valid range.
        /// Repaints if the position actually changes.
        /// </summary>
        private void PerformScrollToLine(long lineIndex)
        {
            if (!_scroll.TryScrollToLine(lineIndex)) return;
            SyncScrollBarToState();
            UpdateVisibleByteRange();
            Invalidate();
        }

        /// <summary>
        /// Scrolls to a position derived from a thumb drag, applying the
        /// dead-zone correction that compensates for integer rounding near
        /// the maximum value.
        /// </summary>
        private void PerformScrollToThumbPosition(long rawPosition)
        {
            PerformScrollToLine(_scroll.CorrectThumbPosition(rawPosition));
        }

        /// <summary>
        /// Recalculates <see cref="ScrollController.Max"/> based on current data,
        /// layout, and <see cref="BaseAddress"/> alignment padding, then pushes
        /// the updated state to the scroll-bar.
        /// </summary>
        private void RebuildScrollRange()
        {
            if (!VScrollBarVisible || _data == null || _data.Length == 0 || _layout.BytesPerRow == 0)
            {
                _scroll.Min = _scroll.Max = _scroll.Position = 0;
                SyncScrollBarToState();
                return;
            }

            int startPadding = GetStartPadding();
            long totalVirtualBytes = startPadding + _data.Length;
            long totalLines = (long)Math.Ceiling(
                                         (double)totalVirtualBytes / _layout.BytesPerRow);
            long newMax = Math.Max(0, totalLines - _layout.VisibleRowCount);
            long newPos = (_firstVisibleByte + startPadding) / _layout.BytesPerRow;

            if (newMax < _scroll.Max && _scroll.Position == _scroll.Max)
                PerformScrollByLines(-1);

            if (newMax == _scroll.Max && newPos == _scroll.Position) return;

            _scroll.Min = 0;
            _scroll.Max = newMax;
            _scroll.Position = Math.Min(newPos, newMax);
            SyncScrollBarToState();
        }

        /// <summary>
        /// Pushes the current <see cref="ScrollController"/> state to the
        /// WinForms <see cref="VScrollBar"/> control.
        /// </summary>
        private void SyncScrollBarToState()
        {
            int max = _scroll.ToScrollBarMax(_scroll.Max);
            if (max > 0)
            {
                _scrollBar.Minimum = 0;
                _scrollBar.Maximum = max;
                _scrollBar.Value = _scroll.ToScrollBarValue(_scroll.Position);
                _scrollBar.Visible = true;
            }
            else
            {
                _scrollBar.Visible = false;
            }
        }

        // ── Painting ───────────────────────────────────────────────────────────

        /// <inheritdoc/>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            switch (_borderStyle)
            {
                case BorderStyle.Fixed3D:
                    if (TextBoxRenderer.IsSupported)
                    {
                        // Let the visual-style renderer draw the themed border,
                        // then fill only the inner content area with the background color.
                        VisualStyleElement state = Enabled
                            ? VisualStyleElement.TextBox.TextEdit.Normal
                            : VisualStyleElement.TextBox.TextEdit.Disabled;
                        Color bg = Enabled ? BackColor : BackColorDisabled;
                        var vsr = new VisualStyleRenderer(state);
                        vsr.DrawBackground(e.Graphics, ClientRectangle);
                        Rectangle inner = vsr.GetBackgroundContentRectangle(e.Graphics, ClientRectangle);
                        using (var brush = new SolidBrush(bg))
                            e.Graphics.FillRectangle(brush, inner);
                    }
                    else
                    {
                        using (var brush = new SolidBrush(BackColor))
                            e.Graphics.FillRectangle(brush, ClientRectangle);
                        ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, Border3DStyle.Sunken);
                    }
                    break;

                case BorderStyle.FixedSingle:
                    using (var brush = new SolidBrush(BackColor))
                        e.Graphics.FillRectangle(brush, ClientRectangle);
                    ControlPaint.DrawBorder(e.Graphics, ClientRectangle, Color.Black, ButtonBorderStyle.Solid);
                    break;

                default: // BorderStyle.None
                    using (var brush = new SolidBrush(BackColor))
                        e.Graphics.FillRectangle(brush, ClientRectangle);
                    break;
            }
        }

        /// <inheritdoc/>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_data == null || _data.Length == 0) return;

            // Clip to the content area so hex text cannot bleed into the border.
            using (var clip = new Region(ClientRectangle))
            {
                clip.Exclude(_layout.ContentBounds);
                e.Graphics.ExcludeClip(clip);
            }

            UpdateVisibleByteRange();

            if (LineInfoVisible) PaintAddressColumn(e.Graphics);
            PaintHexGrid(e.Graphics);
            if (ColumnInfoVisible) PaintColumnHeader(e.Graphics);
            if (GroupSeparatorVisible) PaintGroupSeparators(e.Graphics);
        }

        /// <summary>
        /// Draws the address column: one 8-digit hex address per visible row.
        /// The address shown is the row-aligned virtual address, which may be
        /// lower than <see cref="BaseAddress"/> when it is not row-aligned.
        /// Rows that contain no actual data bytes are skipped.
        /// </summary>
        private void PaintAddressColumn(Graphics g)
        {
            int startPadding = GetStartPadding();
            long alignedBaseAddr = _baseAddress - startPadding; // virtual address of data byte 0

            using (var brush = new SolidBrush(
                InfoForeColor != Color.Empty ? InfoForeColor : ForeColor))
            {
                for (int row = 0; row <= _layout.VisibleRowCount; row++)
                {
                    long virtualRowStart = (_scroll.Position + row) * _layout.BytesPerRow;
                    long dataStart = virtualRowStart - startPadding;
                    long dataEnd = dataStart + _layout.BytesPerRow - 1;

                    // Skip rows that contain no actual data bytes.
                    if (dataEnd < 0 || dataStart >= _data.Length) continue;

                    long address = alignedBaseAddr + (_scroll.Position + row) * _layout.BytesPerRow;
                    PointF pos = _layout.GridCellToPixel(new Point(0, row));
                    g.DrawString(FormatAddressLabel(address), Font, brush,
                        new PointF(_layout.LineInfoBounds.X, pos.Y), _stringFormat);
                }
            }
        }

        /// <summary>
        /// Formats a byte address as an 8-digit zero-padded hex string.
        /// Returns "~~~~~~~~" when the address overflows 8 hex digits.
        /// </summary>
        private string FormatAddressLabel(long address)
        {
            string hex = address.ToString(_hexFormat,
                System.Threading.Thread.CurrentThread.CurrentCulture);
            return hex.Length <= 8
                ? hex.PadLeft(8, '0')
                : new string('~', 8);
        }

        /// <summary>
        /// Draws the column-index header row ("00 01 02 … 0F").
        /// </summary>
        private void PaintColumnHeader(Graphics g)
        {
            using (var brush = new SolidBrush(InfoForeColor))
            {
                for (int col = 0; col < _layout.BytesPerRow; col++)
                    DrawTwoCharHex(g, (byte)col, brush, _layout.ColumnIndexToHeaderPixel(col));
            }
        }

        /// <summary>
        /// Draws a vertical separator line between every <see cref="GroupSize"/> columns.
        /// </summary>
        private void PaintGroupSeparators(Graphics g)
        {
            using (var brush = new SolidBrush(InfoForeColor))
            using (var pen = new Pen(brush, 1f))
            {
                for (int col = GroupSize; col < _layout.BytesPerRow; col += GroupSize)
                {
                    PointF top = _layout.ColumnIndexToHeaderPixel(col);
                    top.X -= _layout.CharSize.Width / 2f;
                    float bottom = top.Y + _layout.ColumnInfoBounds.Height + _layout.HexGridBounds.Height;
                    g.DrawLine(pen, top, new PointF(top.X, bottom));
                }
            }
        }

        /// <summary>
        /// Draws every visible byte as a two-character hex value on the hex grid.
        /// Virtual slots that precede <see cref="BaseAddress"/> (the leading padding
        /// cells introduced by non-aligned base addresses) are left blank.
        /// </summary>
        private void PaintHexGrid(Graphics g)
        {
            int startPadding = GetStartPadding();
            long virtualFirst = _scroll.Position * _layout.BytesPerRow;
            // Include one extra row to avoid a half-visible gap at the bottom.
            long virtualEnd = virtualFirst + _layout.VisibleByteCount + _layout.BytesPerRow;

            using (var brush = new SolidBrush(Enabled ? ForeColor : Color.Gray))
            {
                for (long vIdx = virtualFirst; vIdx <= virtualEnd; vIdx++)
                {
                    long dataIdx = vIdx - startPadding;

                    if (dataIdx < 0) continue; // leading padding cell — leave blank
                    if (dataIdx >= _data.Length) break;

                    long offset = vIdx - virtualFirst;
                    Point cell = _layout.ByteOffsetToGridCell(offset);
                    if (cell.Y > _layout.VisibleRowCount) break;

                    PointF pos = _layout.GridCellToPixel(cell);
                    DrawTwoCharHex(g, _data[dataIdx], brush, pos);
                }
            }
        }

        /// <summary>
        /// Draws a single byte as two hex characters at <paramref name="origin"/>.
        /// The second character is offset by one character width to the right.
        /// </summary>
        private void DrawTwoCharHex(Graphics g, byte value, Brush brush, PointF origin)
        {
            string hex = ByteToHexString(value);
            g.DrawString(hex[0].ToString(), Font, brush, origin, _stringFormat);
            g.DrawString(hex[1].ToString(), Font, brush,
                new PointF(origin.X + _layout.CharSize.Width, origin.Y), _stringFormat);
        }

        // ── Layout management ──────────────────────────────────────────────────

        /// <summary>
        /// Recomputes all geometry (font metrics, rectangles, byte counts) and
        /// updates the scroll range to match.  Must be called after any property
        /// change that affects the visual layout.
        /// </summary>
        private void RebuildLayout()
        {
            _layout.Recalculate(this);
            RebuildScrollRange();
        }

        /// <summary>
        /// Updates <see cref="_firstVisibleByte"/> and <see cref="_lastVisibleByte"/>
        /// from the current scroll position, layout, and start padding.
        /// </summary>
        private void UpdateVisibleByteRange()
        {
            if (_data == null || _data.Length == 0) return;
            int startPadding = GetStartPadding();
            long virtualFirst = _scroll.Position * _layout.BytesPerRow;
            _firstVisibleByte = Math.Max(0, virtualFirst - startPadding);
            _lastVisibleByte = Math.Min(
                _data.Length - 1,
                virtualFirst - startPadding + _layout.VisibleByteCount);
        }

        // ── Control overrides ──────────────────────────────────────────────────

        /// <inheritdoc/>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            // Delta is positive for scroll-up, negative for scroll-down.
            // SystemInformation.MouseWheelScrollLines is typically 3.
            int lines = -(e.Delta * SystemInformation.MouseWheelScrollLines / 120);
            PerformScrollByLines(lines);
            base.OnMouseWheel(e);
        }

        /// <inheritdoc/>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RebuildLayout();
        }

        /// <inheritdoc/>
        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);
            // Defer layout until after the WinForms scaling pass completes
            // so that the new client size is already committed.
            BeginInvoke(new MethodInvoker(() =>
            {
                RebuildLayout();
                Invalidate();
            }));
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the number of blank cells that precede the first data byte
        /// on the first row, determined by the column offset of <see cref="BaseAddress"/>
        /// within a row.
        /// <para>
        /// Example: <c>BaseAddress = 0x0023</c>, <c>BytesPerRow = 16</c> → returns 3,
        /// meaning columns 0–2 of the first row are empty.
        /// </para>
        /// </summary>
        private int GetStartPadding() =>
            _layout.BytesPerRow > 0
                ? (int)(_baseAddress % _layout.BytesPerRow)
                : 0;

        /// <summary>
        /// Converts a byte to a two-character upper- or lower-case hex string
        /// according to the current <see cref="HexCasing"/> setting.
        /// Single-digit values (0x00–0x0F) are zero-padded to two characters.
        /// </summary>
        private string ByteToHexString(byte value)
        {
            string s = value.ToString(_hexFormat,
                System.Threading.Thread.CurrentThread.CurrentCulture);
            return s.Length == 1 ? "0" + s : s;
        }
    }
}