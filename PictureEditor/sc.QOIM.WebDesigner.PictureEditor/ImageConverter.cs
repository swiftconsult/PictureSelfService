/*	  
 *    Copyright (C) 2014  swift.consult GmbH
 *
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU Lesser General Public License (LGPL) as 
 *    published by the Free Software Foundation, either version 3 of the 
 *    License, or (at your option) any later version.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    and a copy of the GNU Lesser General Public License along with this 
 *    program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace sc.QOIM.WebDesigner.PictureEditor
{
    /// <summary>
    ///   The ImageConverter class provides functions for simple image manipulation.
    /// </summary>
    public class ImageConverter
    {

        #region ScaleDown

        /// <summary>
        ///   Sclaes an image to fit maximum width and height while keeping the
        ///   aspect ratio of the source image.
        /// </summary>
        /// <param name="source">
        ///   Specifies the image to scale.
        /// </param>
        /// <param name="maxWidth">
        ///   Specifies the maximum width of the result image in pixels.
        /// </param>
        /// <param name="maxHeight">
        ///   Specifies the maximum height of the result image in pixels.
        /// </param>
        /// <returns>
        ///   A new sclaed bitmap of the source image when this exceeeds at least 
        ///   one of the specified maximum values; otherwise the source image.
        /// </returns>
        public Image ScaleDown(Image source, int maxWidth, int maxHeight)
        {
            if (source == null) return null;

            if (source.Width <= maxWidth && source.Height <= maxHeight) return source;

            int newWidth = source.Width, newHeight = source.Height;

            if (newWidth > maxWidth)
            {
                newWidth = maxWidth;
                newHeight = (int)(source.Height / (double)source.Width * maxWidth);
            }
            if (newHeight > maxHeight)
            {
                newHeight = maxHeight;
                newWidth = (int)(source.Width / (double)source.Height * maxHeight);
            }

            return new Bitmap(source, newWidth, newHeight);
        }

        #endregion

        #region Transform

        /// <summary>
        ///   Adjustes the brightness, contrast and saturation of the given image.
        /// </summary>
        /// <param name="source">
        ///   Specifiers the source image to manipulate.
        /// </param>
        /// <param name="brightness">
        ///   Specifies the new brightness factor. Typically between -1 and 1. 0 (zero) yields no change.
        ///  </param>
        /// <param name="contrast">
        ///   Specifies the new contrast factor. Typically between 0 and 2. 1 yield no change.
        /// </param>
        /// <param name="saturation">
        ///   Specifies the new saturation factor. Typically between 0 and 2. 1 yield no change.
        /// </param>
        /// <param name="sourceRect">
        ///   A Rectangle specifying the area of the image to crop.
        /// </param>
        /// <param name="destinationRect">
        ///   An optional rectangle to specify destintion extends (the image will be scaled to fit this rectangle when not Rectangle.Empty).
        /// </param>
        /// <returns>
        ///   A new Bitmap object containing the transformed area of the source image. Caller is responsible for properly disposing said object.
        /// </returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")] // ownership is transfered when the object is returned
		public Image Transform(Image source, float brightness, float contrast, float saturation, Rectangle sourceRect, Rectangle destinationRect)
        {
            int width = source.Width;
            int height = source.Height;

			using (ImageAttributes imageAttr = new ImageAttributes()) {
				var x = new ColorMatrix();

				Img.QColorMatrix qm = new Img.QColorMatrix();
				qm.SetBrightness(brightness);
				qm.SetContrast(contrast);
				qm.SetSaturation(saturation);

				imageAttr.SetColorMatrix(qm.ToColorMatrix());

				if (destinationRect == Rectangle.Empty)
					destinationRect = new Rectangle(0, 0, sourceRect.Width, sourceRect.Height);

				var b = new Bitmap(destinationRect.Width, destinationRect.Height);

				using (Graphics g = Graphics.FromImage(b))
					g.DrawImage(source, destinationRect, sourceRect.Left, sourceRect.Top, sourceRect.Width, sourceRect.Height, GraphicsUnit.Pixel, imageAttr);
				
				return b;
			}
        }

        #endregion

    }
}
