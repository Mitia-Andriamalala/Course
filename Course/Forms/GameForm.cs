using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Course.Controls;
using Course.Models;

namespace Course.Forms
{
    public class GameForm : Form
    {
        private const double DistanceTotale = 3000.0; // mètres

        private readonly Voiture voiture;
        private readonly Timer gameTimer;
        private readonly Stopwatch chrono = new Stopwatch();

        private double vitesse;     // km/h
        private double position;    // m
        private bool accelere;
        private bool courseFinie;
        private DateTime dernierTick;

        private Panel panneauPiste;
        private PictureBox voiturePb;
        private Label lblHorloge;
        private Label lblDuree;
        private Label lblVitesse;
        private Label lblDistance;
        private Label lblMax;
        private Label lblAccel;
        private Label lblNom;
        private Speedometer compteur;
        private Button btnAccelerer;

        public GameForm(Voiture v)
        {
            voiture = v;

            InitializeUi();

            gameTimer = new Timer { Interval = 30 };
            gameTimer.Tick += GameTimer_Tick;

            KeyPreview = true;
            KeyDown += GameForm_KeyDown;
            KeyUp += GameForm_KeyUp;

            chrono.Start();
            dernierTick = DateTime.Now;
            gameTimer.Start();
        }

        private void InitializeUi()
        {
            Text = "Course - " + voiture.Nom;
            Size = new Size(1020, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(28, 28, 30);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            // --- Piste de course ---------------------------------------
            panneauPiste = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(960, 130),
                BackColor = Color.FromArgb(55, 55, 60),
                BorderStyle = BorderStyle.FixedSingle
            };
            panneauPiste.Paint += PanneauPiste_Paint;

            voiturePb = new PictureBox
            {
                Size = new Size(64, 32),
                BackColor = Color.OrangeRed,
                Location = new Point(2, panneauPiste.Height / 2 - 16)
            };
            panneauPiste.Controls.Add(voiturePb);

            // --- Horloge / chrono --------------------------------------
            var boxHorloge = new Panel
            {
                Location = new Point(20, 170),
                Size = new Size(230, 200),
                BackColor = Color.FromArgb(45, 45, 48),
                BorderStyle = BorderStyle.FixedSingle
            };
            boxHorloge.Controls.Add(new Label
            {
                Text = "HEURE",
                Location = new Point(12, 8),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.LightGray
            });
            lblHorloge = new Label
            {
                Location = new Point(12, 28),
                Size = new Size(206, 40),
                Font = new Font("Consolas", 20, FontStyle.Bold),
                Text = "00:00:00",
                ForeColor = Color.Cyan
            };
            boxHorloge.Controls.Add(lblHorloge);

            boxHorloge.Controls.Add(new Label
            {
                Text = "DURÉE DE COURSE",
                Location = new Point(12, 90),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.LightGray
            });
            lblDuree = new Label
            {
                Location = new Point(12, 110),
                Size = new Size(206, 40),
                Font = new Font("Consolas", 20, FontStyle.Bold),
                Text = "00:00.00",
                ForeColor = Color.Yellow
            };
            boxHorloge.Controls.Add(lblDuree);

            // --- Compteur circulaire -----------------------------------
            compteur = new Speedometer
            {
                Location = new Point(270, 170),
                Size = new Size(240, 240),
                VitesseMax = voiture.VitesseMax
            };

            // --- Tableau de bord ---------------------------------------
            var tableau = new Panel
            {
                Location = new Point(530, 170),
                Size = new Size(450, 240),
                BackColor = Color.FromArgb(45, 45, 48),
                BorderStyle = BorderStyle.FixedSingle
            };
            tableau.Controls.Add(new Label
            {
                Text = "TABLEAU DE BORD",
                Location = new Point(12, 8),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.LightGray
            });
            lblNom      = AjouterStat(tableau, "Voiture",      voiture.Nom,                                                    40);
            lblVitesse  = AjouterStat(tableau, "Vitesse",      "0 km/h",                                                       70);
            lblDistance = AjouterStat(tableau, "Distance",     "0 / " + DistanceTotale.ToString("F0") + " m",                  100);
            lblMax      = AjouterStat(tableau, "Vitesse max",  voiture.VitesseMax.ToString("F0") + " km/h",                    130);
            lblAccel    = AjouterStat(tableau, "Accélération", voiture.CapaciteAcceleration.ToString("F0") + " km/h/s",        160);

            // --- Bouton Accélérer --------------------------------------
            btnAccelerer = new Button
            {
                Text = "ACCÉLÉRER  (maintenir Espace)",
                Location = new Point(20, 430),
                Size = new Size(960, 90),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                BackColor = Color.FromArgb(220, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TabStop = false
            };
            btnAccelerer.FlatAppearance.BorderSize = 0;
            btnAccelerer.MouseDown += (s, e) => accelere = true;
            btnAccelerer.MouseUp += (s, e) => accelere = false;
            btnAccelerer.MouseLeave += (s, e) => accelere = false;

            Controls.AddRange(new Control[] { panneauPiste, boxHorloge, compteur, tableau, btnAccelerer });
        }

        private static Label AjouterStat(Panel parent, string libelle, string valeur, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = libelle + " :",
                Location = new Point(12, y),
                Size = new Size(150, 24),
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.LightGray
            });
            var lblValeur = new Label
            {
                Text = valeur,
                Location = new Point(170, y),
                Size = new Size(270, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White
            };
            parent.Controls.Add(lblValeur);
            return lblValeur;
        }

        private void PanneauPiste_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var w = panneauPiste.Width;
            var h = panneauPiste.Height;
            var milieu = h / 2;

            using (var pen = new Pen(Color.White, 2))
                g.DrawLine(pen, 0, milieu, w, milieu);

            using (var pen = new Pen(Color.Yellow, 2))
            {
                for (int i = 0; i <= 10; i++)
                {
                    var x = i * w / 10;
                    g.DrawLine(pen, x, milieu - 10, x, milieu + 10);
                }
            }

            using (var brushVert = new SolidBrush(Color.LimeGreen))
            using (var brushBlanc = new SolidBrush(Color.White))
            {
                for (int i = 0; i < 8; i++)
                {
                    var y = i * h / 8;
                    var hh = h / 8;
                    g.FillRectangle(i % 2 == 0 ? brushVert : brushBlanc, w - 8, y, 8, hh);
                }
            }
        }

        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                accelere = true;
                e.SuppressKeyPress = true;
            }
        }

        private void GameForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                accelere = false;
                e.SuppressKeyPress = true;
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (courseFinie) return;

            var maintenant = DateTime.Now;
            var dt = (maintenant - dernierTick).TotalSeconds;
            dernierTick = maintenant;

            if (accelere && vitesse < voiture.VitesseMax)
            {
                vitesse = Math.Min(voiture.VitesseMax, vitesse + voiture.CapaciteAcceleration * dt);
            }

            position += (vitesse / 3.6) * dt;
            if (position >= DistanceTotale)
            {
                position = DistanceTotale;
                FinDeCourse();
            }

            var largeurUtile = panneauPiste.Width - voiturePb.Width - 8;
            voiturePb.Left = (int)(position / DistanceTotale * largeurUtile);

            compteur.VitesseActuelle = vitesse;
            lblHorloge.Text = maintenant.ToString("HH:mm:ss");
            lblDuree.Text = chrono.Elapsed.ToString(@"mm\:ss\.ff");
            lblVitesse.Text = vitesse.ToString("F1") + " km/h";
            lblDistance.Text = position.ToString("F0") + " / " + DistanceTotale.ToString("F0") + " m";
        }

        private void FinDeCourse()
        {
            courseFinie = true;
            gameTimer.Stop();
            chrono.Stop();
            accelere = false;

            var msg =
                "Course terminée !\r\n\r\n" +
                "Voiture : " + voiture.Nom + "\r\n" +
                "Distance : " + DistanceTotale.ToString("F0") + " m\r\n" +
                "Temps : " + chrono.Elapsed.ToString(@"mm\:ss\.ff") + "\r\n" +
                "Vitesse finale : " + vitesse.ToString("F1") + " km/h";

            MessageBox.Show(msg, "Bravo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            gameTimer.Stop();
            gameTimer.Dispose();
            base.OnFormClosed(e);
        }
    }
}
