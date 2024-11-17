using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J3DModelViewer.ViewModel
{
    public interface IArc
    {
        string ArcName { get; }
        List<IArc> BMD { get; set; }
    }
}
