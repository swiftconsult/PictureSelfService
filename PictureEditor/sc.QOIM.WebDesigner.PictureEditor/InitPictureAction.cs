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
using VI.WebDesigner.Loader;
using VI.WebDesigner.Expression;
using VI.WebDesigner.Definition.Tables;
using System.Drawing;
using System.IO;

namespace sc.QOIM.WebDesigner.PictureEditor
{
    /// <summary>
    ///   The InitPictureAction class represents a specialized WebDesigner action
    ///   to scale images.
    /// </summary>
    public class InitPictureAction : VI.WebDesigner.Action.ActionBase
    {

        #region -- Fields --

#pragma warning disable 0649

        /// <summary>
        ///   A DataExpression that can be evaluated to a string 
        ///   specifying the name of the collection property that
        ///   holds the image byte array.
        /// </summary>
        [ParameterExpressionBinding("ImageColumn")]
        private DataExpression ImageColumn;

        /// <summary>
        ///   A DataExpression that can be evaluated to a string 
        ///   specifying the name of the collection property that
        ///   will receive the initial x-coordinate of the cropping area.
        /// </summary>
        [ParameterExpressionBinding("XColumn")]
        private DataExpression X;

        /// <summary>
        ///   A DataExpression that can be evaluated to a string 
        ///   specifying the name of the collection property that
        ///   will receive the initial y-coordinate of the cropping area.
        /// </summary>
        [ParameterExpressionBinding("YColumn")]
        private DataExpression Y;

        /// <summary>
        ///   A DataExpression that can be evaluated to a string 
        ///   specifying the name of the collection property that
        ///   will receive the initial width of the cropping area.
        /// </summary>
        [ParameterExpressionBinding("WidthColumn")]
        private DataExpression Width;

        /// <summary>
        ///   A DataExpression that can be evaluated to a string 
        ///   specifying the name of the collection property that
        ///   will receive the initial height of the cropping area.
        /// </summary>
        [ParameterExpressionBinding("HeightColumn")]
        private DataExpression Height;

        /// <summary>
        ///   A DataExpression that can be evaluated to a decimal value
        ///   specifying the aspect ratio of the cropping area.
        /// </summary>
        [ParameterExpressionBinding("AspectRatio")]
        private DataExpression AspectRatio;

        /// <summary>
        ///   A DataExpression that can be evaluated to an Int32 value 
        ///   specifying the maximum width of the image.
        /// </summary>
        [ParameterExpressionBinding("MaxWidth")]
        private DataExpression MaxWidth;

        /// <summary>
        ///   A BaseTableDefinition specifying the collection containing
        ///   the required properties.
        /// </summary>
        [DataTableBinding("Collection", true, true)]
        private BaseTableDefinition Table;

#pragma warning restore 0649

        private ImageConverter converter = new ImageConverter();

        #endregion


        #region ExecuteInternal

        /// <summary>
        ///   Executes the action represented by this class.
        /// </summary>
        /// <param name="executor">
        ///   An ActionExecutor object holding contextual information for execution.
        /// </param>
        protected override void ExecuteInternal(VI.WebDesigner.Runtime.ActionExecutor executor)
        {

            Image source = null;
            Image result = null;

            try
            {

                // get row for result
                var target = executor.GetCurrentRow(executor.GetTable(Table));

                // get image
                var image = target.GetValue(ImageColumn.EvaluateToString(executor));

                if (image == null || image.GetType() == typeof(System.DBNull)) return;

                if (image as byte[] != null && ((byte[])image).Length == 0) return;

                using (MemoryStream ms = new MemoryStream(image as byte[]))
                    source = Image.FromStream(ms);

                // prepare parameters
                Int32 maxWidth = Convert.ToInt32(MaxWidth.Evaluate(executor));
                Double ratio = Convert.ToDouble(AspectRatio.Evaluate(executor));

                // scale image
                result = converter.ScaleDown(source, maxWidth, (int)(maxWidth / ratio));

                // save changes (when different) 
                if (result != source)
                    using (var ms = new MemoryStream())
                    {
                        result.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        target.SetValue(ImageColumn.EvaluateToString(executor), ms.ToArray());
                    }

                // calculate crop area
                var originalRatio = result.Width / (double)result.Height;

                int x = 0, y = 0, w = result.Width, h = result.Height;

                if (originalRatio > ratio)
                {
                    var newWidth = (int)(h * ratio);
                    x = (w - newWidth) / 2;
                    w = newWidth;
                }
                else
                {
                    var newHeight = (int)(w / ratio);
                    y = (h - newHeight) / 2;
                    h = newHeight;
                }

                // return area definition
                target.SetValue(X.EvaluateToString(executor), x);
                target.SetValue(Y.EvaluateToString(executor), y);
                target.SetValue(Width.EvaluateToString(executor), w);
                target.SetValue(Height.EvaluateToString(executor), h);
            }
            catch (Exception ex)
            {
                throw new VI.Base.ViException(ex.ToString(), VI.Base.ExceptionRelevance.EndUser);
            }
            finally
            {
                if (source != null) source.Dispose();
                if (result != null) result.Dispose();
            }
        }

        #endregion
    }
}
