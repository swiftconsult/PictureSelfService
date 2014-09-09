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
using VI.WebDesigner.Runtime;

namespace sc.QOIM.WebDesigner.PictureEditor
{
    /// <summary>
    ///   The TransformAction class represents a specialized WebDesigner action
    ///   to manipulate images.
    /// </summary>
    public class TransformAction : VI.WebDesigner.Action.ActionBase
    {

        #region -- Fields --

        /// <summary>
        ///   A DataExpression that can be evaluated to a byte array
        ///   holding the source image.
        /// </summary>
        [ParameterExpressionBinding("InImage")]
        protected DataExpression UploadedImage;


        /// <summary>
        ///   A DataExpression that can be evaluated to an Int32
        ///   specifying the brightness of the result image.
        /// </summary>
        [ParameterExpressionBinding("Brightness")]
        protected DataExpression Brightness;

        /// <summary>
        ///   A DataExpression that can be evaluated to an Int32
        ///   specifying the contrast of the result image.
        /// </summary>
        [ParameterExpressionBinding("Contrast")]
        protected DataExpression Contrast;

        /// <summary>
        ///   A DataExpression that can be evaluated to an Int32
        ///   specifying the saturation of the result image.
        /// </summary>
        [ParameterExpressionBinding("Saturation")]
        protected DataExpression Saturation;

        /// <summary>
        ///   A DataExpression that can be evaluated to a string 
        ///   specifying the name of the collection property that
        ///   will receive the byte array containing the result image.
        /// </summary>
        [ParameterExpressionBinding("OutImage")]
        protected DataExpression FinalJpeg;

        /// <summary>
        ///   A BaseTableDefinition specifying the collection containing
        ///   the OutImage/FimalJpeg property.
        /// </summary>
        [DataTableBinding("TargetCollection", true, true)]
        protected BaseTableDefinition TargetTable;


        protected ImageConverter converter = new ImageConverter();

        #endregion

        protected void SetResult(ActionExecutor executor, Image value)
        {
            var target = executor.GetCurrentRow(executor.GetTable(TargetTable));

            if (value == null)
                target.SetValue(FinalJpeg.EvaluateToString(executor), value);
            else
            {
                using (var ms = new MemoryStream())
                {
                    value.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    target.SetValue(FinalJpeg.EvaluateToString(executor), ms.ToArray());
                }
            }
        }

        protected Image GetSource(ActionExecutor executor)
        {
            // get source image
            var image = UploadedImage.Evaluate(executor);

            try
            {
                using (MemoryStream ms = new MemoryStream(image as byte[]))
                    return Image.FromStream(ms);
            }
            catch (Exception ex)
            {
                VI.Base.AppData.Instance.RaiseMessage(VI.Base.MsgSeverity.Warning, ex.ToString());
                return null;
            }
        }

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

                source = GetSource(executor);

                if (source == null)
                {
                    SetResult(executor, null);
                    return;
                }

                // prepare parameters
                int brightness = TryConvertToInt32(executor, Brightness);
                int contrast = TryConvertToInt32(executor, Contrast, 100);
                int saturation = TryConvertToInt32(executor, Saturation, 100);


                // convert image
                result = converter.Transform(
                    source,
                    brightness / 100F,
                    contrast / 100F,
                    saturation / 100F,
                    new Rectangle(0, 0, source.Width, source.Height),
                    Rectangle.Empty);

                // return result
                SetResult(executor, result);
            }
            catch (Exception ex)
            {
                VI.Base.AppData.Instance.RaiseMessage(VI.Base.MsgSeverity.Warning, ex.ToString());
            }
            finally
            {
                if (source != null) source.Dispose();
                if (result != null) result.Dispose();
            }
        }

        #endregion

        #region TryConvert

        /// <summary>
        ///   Tries to convert the value described by the given DataExpression to an
        ///   Int32 value.
        /// </summary>
        /// <param name="executor">
        ///   Specifies an ActionExecutor that is used to evaluate the given expression.
        /// </param>
        /// <param name="property">
        ///   Specifies the property to be evaluated.
        /// </param>
        /// <param name="value">
        ///   Receives the result value when the conversion succeeds.
        /// </param>
        /// <returns>
        ///   True, when the conversion was successfull; otherwise false.
        /// </returns>
        protected Int32 TryConvertToInt32(VI.WebDesigner.Runtime.ActionExecutor executor, DataExpression property, Int32 defaultValue = 0)
        {
            try
            {
                return Convert.ToInt32(property.Evaluate(executor));
            }
            catch (Exception ex)
            {
                VI.Base.AppData.Instance.RaiseMessage(VI.Base.MsgSeverity.Warning, ex.ToString());
            }
            return defaultValue;
        }

        #endregion

    }
}
