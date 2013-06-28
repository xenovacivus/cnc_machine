using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Router
{
    public interface IHasRouts
    {
        List<Rout> GetRouts();
    }
}
