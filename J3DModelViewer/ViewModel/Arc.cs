using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J3DModelViewer.ViewModel
{
    public class Arc : IArc
    {
        public string ArcName { get; set; }
        List<IArc> IArc.BMD { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public List<IArc> BMD = new List<IArc>();
    }
}
