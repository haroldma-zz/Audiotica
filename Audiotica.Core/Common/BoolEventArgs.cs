using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audiotica.Core.Common
{
    public class BoolEventArgs : EventArgs
    {
        public BoolEventArgs(bool content)
        {
            Content = content;
        }

        public bool Content { get; private set; }
    }
}
