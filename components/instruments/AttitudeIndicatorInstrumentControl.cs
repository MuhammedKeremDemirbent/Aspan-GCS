/*****************************************************************************/
/* Project  : AvionicsInstrumentControlDemo                                  */
/* File     : AttitudeIndicatorInstrumentControl.cs                          */
/* Version  : 1                                                              */
/* Language : C#                                                             */
/* Summary  : The attitude indicator instrument control                      */
/* Creation : 22/06/2008                                                     */
/* Autor    : Guillaume CHOUTEAU                                             */
/* History  :                                                                */
/*****************************************************************************/

using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Data;

namespace Aspan_Ground.components.instruments
{
    class AttitudeIndicatorInstrumentControl : InstrumentControl
    {
        #region Fields

        // Parameters
        double PitchAngle=0;  // Phi

        double RollAngle=0; // Theta

        // Images (Loaded safely)
        Bitmap bmpCadran;
        Bitmap bmpBoule;
        Bitmap bmpAvion;

        #endregion

        #region Contructor

        /// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

        public AttitudeIndicatorInstrumentControl()
		{
			// Double bufferisation
			SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint |
				ControlStyles.AllPaintingInWmPaint, true);
        }

        private string GetAssetPath(string fileName)
        {
            // Execute safe path resolution independent of WPF Designer's AppDomain
            string baseDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? AppDomain.CurrentDomain.BaseDirectory;
            return System.IO.Path.Combine(baseDir, "assets", "Icons", fileName);
        }

        #endregion

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }
        #endregion

        #region Paint

        protected override void OnPaint(PaintEventArgs pe)
        {
            // Calling the base class OnPaint
            base.OnPaint(pe);

            // Fill the background with the WPF Dark Theme Color to hide white corners
            using (SolidBrush darkBrush = new SolidBrush(ColorTranslator.FromHtml("#1B1F23")))
            {
                pe.Graphics.FillRectangle(darkBrush, this.ClientRectangle);
            }

            // Lazy Load Images
            if (bmpCadran == null || bmpBoule == null || bmpAvion == null)
            {
                try
                {
                    bmpCadran = new Bitmap(GetAssetPath("Horizon_Background.png"));
                    bmpBoule = new Bitmap(GetAssetPath("Horizon_GroundSky.png"));
                    bmpAvion = new Bitmap(GetAssetPath("Maquette_Avion.bmp"));
                }
                catch
                {
                    // Fallback for WPF Designer
                    if (bmpCadran == null) bmpCadran = new Bitmap(1, 1);
                    if (bmpBoule == null) bmpBoule = new Bitmap(1, 1);
                    if (bmpAvion == null) bmpAvion = new Bitmap(1, 1);
                }
            }

            // Pre Display computings

            Point ptBoule = new Point(25, - 210);
            Point ptRotation = new Point(150, 150);

            float scale = (float)this.Width / bmpCadran.Width;

            // Affichages - - - - - - - - - - - - - - - - - - - - - - 

            bmpCadran.MakeTransparent(Color.Yellow);
            bmpAvion.MakeTransparent(Color.Yellow);

            // display Horizon
            RotateAndTranslate(pe, bmpBoule, RollAngle, 0, ptBoule, (int)(4*PitchAngle), ptRotation, scale);

            // diplay mask
            using (Pen maskPen = new Pen(ColorTranslator.FromHtml("#1B1F23"), 30 * scale))
            {
                pe.Graphics.DrawRectangle(maskPen, 0, 0, bmpCadran.Width * scale, bmpCadran.Height * scale);
            }

            // display cadran
            pe.Graphics.DrawImage(bmpCadran, 0, 0, (float)(bmpCadran.Width * scale), (float)(bmpCadran.Height * scale));

            // display aircraft symbol
            pe.Graphics.DrawImage(bmpAvion, (float)((0.5 * bmpCadran.Width - 0.5 * bmpAvion.Width) * scale), (float)((0.5 * bmpCadran.Height - 0.5 * bmpAvion.Height) * scale), (float)(bmpAvion.Width * scale), (float)(bmpAvion.Height * scale));


        }

        #endregion

        #region Methods

        /// <summary>
        /// Define the physical value to be displayed on the indicator
        /// </summary>
        /// <param name="aircraftPitchAngle">The aircraft pitch angle in °deg</param>
        /// <param name="aircraftRollAngle">The aircraft roll angle in °deg</param
        public void SetAttitudeIndicatorParameters(float aircraftPitchAngle, float aircraftRollAngle)
        {
            PitchAngle = aircraftPitchAngle;
            RollAngle = aircraftRollAngle * Math.PI / 180; ;

            this.Refresh();
        }

        #endregion

    }
}
