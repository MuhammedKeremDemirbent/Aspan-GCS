/*****************************************************************************/
/* Project  : AvionicsInstrumentControlDemo                                  */
/* File     : HeadingIndicatorInstrumentControl.cs                           */
/* Version  : 1                                                              */
/* Language : C#                                                             */
/* Summary  : The heading indicator instrument control                       */
/* Creation : 25/06/2008                                                     */
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
    class HeadingIndicatorInstrumentControl : InstrumentControl
    {
        #region Fields

        // Parameters
        int Heading;

        // Images
        Bitmap bmpCadran;
        Bitmap bmpHedingWeel;
        Bitmap bmpAircaft;

        #endregion

        #region Contructor

        /// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

        public HeadingIndicatorInstrumentControl()
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
            if (bmpCadran == null || bmpHedingWeel == null || bmpAircaft == null)
            {
                try
                {
                    bmpCadran = new Bitmap(GetAssetPath("HeadingIndicator_Background.png"));
                    bmpHedingWeel = new Bitmap(GetAssetPath("HeadingWeel.png"));
                    bmpAircaft = new Bitmap(GetAssetPath("HeadingIndicator_Aircraft.bmp"));
                }
                catch
                {
                    // Fallback for WPF Designer
                    if (bmpCadran == null) bmpCadran = new Bitmap(1, 1);
                    if (bmpHedingWeel == null) bmpHedingWeel = new Bitmap(1, 1);
                    if (bmpAircaft == null) bmpAircaft = new Bitmap(1, 1);
                }
            }

            // Pre Display computings
            Point ptRotation = new Point(150, 150);
            Point ptImgAircraft = new Point(73,41);
            Point ptImgHeadingWeel = new Point(13, 13);

            bmpCadran.MakeTransparent(Color.Yellow);
            bmpHedingWeel.MakeTransparent(Color.Yellow);
            bmpAircaft.MakeTransparent(Color.Yellow);

            double alphaHeadingWeel = InterpolPhyToAngle(Heading,0,360,360,0);

            float scale = (float)this.Width / bmpCadran.Width;

            // diplay mask
            using (Pen maskPen = new Pen(ColorTranslator.FromHtml("#1B1F23"), 30 * scale))
            {
                pe.Graphics.DrawRectangle(maskPen, 0, 0, bmpCadran.Width * scale, bmpCadran.Height * scale);
            }

            // display cadran
            pe.Graphics.DrawImage(bmpCadran, 0, 0, (float)(bmpCadran.Width * scale), (float)(bmpCadran.Height * scale));

            // display HeadingWeel
            RotateImage(pe,bmpHedingWeel, alphaHeadingWeel, ptImgHeadingWeel, ptRotation, scale);

            // display aircraft
            pe.Graphics.DrawImage(bmpAircaft, (int)(ptImgAircraft.X*scale), (int)(ptImgAircraft.Y*scale), (float)(bmpAircaft.Width * scale), (float)(bmpAircaft.Height * scale));

        }

        #endregion

        #region Methods

        /// <summary>
        /// Define the physical value to be displayed on the indicator
        /// </summary>
        /// <param name="aircraftHeading">The aircraft heading in °deg</param>
        public void SetHeadingIndicatorParameters(int aircraftHeading)
        {
            Heading = aircraftHeading;

            this.Refresh();
        }

        #endregion
    }
}
