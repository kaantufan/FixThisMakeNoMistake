#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TooLateToPanic
{
    public sealed class PanicMusicImport : AssetPostprocessor
    {
        void OnPreprocessAudio()
        {
            if(!assetPath.StartsWith("Assets/Game/Resources/Audio/Music/"))return;
            var importer=(AudioImporter)assetImporter;
            var settings=importer.defaultSampleSettings;
            settings.loadType=AudioClipLoadType.Streaming;
            settings.compressionFormat=AudioCompressionFormat.Vorbis;
            settings.quality=.72f;settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;
            settings.preloadAudioData=false;
            importer.defaultSampleSettings=settings;
            importer.forceToMono=false;importer.loadInBackground=true;
        }
    }
}
#endif
