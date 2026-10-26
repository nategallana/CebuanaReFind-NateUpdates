using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public static class GradientStyle
{
    #region Form

    public static void ApplyGradient(Form form, Color topColor, Color bottomColor)
    {
        form.Paint -= Form_Paint;
        form.Paint += Form_Paint;

        form.Tag = new GradientInfo
        {
            TopColor = topColor,
            BottomColor = bottomColor
        };

        form.Invalidate();
    }

    private static void Form_Paint(object sender, PaintEventArgs e)
    {
        Form form = sender as Form;
        GradientInfo info = form.Tag as GradientInfo;

        if (info == null)
            return;

        using (LinearGradientBrush brush = new LinearGradientBrush(
            form.ClientRectangle,
            info.TopColor,
            info.BottomColor,
            LinearGradientMode.Vertical))
        {
            e.Graphics.FillRectangle(brush, form.ClientRectangle);
        }
    }

    #endregion

    #region Panel

    public static void ApplyGradient(Panel panel, Color topColor, Color bottomColor)
    {
        panel.Paint -= Panel_Paint;
        panel.Paint += Panel_Paint;

        panel.Tag = new GradientInfo
        {
            TopColor = topColor,
            BottomColor = bottomColor
        };

        panel.Invalidate();
    }

    private static void Panel_Paint(object sender, PaintEventArgs e)
    {
        Panel panel = sender as Panel;
        GradientInfo info = panel.Tag as GradientInfo;

        if (info == null)
            return;

        using (LinearGradientBrush brush = new LinearGradientBrush(
            panel.ClientRectangle,
            info.TopColor,
            info.BottomColor,
            LinearGradientMode.Vertical))
        {
            e.Graphics.FillRectangle(brush, panel.ClientRectangle);
        }
    }

    #endregion

    #region Button

    public static void ApplyGradient(
        Button button,
        Color topColor,
        Color bottomColor,
        Color textColor,
        Color borderColor)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.ForeColor = textColor;
        button.Cursor = Cursors.Hand;

        button.Paint -= Button_Paint;
        button.Paint += Button_Paint;

        button.MouseEnter -= Button_MouseEnter;
        button.MouseLeave -= Button_MouseLeave;
        button.MouseDown -= Button_MouseDown;
        button.MouseUp -= Button_MouseUp;

        button.MouseEnter += Button_MouseEnter;
        button.MouseLeave += Button_MouseLeave;
        button.MouseDown += Button_MouseDown;
        button.MouseUp += Button_MouseUp;

        button.Tag = new GradientInfo
        {
            TopColor = topColor,
            BottomColor = bottomColor,
            TextColor = textColor,
            BorderColor = borderColor,

            OriginalTopColor = topColor,
            OriginalBottomColor = bottomColor,
            OriginalBorderColor = borderColor
        };

        button.Invalidate();
    }

    private static void Button_Paint(object sender, PaintEventArgs e)
    {
        Button button = sender as Button;
        GradientInfo info = button.Tag as GradientInfo;

        if (info == null)
            return;

        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        Rectangle rect = button.ClientRectangle;

        using (LinearGradientBrush brush = new LinearGradientBrush(
            rect,
            info.TopColor,
            info.BottomColor,
            LinearGradientMode.Vertical))
        {
            g.FillRectangle(brush, rect);
        }

        using (Pen pen = new Pen(info.BorderColor, 1))
        {
            g.DrawRectangle(pen, 0, 0, rect.Width - 1, rect.Height - 1);
        }

        TextRenderer.DrawText(
            g,
            button.Text,
            button.Font,
            rect,
            info.TextColor,
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter);
    }

    private static void Button_MouseEnter(object sender, EventArgs e)
    {
        Button button = sender as Button;
        GradientInfo info = button.Tag as GradientInfo;

        if (info == null)
            return;

        info.TopColor = ControlPaint.Light(info.OriginalTopColor, 0.15f);
        info.BottomColor = ControlPaint.Light(info.OriginalBottomColor, 0.15f);

        // Cebuana Gold
        info.BorderColor = Color.FromArgb(212, 175, 55);

        button.Invalidate();
    }

    private static void Button_MouseLeave(object sender, EventArgs e)
    {
        Button button = sender as Button;
        GradientInfo info = button.Tag as GradientInfo;

        if (info == null)
            return;

        info.TopColor = info.OriginalTopColor;
        info.BottomColor = info.OriginalBottomColor;
        info.BorderColor = info.OriginalBorderColor;

        button.Invalidate();
    }

    private static void Button_MouseDown(object sender, MouseEventArgs e)
    {
        Button button = sender as Button;
        GradientInfo info = button.Tag as GradientInfo;

        if (info == null)
            return;

        info.TopColor = ControlPaint.Dark(info.OriginalTopColor, 0.15f);
        info.BottomColor = ControlPaint.Dark(info.OriginalBottomColor, 0.15f);

        button.Invalidate();
    }

    private static void Button_MouseUp(object sender, MouseEventArgs e)
    {
        Button button = sender as Button;
        GradientInfo info = button.Tag as GradientInfo;

        if (info == null)
            return;

        info.TopColor = ControlPaint.Light(info.OriginalTopColor, 0.15f);
        info.BottomColor = ControlPaint.Light(info.OriginalBottomColor, 0.15f);

        button.Invalidate();
    }

    #endregion

    #region TextBox

    public static void ApplyStyle(
        TextBox textBox,
        Color backColor,
        Color textColor,
        Font font = null)
    {
        textBox.BackColor = backColor;
        textBox.ForeColor = textColor;
        textBox.BorderStyle = BorderStyle.FixedSingle;

        if (font != null)
            textBox.Font = font;
    }

    #endregion

    private class GradientInfo
    {
        public Color TopColor;
        public Color BottomColor;
        public Color TextColor;
        public Color BorderColor;

        public Color OriginalTopColor;
        public Color OriginalBottomColor;
        public Color OriginalBorderColor;
    }
}