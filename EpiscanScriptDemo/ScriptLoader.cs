using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSScriptLibrary;

namespace UserScriptHost
{
    public class ScriptLoader
    {
        public static dynamic Load(string filename)
        {
            CSScript.GlobalSettings.UseAlternativeCompiler = "CSSRoslynProvider.dll";
            Console.WriteLine(filename);
            dynamic script = CSScript.CodeDomEvaluator.LoadFile<IEpiscanScript>(filename);
            return script;
        }
    }
}
