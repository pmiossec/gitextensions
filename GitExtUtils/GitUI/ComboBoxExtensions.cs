using System;
using System.Windows.Forms;
using GitExtUtils.GitUI;

namespace GitUI
{
    public static class ComboBoxExtensions
    {
        public static void AdjustWidthToFitContent(this ComboBox comboBox)
        {
            if (comboBox is null)
            {
                throw new ArgumentNullException(nameof(comboBox));
            }

            var width = GetPreferredDropDownWidth(comboBox);

            comboBox.Width = width;

            if (width != 0)
            {
                comboBox.DropDownWidth = width;
            }
        }

        public static void ResizeDropDownWidth(this ComboBox comboBox, int minWidth, int maxWidth)
        {
            if (comboBox is null)
            {
                throw new ArgumentNullException(nameof(comboBox));
            }

            comboBox.DropDownWidth = GetDropDownWidth(comboBox, minWidth, maxWidth);
        }

        private static int GetDropDownWidth(dynamic control, int minWidth, int maxWidth)
        {
            var calculatedWidth = GetPreferredDropDownWidth(control);
            return Math.Min(Math.Max(calculatedWidth, DpiUtil.Scale(minWidth)), DpiUtil.Scale(maxWidth));
        }

        public static void ResizeDropDownWidth(this ToolStripComboBox comboBox, int minWidth, int maxWidth)
        {
            if (comboBox is null)
            {
                throw new ArgumentNullException(nameof(comboBox));
            }

            comboBox.DropDownWidth = GetDropDownWidth(comboBox.Control, minWidth, maxWidth);
        }

        private static int GetPreferredDropDownWidth(dynamic comboBox)
        {
            var calculatedWidth = 0;
            using var graphics = comboBox.CreateGraphics();
            foreach (object obj in comboBox.Items)
            {
                var area = graphics.MeasureString(obj.ToString(), comboBox.Font);
                calculatedWidth = Math.Max((int)area.Width, calculatedWidth);
            }

            return calculatedWidth;
        }
    }
}
