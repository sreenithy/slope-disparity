using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EpiscanUtil;

namespace UserScriptHost
{
    public interface IEpiscanScript
    {
        void Configure(MyEpiscan episcan);
        Task Run(MyEpiscan episcan);
    }
}
