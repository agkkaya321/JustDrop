using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
public class VoisinSelectorForm : Form
{
    private readonly ListBox listBox;
    private readonly Button btnActualiser;
    private readonly Button btnSelectionner;
    private List<Voisin> voisins;

    public Voisin? SelectedVoisin { get; private set; }

    public VoisinSelectorForm(List<Voisin> voisins)
    {
        this.voisins = voisins ?? throw new ArgumentNullException(nameof(voisins));

        // Propriétés du Formulaire
        Text = "Liste des voisins";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(450, 400);

        // ListBox en Dock.Fill
        listBox = new ListBox
        {
            Dock = DockStyle.Fill,
            SelectionMode = SelectionMode.One
        };

        // Boutons
        btnActualiser = new Button
        {
            Text = "🔄 Actualiser",
            AutoSize = true
        };
        btnActualiser.Click += BtnActualiser_Click;

        btnSelectionner = new Button
        {
            Text = "Sélectionner",
            AutoSize = true
        };
        btnSelectionner.Click += BtnSelectionner_Click;

        // Panel pour centrer les boutons
        var panelButtons = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = btnActualiser.PreferredSize.Height
                  + btnSelectionner.PreferredSize.Height
                  + 20
        };

        // Calcul de la position des boutons
        void PositionButtons()
        {
            int centerX = (panelButtons.ClientSize.Width - btnActualiser.Width) / 2;
            btnActualiser.Location = new Point(centerX, 5);
            btnSelectionner.Location = new Point(centerX,
                btnActualiser.Bottom + 10);
        }

        panelButtons.Resize += (s, e) => PositionButtons();
        panelButtons.Controls.Add(btnActualiser);
        panelButtons.Controls.Add(btnSelectionner);
        PositionButtons(); // positionne la première fois

        // Ajout dans le Form (ordre important)
        Controls.Add(listBox);
        Controls.Add(panelButtons);

        // Remplissage initial
        ChargerVoisins(voisins);
    }

    private void BtnActualiser_Click(object? sender, EventArgs e)
    {
        // Rafraîchir la liste depuis l'extérieur, puis :
        ChargerVoisins(voisins);
    }

    private void BtnSelectionner_Click(object? sender, EventArgs e)
    {
        if (listBox.SelectedIndex >= 0)
        {
            SelectedVoisin = voisins[listBox.SelectedIndex];
            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            MessageBox.Show(
                "Veuillez sélectionner un voisin.",
                "Attention",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
    }

    public void ChargerVoisins(List<Voisin> nouveauxVoisins)
    {
        voisins = nouveauxVoisins ?? throw new ArgumentNullException(nameof(nouveauxVoisins));
        listBox.DataSource = null;
        listBox.DataSource = voisins;
        listBox.DisplayMember = nameof(Voisin.Name);
    }

    public static Voisin? AfficherEtChoisir(List<Voisin> voisins)
    {
        using var form = new VoisinSelectorForm(voisins);
        return form.ShowDialog() == DialogResult.OK
            ? form.SelectedVoisin
            : null;
    }
}