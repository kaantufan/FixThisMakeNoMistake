#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
namespace TooLateToPanic
{
    // Explicit combined-candidate entry point. Reimport after editor script compilation,
    // then verify the supplied music import policy before invoking the existing builder.
    public static class PanicCombinedQaBuild
    {
        public static void Build()
        {
            foreach(string id in PanicBackgroundMusic.TrackIds)
            {
                string path="Assets/Game/Resources/Audio/Music/"+id+".ogg";
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
                var a=AssetImporter.GetAtPath(path) as AudioImporter;var s=a.defaultSampleSettings;
                if(s.loadType!=AudioClipLoadType.Streaming||s.compressionFormat!=AudioCompressionFormat.Vorbis||a.forceToMono||s.preloadAudioData||!a.loadInBackground||Math.Abs(s.quality-.72f)>.001f)
                    throw new Exception("Combined music importer mismatch: "+path);
                Debug.Log("COMBINED_MUSIC_IMPORT_PASS "+id+" Streaming/Vorbis/stereo/quality.72/preloadfalse/backgroundtrue");
            }
            PanicResidenceBuild.Build();
        }
    }
}
#endif
