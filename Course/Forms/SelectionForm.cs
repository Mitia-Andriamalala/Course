using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Course.Models;

namespace Course.Forms
{
    public class SelectionForm : Form
    {
        private readonly ListBox lstVoitures;
        private readonly PictureBox picApercu;
        private readonly Label lblInfo;
        private readonly Button btnDemarrer;
        private List<Voiture> voitures = new List<Voiture>();

        public SelectionForm()
        {
            Text = "Course - Choix de la voiture";
            Size = new Size(720, 480);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 245, 248);
            Font = new Font("Segoe UI", 10);

            var titre = new Label
            {
                Text = "Choisissez votre voiture",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White
            };

            lstVoitures = new ListBox
            {
                Location = new Point(20, 80),
                Size = new Size(280, 300),
                Font = new Font("Segoe UI", 11),
                IntegralHeight = false
            };
            lstVoitures.SelectedIndexChanged += (s, e) => RafraichirApercu();

            picApercu = new PictureBox
            {
                Location = new Point(320, 80),
                Size = new Size(360, 180),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };

            lblInfo = new Label
            {
                Location = new Point(320, 270),
                Size = new Size(360, 110),
                Font = new Font("Segoe UI", 10),
                Text = ""
            };

            btnDemarrer = new Button
            {
                Text = "Démarrer la course",
                Location = new Point(480, 395),
                Size = new Size(200, 40),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 153, 51),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDemarrer.FlatAppearance.BorderSize = 0;
            btnDemarrer.Click += BtnDemarrer_Click;

            Controls.AddRange(new Control[] { titre, lstVoitures, picApercu, lblInfo, btnDemarrer });

            ChargerVoitures();
        }

        private void ChargerVoitures()
        {
            try
            {
                voitures = Voiture.Charger("voitures.txt");
                foreach (var v in voitures) lstVoitures.Items.Add(v);
                if (voitures.Count > 0) lstVoitures.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Impossible de charger voitures.txt :\n" + ex.Message,
                    "Erreur de chargement",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void RafraichirApercu()
        {
            var v = lstVoitures.SelectedItem as Voiture;
            if (v == null) return;

            lblInfo.Text =
                "Vitesse maximum : " + v.VitesseMax.ToString("F0") + " km/h\r\n" +
                "Accélération : " + v.CapaciteAcceleration.ToString("F0") + " km/h/s\r\n" +
                "Temps pour atteindre v_max : " + (v.VitesseMax / v.CapaciteAcceleration).ToString("F1") + " s\r\n\r\n" +
                v.Description;

            try
            {
                if (picApercu.Image != null) picApercu.Image.Dispose();
                picApercu.Image = !string.IsNullOrEmpty(v.ImagePath) && File.Exists(v.ImagePath)
                    ? Image.FromFile(v.ImagePath)
                    : null;
            }
            catch
            {
                picApercu.Image = null;
            }
        }

        private void BtnDemarrer_Click(object sender, EventArgs e)
        {
            var v = lstVoitures.SelectedItem as Voiture;
            if (v == null)
            {
                MessageBox.Show("Sélectionnez une voiture.", "Information");
                return;
            }

            Hide();
            using (var jeu = new GameForm(v))
            {
                jeu.ShowDialog();
            }
            Show();
        }
    }
}
