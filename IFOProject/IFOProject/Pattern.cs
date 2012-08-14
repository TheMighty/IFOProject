using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace IFOProject
{
    public class Pattern
    {
        /// <summary>
        /// Intensity matrix
        /// </summary>
        private byte[,] matrix;
        /// <summary>
        /// Pattern width
        /// </summary>
        private int width;
        /// <summary>
        /// Pattern height
        /// </summary>
        private int height;
        /// <summary>
        /// Unsmoothed backup matrix
        /// </summary>
        private byte[,] backup;
        /// <summary>
        /// Image file path
        /// </summary>
        private string path;
        /// <summary>
        /// Calculation results
        /// </summary>
        private List<RowInfo> results;
        /// <summary>
        /// Smoothed intensity matrix
        /// </summary>
        private byte[,] smoothed;
        /// <summary>
        /// Return normal or smoothed image
        /// </summary>
        private bool useSmoothed;
        /// <summary>
        /// Smoothing radius for smoothed image
        /// </summary>
        private int smoothingRadius;
        /// <summary>
        /// Selection rectangle for calculating
        /// </summary>
        private Rectangle selection;

        /// <summary>
        /// Reads pattern from image file
        /// </summary>
        /// <param name="fileName"></param>
        public Pattern(string fileName)
        {
            path = fileName;
            Bitmap bmp = new Bitmap(path);
            width = bmp.Width;
            height = bmp.Height;
            matrix = new byte[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    matrix[x, y] = bmp.GetPixel(x, y).R;
        }

        /// <summary>
        /// Image file name
        /// </summary>
        public string Name { get { return Path.GetFileName(path); } }

        /// <summary>
        /// Image width
        /// </summary>
        public int Width { get { return width; } }

        /// <summary>
        /// Image height
        /// </summary>
        public int Height { get { return height; } }

        /// <summary>
        /// True for smoothed, false for normal mode
        /// </summary>
        public bool UseSmoothing
        {
            get { return useSmoothed; }
            set { useSmoothed = value; }
        }

        /// <summary>
        /// Gets value indicating whether the image was smoothed
        /// </summary>
        public bool ContainsSmoothed { get { return smoothed == null; } }

        /// <summary>
        /// Gets smoothing radius for smoothed image
        /// </summary>
        public int SmoothingRadius { get { return smoothingRadius; } }

        /// <summary>
        /// Gets or sets selection rectangle for pattern calculations
        /// </summary>
        public Rectangle Selection
        {
            get { return selection; }
            set { selection = value; }
        }

        /// <summary>
        /// Pattern image
        /// </summary>
        public Bitmap Bitmap
        {
            get
            {
                if (useSmoothed) return SmoothedImage;
                else return NormalImage;
            }
        }

        /// <summary>
        /// Unsmoothed image
        /// </summary>
        private Bitmap NormalImage
        {
            get
            {
                Bitmap bmp = new Bitmap(width, height);
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        byte current = matrix[x, y];
                        bmp.SetPixel(x, y, Color.FromArgb(current, current, current));
                    }
                }
                return bmp;
            }
        }

        /// <summary>
        /// Smoothed image
        /// </summary>
        private Bitmap SmoothedImage
        {
            get
            {
                Bitmap bmp = new Bitmap(width, height);
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        byte current = smoothed[x, y];
                        bmp.SetPixel(x, y, Color.FromArgb(current, current, current));
                    }
                }
                return bmp;
            }
        }

        /// <summary>
        /// Gets column intensity vector
        /// </summary>
        /// <param name="column">Column index</param>
        public byte[] ColumnProfile(int column)
        {
            byte[] result = new byte[height];
            for (int i = 0; i < height; i++) result[i] = useSmoothed ? smoothed[column, i] : matrix[column, i];
            return result;
        }

        /// <summary>
        /// Gets row intensity vector
        /// </summary>
        /// <param name="row">Row index</param>
        public byte[] RowProfile(int row)
        {
            byte[] result = new byte[width];
            for (int i = 0; i < width; i++) result[i] = useSmoothed ? smoothed[i, row] : matrix[i, row];
            return result;
        }

        /// <summary>
        /// Gets average intensity value for square with 2*radius+1 edge and center in point
        /// </summary>
        /// <param name="point">Center point of square</param>
        /// <param name="radius">Distance from center to edge</param>
        public byte PointAverageIntensity(Location point, int radius)
        {
            int sum = 0, count = 0;
            for (int x = point.x - radius; x <= point.x + radius; x++)
            {
                if (x < 0 || x >= width) continue;
                for (int y = point.y - radius; y <= point.y + radius; y++)
                {
                    if (y < 0 || y >= height) continue;
                    sum += useSmoothed ? smoothed[x, y] : matrix[x, y];
                    count++;
                }
            }
            return (byte)(sum / count);
        }

        /// <summary>
        /// Smoothes every point for square with 2*radius+1 edge
        /// </summary>
        /// <param name="radius">Smooth radius</param>
        public void Smooth(int radius, ref ProgressBar progress)
        {
            smoothed = new byte[width, height];
            smoothingRadius = radius;

            progress.Value = 0;
            progress.Maximum = width * height;

            for (int centerX = 0; centerX < width; centerX++)
            {
                for (int centerY = 0; centerY < height; centerY++)
                {
                    int sum = 0, count = 0;
                    for (int x = centerX - radius; x < centerX + radius; x++)
                    {
                        if (x < 0 || x >= width) continue;
                        for (int y = centerY - radius; y < centerY + radius; y++)
                        {
                            if (y < 0 || y >= height) continue;
                            sum += matrix[x, y];
                            count++;
                        }
                    }
                    smoothed[centerX, centerY] = (byte)(sum / count);
                    progress.Value++;
                }
            }
        }

        /// <summary>
        /// Initializes default selection rectangle
        /// </summary>
        public void CreateDefaultSelection()
        {
            selection = new Rectangle(width / 4, height / 4, width / 2, height / 2);
        }

        /// <summary>
        /// Calculates and saves every row's results
        /// </summary>
        public void Calculate()
        {
            throw new System.NotImplementedException();
        }
    }
}
