using System;
using System.Windows.Forms;

namespace Gear.GUI
{
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            DoubleBuffered = true;
        }
    }
}
