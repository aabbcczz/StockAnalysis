using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Util;
using System.Windows.Forms;

namespace TradingClient
{
    /// <summary>
    /// Displays messages to text box
    /// </summary>
    public class TextBoxAppender : AppenderSkeleton
    {
        private object _syncObj = new object();
        private TextBox _textBox = null;

        protected override bool RequiresLayout { get { return true; } }

        public TextBoxAppender()
            : base()
        {
        }

        public void SetControl(TextBox textBox)
        {
            lock (_syncObj)
            {
                _textBox = textBox;
            }
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (_textBox == null)
            {
                lock(_syncObj)
                {
                    if (_textBox == null)
                    {
                        return;
                    }
                }
            }

            string message = RenderLoggingEvent(loggingEvent);

            _textBox.AppendText(message);
        }
    }
}
