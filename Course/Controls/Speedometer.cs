using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Course.Controls
{
    /// <summary>
    /// Compteur de vitesse circulaire style tableau de bord sportif.
    /// Arc de 270 degres (135 a 405), zones vert/jaune/rouge, aiguille rouge-orange, hub central.
    /// </summary>
    public class Speedometer : Control
    {
        private const float AngleDebut = 135f;
        private const float Amplitude = 270f;
        private const int NbGraduations = 10;
        private const float SeuilJaune = 0.70f;
        private const float SeuilRouge = 0.85f;

        private double vitesseActuelle;
        private double vitesseMax = 200;

        public double VitesseActuelle
        {
            get { return vitesseActuelle; }
            set
            {
                vitesseActuelle = Clamp(value, 0, vitesseMax);
                Invalidate();
            }
        }

        public double VitesseMax
        {
            get { return vitesseMax; }
            set
            {
                vitesseMax = Math.Max(1, value);
                if (vitesseActuelle > vitesseMax) vitesseActuelle = vitesseMax;
                Invalidate();
            }
        }

        public Speedometer()
        {
            DoubleBuffered = true;
            BackColor = Color.FromArgb(28, 28, 30);
            ForeColor = Color.White;
            Size = new Size(240, 240);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            var cx = Width / 2f;
            var cy = Height / 2f;
            var rayon = Math.Min(Width, Height) / 2f - 8;

            DessinerCadran(g, cx, cy, rayon);
            DessinerZones(g, cx, cy, rayon);
            DessinerGraduations(g, cx, cy, rayon);
            DessinerAffichageNumerique(g, cx, cy, rayon);
            DessinerAiguille(g, cx, cy, rayon);
            DessinerHub(g, cx, cy);
        }

        private static void DessinerCadran(Graphics g, float cx, float cy, float rayon)
        {
            var rect = new RectangleF(cx - rayon, cy - rayon, rayon * 2, rayon * 2);

            using (var fond = new LinearGradientBrush(
                rect,
                Color.FromArgb(38, 38, 44),
                Color.FromArgb(14, 14, 18),
                LinearGradientMode.ForwardDiagonal))
            {
                g.FillEllipse(fond, rect);
            }

            using (var contour = new Pen(Color.FromArgb(85, 85, 92), 2))
            {
                g.DrawEllipse(contour, rect);
            }
        }

        private static void DessinerZones(Graphics g, float cx, float cy, float rayon)
        {
            const float epaisseur = 8f;
            var rArc = rayon - 12;
            var rect = new RectangleF(cx - rArc, cy - rArc, rArc * 2, rArc * 2);

            using (var penVert = new Pen(Color.FromArgb(70, 200, 110), epaisseur))
            {
                penVert.StartCap = LineCap.Round;
                g.DrawArc(penVert, rect, AngleDebut, Amplitude * SeuilJaune);
            }

            using (var penJaune = new Pen(Color.FromArgb(240, 200, 60), epaisseur))
            {
                g.DrawArc(penJaune, rect,
                    AngleDebut + Amplitude * SeuilJaune,
                    Amplitude * (SeuilRouge - SeuilJaune));
            }

            using (var penRouge = new Pen(Color.FromArgb(220, 60, 60), epaisseur))
            {
                penRouge.EndCap = LineCap.Round;
                g.DrawArc(penRouge, rect,
                    AngleDebut + Amplitude * SeuilRouge,
                    Amplitude * (1f - SeuilRouge));
            }
        }

        private void DessinerGraduations(Graphics g, float cx, float cy, float rayon)
        {
            var rTickExt = rayon - 22;
            var rTickMaj = rayon - 36;
            var rTickMin = rayon - 30;
            var rTexte = rayon - 50;

            using (var penMaj = new Pen(Color.White, 2.5f))
            using (var penMin = new Pen(Color.FromArgb(140, 140, 145), 1f))
            using (var fontGrad = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (var brushTxt = new SolidBrush(Color.White))
            {
                for (int i = 0; i <= NbGraduations; i++)
                {
                    float c, s;
                    AngleVers((float)i / NbGraduations, out c, out s);

                    g.DrawLine(penMaj,
                        cx + rTickExt * c, cy + rTickExt * s,
                        cx + rTickMaj * c, cy + rTickMaj * s);

                    var val = ((int)Math.Round(vitesseMax * i / NbGraduations)).ToString();
                    var ts = g.MeasureString(val, fontGrad);
                    g.DrawString(val, fontGrad, brushTxt,
                        cx + rTexte * c - ts.Width / 2,
                        cy + rTexte * s - ts.Height / 2);

                    if (i < NbGraduations)
                    {
                        for (int j = 1; j <= 4; j++)
                        {
                            float cM, sM;
                            AngleVers((i + j / 5f) / NbGraduations, out cM, out sM);
                            g.DrawLine(penMin,
                                cx + rTickExt * cM, cy + rTickExt * sM,
                                cx + rTickMin * cM, cy + rTickMin * sM);
                        }
                    }
                }
            }
        }

        private void DessinerAffichageNumerique(Graphics g, float cx, float cy, float rayon)
        {
            var txt = vitesseActuelle.ToString("F0");
            using (var fontTxt = new Font("Consolas", 18f, FontStyle.Bold))
            using (var brushTxt = new SolidBrush(Color.White))
            using (var fontUnit = new Font("Segoe UI", 8f))
            using (var brushUnit = new SolidBrush(Color.FromArgb(170, 170, 180)))
            {
                var ts = g.MeasureString(txt, fontTxt);
                var yReadout = cy + rayon * 0.55f;
                g.DrawString(txt, fontTxt, brushTxt,
                    cx - ts.Width / 2,
                    yReadout - ts.Height / 2);

                const string unite = "km/h";
                var tu = g.MeasureString(unite, fontUnit);
                g.DrawString(unite, fontUnit, brushUnit,
                    cx - tu.Width / 2,
                    yReadout + ts.Height / 2);
            }
        }

        private void DessinerAiguille(Graphics g, float cx, float cy, float rayon)
        {
            var ratio = (float)(vitesseActuelle / vitesseMax);
            float c, s;
            AngleVers(ratio, out c, out s);

            var longueur = rayon - 28;
            const float recul = -10f;
            const float largeur = 5f;

            var bout = new PointF(cx + longueur * c, cy + longueur * s);
            var base1 = new PointF(cx + recul * c - largeur * s, cy + recul * s + largeur * c);
            var base2 = new PointF(cx + recul * c + largeur * s, cy + recul * s - largeur * c);
            var pts = new[] { bout, base1, base2 };

            using (var brush = new SolidBrush(Color.FromArgb(255, 95, 55)))
                g.FillPolygon(brush, pts);
            using (var pen = new Pen(Color.FromArgb(160, 40, 15), 1f))
                g.DrawPolygon(pen, pts);
        }

        private static void DessinerHub(Graphics g, float cx, float cy)
        {
            const float r = 12f;
            using (var brush = new SolidBrush(Color.FromArgb(55, 55, 62)))
                g.FillEllipse(brush, cx - r, cy - r, r * 2, r * 2);
            using (var pen = new Pen(Color.FromArgb(125, 125, 130), 1.5f))
                g.DrawEllipse(pen, cx - r, cy - r, r * 2, r * 2);

            const float r2 = 4f;
            using (var brushInt = new SolidBrush(Color.FromArgb(210, 210, 220)))
                g.FillEllipse(brushInt, cx - r2, cy - r2, r2 * 2, r2 * 2);
        }

        private static void AngleVers(float ratio, out float cos, out float sin)
        {
            var rad = (AngleDebut + ratio * Amplitude) * (float)Math.PI / 180f;
            cos = (float)Math.Cos(rad);
            sin = (float)Math.Sin(rad);
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
