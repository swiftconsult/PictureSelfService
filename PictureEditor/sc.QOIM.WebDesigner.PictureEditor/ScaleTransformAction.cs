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
using System.Drawing;

namespace sc.QOIM.WebDesigner.PictureEditor
{
    public class ScaleTransformAction : TransformAction
    {

        #region -- Fields --

#pragma warning disable 0649

        /// <summary>
        ///   A DataExpression that can be evaluated to an Int32
        ///   specifying the x-coordinate of the croped area.
        /// </summary>
        [ParameterExpressionBinding("XPos")]
        private DataExpression X;

        /// <summary>
        ///   A DataExpression that can be evaluated to an Int32
        ///   specifying the y-coordinate of the croped area.
        /// </summary>
        [ParameterExpressionBinding("YPos")]
        private DataExpression Y;

        /// <summary>
        ///   A DataExpression that can be evaluated to an Int32
        ///   specifying the width of the croped area.
        /// </summary>
        [ParameterExpressionBinding("Width")]
        private DataExpression Width;

        /// <summary>
        ///   A DataExpression that can be evaluated to an Int32
        ///   specifying the height of the croped area.
        /// </summary>
        [ParameterExpressionBinding("Height")]
        private DataExpression Height;

        /// <summary>
        ///   A DataExpression that can be evaluated to a decimal value
        ///   specifying the aspect ratio of the cropping area.
        /// </summary>
        [ParameterExpressionBinding("AspectRatio")]
        private DataExpression AspectRatio;

        /// <summary>
        ///   A DataExpression that can be evaluated to an Int32 value 
        ///   specifying the fixed width of the target image.
        /// </summary>
        [ParameterExpressionBinding("StoreWidth")]
        private DataExpression StoreWidth;


#pragma warning restore 0649

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
                source = GetSource(executor);
                Double ratio = Convert.ToDouble(AspectRatio.Evaluate(executor));

                if (source == null)
                {
                    SetResult(executor, null);
                    return;
                }

                // prepare parameters
                int brightness = TryConvertToInt32(executor, Brightness);
                int contrast = TryConvertToInt32(executor, Contrast, 100);
                int saturation = TryConvertToInt32(executor, Saturation, 100);
                int x = TryConvertToInt32(executor, X);
                int y = TryConvertToInt32(executor, Y);
                int width = TryConvertToInt32(executor, Width);
                int height = TryConvertToInt32(executor, Height);

                int destWidth = TryConvertToInt32(executor, StoreWidth);
                int destHeight = (int)(destWidth / ratio);

                // convert image
                result = converter.Transform(
                    source,
                    brightness / 100F,
                    contrast / 100F,
                    saturation / 100F,
                    new Rectangle(x, y, width, height),
                    new Rectangle(0, 0, destWidth, destHeight));

                // return result
                SetResult(executor, result);
            }
            catch (Exception ex)
            {
                VI.Base.AppData.Instance.RaiseMessage(VI.Base.MsgSeverity.Serious, ex.ToString());
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
