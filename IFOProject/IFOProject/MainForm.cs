using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace IFOProject
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// Loaded package
        /// </summary>
        private Package package;
        /// <summary>
        /// Image drawing control
        /// </summary>
        private ImageCanvas patternImageView;
        /// <summary>
        /// Row profile plot
        /// </summary>
        private ProfileCanvas rowProfileView;
        /// <summary>
        /// Column profile plot
        /// </summary>
        private ProfileCanvas columnProfileView;
        /// <summary>
        /// Profiles position: X = column index, Y = row index
        /// </summary>
        private Location profilesPoint;
        /// <summary>
        /// Profiles plot height
        /// </summary>
        private const int profilesHeight = 128;
        /// <summary>
        /// Selected corner choises
        /// </summary>
        private enum Edge {topLeft, topRight, bottomLeft, bottomRight};
        /// <summary>
        /// Editing selection corner
        /// </summary>
        private Edge focusedInSelection;

        /// <summary>
        /// DoubleBuffered PictureBox for pattern image
        /// </summary>
        class ImageCanvas : PictureBox
        {
            /// <summary>
            /// Parent form
            /// </summary>
            private MainForm parent;
            /// <summary>
            /// True between MouseDown and MouseUp events
            /// </summary>
            private bool movePoint;
            /// <summary>
            /// Image width
            /// </summary>
            private int width;
            /// <summary>
            /// Image height
            /// </summary>
            private int height;

            /// <summary>
            /// Default contructor
            /// </summary>
            public ImageCanvas()
            {
                DoubleBuffered = true;
                movePoint = false;
                width = height = 0;
            }

            /// <summary>
            /// Sets parent form for accessing members
            /// </summary>
            /// <param name="parent">Parent form</param>
            public ImageCanvas(MainForm parent) : this()
            {
                this.parent = parent;
                GotFocus += new EventHandler(ImageCanvas_GotFocus);
                Paint += new PaintEventHandler(ImageCanvas_Paint);
                MouseDown += new MouseEventHandler(ImageCanvas_MouseDown);
                MouseMove += new MouseEventHandler(ImageCanvas_MouseMove);
                MouseUp += new MouseEventHandler(ImageCanvas_MouseUp);
            }

            /// <summary>
            /// Process arrows keys (with CTRL)
            /// </summary>
            /// <param name="e"></param>
            protected override void OnKeyDown(KeyEventArgs e)
            {
                Location profiles = parent.profilesPoint;
                int step = e.Control ? 15 : 1;
                if (e.KeyCode == Keys.Up && profiles.y >= step) profiles.y -= step;
                else if (e.KeyCode == Keys.Down && profiles.y < Height - step) profiles.y += step;
                else if (e.KeyCode == Keys.Left && profiles.x >= step) profiles.x -= step;
                else if (e.KeyCode == Keys.Right && profiles.x < Width - step) profiles.x += step;
                MoveProfilesPoint(profiles);
            }

            /// <summary>
            /// Moves profiles point to the mouse position
            /// </summary>
            /// <param name="newPoint">Location from the upper-left window corner</param>
            private void MoveProfilesPoint(Location newPoint)
            {
                parent.profilesPoint = newPoint - new Location(this.Location);
                parent.RefreshProfiles();
                Invalidate();
            }

            /// <summary>
            /// Mouse clicked
            /// </summary>
            private void ImageCanvas_MouseDown(object sender, MouseEventArgs e)
            {
                if (!parent.ProfilesSelected) return;
                movePoint = true;
                MoveProfilesPoint(new Location(e.Location));
            }

            /// <summary>
            /// Mouse moved
            /// </summary>
            private void ImageCanvas_MouseMove(object sender, MouseEventArgs e)
            {
                if (!parent.ProfilesSelected) return;
                if (movePoint) MoveProfilesPoint(new Location(e.Location));
            }

            /// <summary>
            /// Mouse released
            /// </summary>
            private void ImageCanvas_MouseUp(object sender, MouseEventArgs e)
            {
                if (!parent.ProfilesSelected) return;
                movePoint = false;
                MoveProfilesPoint(new Location(e.Location));
            }

            /// <summary>
            /// Drawing lines
            /// </summary>
            private void ImageCanvas_Paint(object sender, PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                if (parent.SelectionSelected)
                {
                    // draw selection with red margin
                    Rectangle selection = parent.package.CurrentPattern.Selection;
                    g.DrawRectangle(new Pen(Color.Red), selection);

                    // get currently editing selection point
                    if (parent.focusedInSelection == Edge.topLeft) parent.profilesPoint = new Location(selection.Location);
                    else if (parent.focusedInSelection == Edge.topRight) parent.profilesPoint = new Location(selection.X + selection.Width, selection.Y);
                    else if (parent.focusedInSelection == Edge.bottomLeft) parent.profilesPoint = new Location(selection.X, selection.Y + selection.Height);
                    else parent.profilesPoint = new Location(selection.X + selection.Width, selection.Y + selection.Height);

                    // set profiles view point to our point
                    parent.checkBoxProfiles.Checked = true;
                }
                if (parent.ProfilesSelected)
                {
                    Location profiles = parent.profilesPoint;
                    g.DrawLine(new Pen(Color.Yellow), new Point(profiles.x, 0), new Point(profiles.x, Height - 1));
                    g.DrawLine(new Pen(Color.Yellow), new Point(0, profiles.y), new Point(Width - 1, profiles.y));
                }
            }

            /// <summary>
            /// Redraw when focused
            /// </summary>
            private void ImageCanvas_GotFocus(object sender, EventArgs e)
            {
                Invalidate();
            }

            /// <summary>
            /// Sets image and size
            /// </summary>
            /// <param name="bitmap">Pattern image</param>
            public void SetImage(Bitmap bitmap)
            {
                Image = bitmap;
                Size = bitmap.Size;
                width = Width;
                height = Height;
            }

            public void Clear()
            {
                Image = null;
            }
        }

        /// <summary>
        /// DoubleBuffered PictureBox for profiles
        /// </summary>
        class ProfileCanvas : PictureBox
        {
            /// <summary>
            /// Distance between vertical lines
            /// </summary>
            private const int gridDistanceX = 50;
            /// <summary>
            /// Distance between horizontal lines
            /// </summary>
            private const int gridDistanceY = 32;

            /// <summary>
            /// Default contructor
            /// </summary>
            public ProfileCanvas()
            {
                DoubleBuffered = true;
            }

            /// <summary>
            /// Draws horizontal or vertical profile
            /// </summary>
            /// <param name="profile">Intensity values</param>
            /// <param name="row">True if row, false if column</param>
            public void DrawProfile(byte[] profile, bool row)
            {
                double scale = profilesHeight / (byte.MaxValue + 1.0);
                int length = profile.Length;
                Bitmap img = new Bitmap(length, profilesHeight);
                Graphics g = Graphics.FromImage(img);
                Point prev = new Point(0, profilesHeight - (int)(profile[0] * scale));
                for (int i = gridDistanceY; i < profilesHeight; i += gridDistanceY)
                    g.DrawLine(new Pen(Color.Black), new Point(0, i), new Point(length - 1, i));
                for (int i = 1; i < length; i++)
                {
                    if (i % 50 == 0) g.DrawLine(new Pen(Color.Black), new Point(i, 0), new Point(i, profilesHeight));
                    Point next = new Point(i, profilesHeight - (int)(profile[i] * scale));
                    g.DrawLine(new Pen(Color.Blue), prev, next);
                    prev = next;
                }
                if (!row) img.RotateFlip(RotateFlipType.Rotate90FlipNone);
                Image = img;
                Size = img.Size;
            }

            public void Clear()
            {
                Image = null;
                Invalidate();
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            package = new Package();
            patternImageView = new ImageCanvas(this);
            columnProfileView = new ProfileCanvas();
            rowProfileView = new ProfileCanvas();
            panelImage.Controls.Add(patternImageView);
            panelColumnProfile.Controls.Add(columnProfileView);
            panelRowProfile.Controls.Add(rowProfileView);
            MaximumSize = MinimumSize = Size;
            profilesPoint = new Location();
            RefreshAll();
        }

        /// <summary>
        /// Checks if profiles checkbox selected
        /// </summary>
        private bool ProfilesSelected { get { return checkBoxProfiles.Checked; } }

        /// <summary>
        /// Checks if smoothing checkbox selected
        /// </summary>
        private bool SmoothingSelected { get { return checkBoxUseSmoothed.Checked; } }

        /// <summary>
        /// Checks if selection checkbox selected
        /// </summary>
        private bool SelectionSelected { get { return checkBoxShowSelection.Checked; } }

        /// <summary>
        /// Checks if package is empty
        /// </summary>
        private bool Empty { get { return package.PatternsCount == 0; } }

        /// <summary>
        /// Adds image files to package
        /// </summary>
        private void AddPatterns()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;
                if (dialog.FileNames.Length == 1) package.Add(dialog.FileName);
                else package.Add(dialog.FileNames);
                Cursor.Current = Cursors.Arrow;
            }
            RefreshAll();
        }

        /// <summary>
        /// Refreshes all visual info in thw window
        /// </summary>
        private void RefreshAll()
        {
            RefreshImage();
            RefreshPackageInfo();
            RefreshProfiles();
            RefreshSmoothing();
            RefreshCalculating();
        }

        /// <summary>
        /// Refresh pattern image
        /// </summary>
        private void RefreshImage()
        {
            if (Empty) patternImageView.Clear();
            else
            {
                Cursor.Current = Cursors.WaitCursor;
                patternImageView.SetImage(package.CurrentPattern.Bitmap);
                Cursor.Current = Cursors.Arrow;
            }
        }

        /// <summary>
        /// Refresh package info
        /// </summary>
        private void RefreshPackageInfo()
        {
            if (Empty)
            {
                textBoxPatternPath.Text = "";
                labelPatternIndex.Text = "Current: 0/0";
                labelWidth.Text = "Width: 0";
                labelHeight.Text = "Height: 0";
            }
            else
            {
                textBoxPatternPath.Text = package.CurrentPattern.Name;
                labelPatternIndex.Text = string.Format("Current: {0}/{1}", package.CurrentIndex + 1, package.PatternsCount);
                labelWidth.Text = string.Format("Width: {0}", package.CurrentPattern.Width);
                labelHeight.Text = string.Format("Height: {0}", package.CurrentPattern.Height);
            }
        }

        /// <summary>
        /// Refresh profiles view
        /// </summary>
        private void RefreshProfiles()
        {
            // enable or disable profiles
            if (Empty) groupBoxProfiles.Enabled = false;
            else groupBoxProfiles.Enabled = true;

            if (Empty && ProfilesSelected)
            {
                checkBoxProfiles.Checked = false;
            }
            else if (ProfilesSelected)
            {
                Pattern current = package.CurrentPattern;
                patternImageView.Select();

                // draw profile views
                if (!profilesPoint.exists) profilesPoint = new Location(current.Width / 2, current.Height / 2);
                byte[] columnProfile = current.ColumnProfile(profilesPoint.x);
                byte[] rowProfile = current.RowProfile(profilesPoint.y);
                columnProfileView.DrawProfile(columnProfile, false);
                panelColumnProfile.Invalidate();
                rowProfileView.DrawProfile(rowProfile, true);

                // show numeric values
                labelColumnProfile.Text = string.Format("X : {0}", profilesPoint.x);
                labelRowProfile.Text = string.Format("Y : {0}", profilesPoint.y);
                labelIntensity1x1.Text = string.Format("1 x 1 : {0}", current.PointAverageIntensity(profilesPoint, 0));
                labelIntensity3x3.Text = string.Format("3 x 3 : {0}", current.PointAverageIntensity(profilesPoint, 1));
                labelIntensity5x5.Text = string.Format("5 x 5 : {0}", current.PointAverageIntensity(profilesPoint, 2));
            }
            else
            {
                // hide profile views
                columnProfileView.Clear();
                rowProfileView.Clear();
                patternImageView.Invalidate();

                // hide numeric values
                labelColumnProfile.Text = "X :";
                labelRowProfile.Text = "Y :";
                labelIntensity1x1.Text = "1 x 1 :";
                labelIntensity3x3.Text = "3 x 3 :";
                labelIntensity5x5.Text = "5 x 5 :";
            }
        }

        /// <summary>
        /// Refresh smoothing mode
        /// </summary>
        private void RefreshSmoothing()
        {
            progressBarSmoothing.Value = 0;
            if (Empty)
            {
                groupBoxSmoothing.Enabled = false;
                textBoxSmoothRadius.Text = "";
                checkBoxUseSmoothed.Checked = false;
            }
            else
            {
                groupBoxSmoothing.Enabled = true;
                Pattern current = package.CurrentPattern;
                textBoxSmoothRadius.Text = current.SmoothingRadius.ToString();
                checkBoxUseSmoothed.Checked = current.UseSmoothing;
            }
        }

        /// <summary>
        /// Refresh selection and rows step
        /// </summary>
        private void RefreshCalculating()
        {
            if (Empty) groupBoxCalculate.Enabled = false;
            else groupBoxCalculate.Enabled = true;
        }

        /// <summary>
        /// Moves to next pattern
        /// </summary>
        private void MoveToNext()
        {
            if (package.MoveNext()) RefreshAll();
        }

        /// <summary>
        /// Moves to previous pattern
        /// </summary>
        private void MoveToPrevious()
        {
            if (package.MovePrevious()) RefreshAll();
        }

        /// <summary>
        /// Removes current pattern from package
        /// </summary>
        private void RemovePattern()
        {
            if (package.Remove()) RefreshAll();
        }

        /// <summary>
        /// Removes all patterns from package
        /// </summary>
        private void ClearPackage()
        {
            if (package.Clear()) RefreshAll();
        }

        /// <summary>
        /// Smoothes current image
        /// </summary>
        /// <param name="radius">Smoothing radius</param>
        private void SmoothImage(int radius)
        {
            package.CurrentPattern.Smooth(radius, ref progressBarSmoothing);
        }

        /// <summary>
        /// Calculates results for all patterns in package and shows them in one window
        /// </summary>
        private void ShowPackageResults()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Add patterns button clicked
        /// </summary>
        private void buttonAddImages_Click(object sender, EventArgs e)
        {
            AddPatterns();
        }

        /// <summary>
        /// Moves focus to previous pattern
        /// </summary>
        private void buttonPreviousPattern_Click(object sender, EventArgs e)
        {
            MoveToPrevious();
        }

        /// <summary>
        /// Moves focus to next pattern
        /// </summary>
        private void buttonNextPattern_Click(object sender, EventArgs e)
        {
            MoveToNext();
        }

        /// <summary>
        /// Removes current pattern from package
        /// </summary>
        private void buttonRemove_Click(object sender, EventArgs e)
        {
            RemovePattern();
        }

        /// <summary>
        /// Clears package
        /// </summary>
        private void buttonClear_Click(object sender, EventArgs e)
        {
            ClearPackage();
        }

        /// <summary>
        /// Smooth image and show smoothed
        /// </summary>
        private void buttonSmooth_Click(object sender, EventArgs e)
        {
            if (Empty) return;
            try
            {
                SmoothImage((int)uint.Parse(textBoxSmoothRadius.Text));
                checkBoxUseSmoothed.Checked = true;
                RefreshAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Smoothing radius must be positive integer", "Error");
            }
        }

        /// <summary>
        /// Show or hide profiles view
        /// </summary>
        private void checkBoxProfiles_CheckedChanged(object sender, EventArgs e)
        {
            RefreshProfiles();
        }

        /// <summary>
        /// Use smoothed image or not
        /// </summary>
        private void checkBoxUseSmoothed_CheckedChanged(object sender, EventArgs e)
        {
            if ((Empty || package.CurrentPattern.ContainsSmoothed) && checkBoxUseSmoothed.Checked)
            {
                checkBoxUseSmoothed.Checked = false;
            }
            if (!Empty)
            {
                package.CurrentPattern.UseSmoothing = checkBoxUseSmoothed.Checked;
                RefreshAll();
            }
        }

        /// <summary>
        /// Change showing selection margin on image
        /// </summary>
        private void checkBoxShowSelection_CheckedChanged(object sender, EventArgs e)
        {
            if (Empty) return;
            Pattern current = package.CurrentPattern;
            if (current.Selection.IsEmpty) current.CreateDefaultSelection();
            RefreshImage();
        }
    }
}
