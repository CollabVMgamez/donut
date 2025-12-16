/*
 * Simple Windows Forms 3D Spinning Donut
 * ------------------------------------------------------------
 * This single-file program creates a window that displays a rotating
 * donut (torus) using a classic parametric 3D rendering algorithm.
 *
 * Build & Run (Windows Command Prompt):
 *   csc /t:winexe /out:Donut.exe src/Donut.cs /r:System.Drawing.dll /r:System.Windows.Forms.dll
 *   Donut.exe
 *
 * No external libraries are required beyond the .NET Framework.
 *
 * The animation runs at ~30 fps using a System.Windows.Forms.Timer.
 */

using System;
using System.Drawing;
using System.Windows.Forms;

public class DonutForm : Form
{
    private const int ScreenWidth = 400;
    private const int ScreenHeight = 400;
    private const float ThetaStep = 0.07f;
    private const float PhiStep   = 0.02f;
    private const float R1 = 1.0f;   // tube radius
    private const float R2 = 2.0f;   // torus radius
    private const float K1 = 150;    // scaling factor

    private float A = 0.0f; // rotation angle around X‑axis
    private float B = 0.0f; // rotation angle around Z‑axis

    private Bitmap _bitmap;
    private Timer _timer;

    public DonutForm()
    {
        this.Text = "Spinning Donut";
        this.ClientSize = new Size(ScreenWidth, ScreenHeight);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;

        _bitmap = new Bitmap(ScreenWidth, ScreenHeight);
        _timer = new Timer();
        _timer.Interval = 30; // ~33 fps
        _timer.Tick += (s, e) => { UpdateDonut(); Invalidate(); };
        _timer.Start();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.DrawImageUnscaled(_bitmap, 0, 0);
    }

    private void UpdateDonut()
    {
        // Clear bitmap
        using (Graphics g = Graphics.FromImage(_bitmap))
        {
            g.Clear(Color.Black);
        }

        // Pre‑compute sin/cos for current rotations
        float cosA = (float)Math.Cos(A), sinA = (float)Math.Sin(A);
        float cosB = (float)Math.Cos(B), sinB = (float)Math.Sin(B);

        // Luminance buffer – optional, here we just map to brightness
        for (float theta = 0; theta < 2 * (float)Math.PI; theta += ThetaStep)
        {
            float costheta = (float)Math.Cos(theta);
            float sintheta = (float)Math.Sin(theta);

            for (float phi = 0; phi < 2 * (float)Math.PI; phi += PhiStep)
            {
                float cosphi = (float)Math.Cos(phi);
                float sinphi = (float)Math.Sin(phi);

                // 3‑D coordinates of the point on the torus before rotation
                float circleX = R2 + R1 * costheta;
                float circleY = R1 * sintheta;

                // Apply rotations (X‑axis = A, Z‑axis = B)
                float x = circleX * (cosB * cosphi + sinA * sinB * sinphi) - circleY * cosA * sinB;
                float y = circleX * (sinB * cosphi - sinA * cosB * sinphi) + circleY * cosA * cosB;
                float z = cosA * circleX * sinphi + circleY * sinA + 5; // shift forward

                float ooz = 1 / z; // "one over z"

                // 2‑D projection coordinates
                int xp = (int)(ScreenWidth / 2 + K1 * ooz * x);
                int yp = (int)(ScreenHeight / 2 - K1 * ooz * y);

                // Simple shading based on surface normal (dot product with light vector)
                float L = cosphi * costheta * sinB - cosA * costheta * sinphi - sinA * sintheta + cosB * (cosA * sintheta - costheta * sinA * sinphi);
                int brightness = (int)Math.Round(Math.Max(0f, Math.Min(1f, L)) * 255f);
                if (brightness > 255) brightness = 255;
                if (brightness < 0) brightness = 0;
                Color col = Color.FromArgb(brightness, brightness, brightness);

                // Draw pixel if within bounds
                if (xp >= 0 && xp < ScreenWidth && yp >= 0 && yp < ScreenHeight)
                {
                    _bitmap.SetPixel(xp, yp, col);
                }
            }
        }

        // Increment rotation angles
        A += 0.04f;
        B += 0.02f;
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new DonutForm());
    }
}