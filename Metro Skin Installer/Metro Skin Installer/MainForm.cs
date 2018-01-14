using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Metro_Skin_Installer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }



        #region dragabbletitlebar
            private bool dragging = false;
            private Point dragCursorPoint;
            private Point dragFormPoint;

            private void FormMain_MouseDown(object sender, MouseEventArgs e)
            {
                dragging = true;
                dragCursorPoint = Cursor.Position;
                dragFormPoint = this.Location;
            }

            private void FormMain_MouseMove(object sender, MouseEventArgs e)
            {
                if (dragging)
                {
                    Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                    this.Location = Point.Add(dragFormPoint, new Size(dif));
                }
            }

            private void FormMain_MouseUp(object sender, MouseEventArgs e)
            {
                dragging = false;
            }
        #endregion

        #region Page1
            private void PatchedNextButton_Click(object sender, EventArgs e)
            {
                page1.Visible = false;
                page2patched.Visible = true;
            }
            private void OfficialInstallbutton_Click(object sender, EventArgs e)
            {
                page1.Visible = false;
                InstallerPage.Visible = true;

            }
        #endregion
    }
}
