using System;
using System.IO;
using System.Linq;

namespace TooLateToPanic
{
    // Pure opt-in QA argument validator. No Unity API, directory creation or score access.
    // The companion World patch invokes this before constructing the production model.
    public static class PanicTestLaunchRules
    {
        public static readonly string[] Drivers = {
            "--critical-v513-smoke", "--presentation-v512-smoke", "--roof-wall-v511-smoke", "--onboarding-v510-smoke", "--v5-smoke", "--v51-smoke", "--v52-smoke", "--v53-smoke", "--v54-smoke", "--v55-smoke", "--v56-smoke", "--wave-ledger-v56-smoke", "--ui-v57-smoke", "--yarkin-hybrid-smoke",
            "--quake414-smoke", "--wave50-smoke", "--overhaul414-smoke", "--home-smoke", "--residence-smoke", "--refinement-smoke", "--bathroom-smoke",
            "--doors-stairs-smoke", "--interaction-smoke", "--door-jam-smoke", "--developer-smoke",
            "--hud-concept-smoke", "--jury-smoke", "--fullflow-smoke", "--direct-actions-smoke",
            "--interaction-audit", "--direct-matrix-audit", "--e2e-mechanics-audit", "--e2e-physics-audit",
            "--e2e-visual-atlas", "--e2e-story-audio-audit", "--e2e-system-audit", "--visual-text-smoke", "--audio-smoke", "--revision-smoke", "--revision412-smoke"
        };
        public static bool Validate(string[] args, Func<string,bool> fileExists, out string reason)
        {
            reason=string.Empty;
            if(args==null)args=Array.Empty<string>();
            int drivers=args.Count(a=>Drivers.Contains(a));
            if(drivers==0)return true; // Normal launches and normal evidence fixtures retain their policy.
            if(args.Contains("--os-e2e-observe")){reason="OS observer cannot share a gameplay test driver.";return false;}
            if(drivers!=1){reason="QA requires exactly one gameplay driver; observers are separate.";return false;}
            if(args.Count(a=>a=="--home-evidence")!=1){reason="QA requires one --home-evidence argument before model construction.";return false;}
            int at=Array.IndexOf(args,"--home-evidence");
            if(at+1>=args.Length||string.IsNullOrWhiteSpace(args[at+1])||args[at+1].StartsWith("--",StringComparison.Ordinal))
            {reason="QA requires an evidence directory value.";return false;}
            try
            {
                string directory=Path.GetFullPath(args[at+1]);
                string profile=Path.GetFileName(directory.TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar));
                Guid id;
                if(profile.Length<36||!Guid.TryParseExact(profile.Substring(profile.Length-36),"D",out id))
                {reason="QA requires a fresh GUID profile directory.";return false;}
                if(fileExists(Path.Combine(directory,"isolated-home-scores.json")))
                {reason="QA refuses an existing isolated score fixture; use a new GUID.";return false;}
            }
            catch(Exception e){reason="Invalid QA evidence directory: "+e.GetType().Name;return false;}
            return true;
        }
    }
}
