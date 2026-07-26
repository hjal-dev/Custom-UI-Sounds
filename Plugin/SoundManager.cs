using EFT.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace CustomUISounds
{
    static class SoundManager
    {
        class Sound
        {
            public float[] data;
            public int channels;
            public int frequency;
            public AudioClip clip;
        }

        // All the sounds are loaded, one soundfile per name (like "ButtonClick", "BackpackOpen/Close"). This patch will read this.
        static Dictionary<EUISoundType, Sound> sounds = new Dictionary<EUISoundType, Sound>();

        // The file types it accepts and how Unity should read each one.
        static Dictionary<string, AudioType> fileTypes = new Dictionary<string, AudioType>
        {
            { ".wav", AudioType.WAV },
            { ".mp3", AudioType.MPEG },
            { ".ogg", AudioType.OGGVORBIS },
        };

        // The "Sounds" folder are next to the dll. Every folder inside it is a soundpack.
        public static string soundsFolder = Path.Combine(Path.GetDirectoryName(typeof(Plugin).Assembly.Location), "Sounds");

        // Reads every sound file in the selected soundpack. Any missing files will just get skipped and play the original soundfile instead of being completely silent.
        public static IEnumerator LoadPack(string pack)
        {
            sounds.Clear();

            if (pack == "Default (Vanilla)")
            {
                Plugin.Log.LogInfo("Using EFT own soundfiles.");
                yield break;
            }

            string folder = Path.Combine(soundsFolder, pack);
            if (!Directory.Exists(folder))
            {
                Plugin.Log.LogWarning("Can't find the soundpack folder: " + folder);
                yield break;
            }

            foreach (EUISoundType name in Enum.GetValues(typeof(EUISoundType)))
            {
                // Looking for example: "ButtonClick.wav", then "ButtonClick.mp3", then "ButtonClick.ogg" :D.
                string file = FindFile(folder, name.ToString());
                if (file == null)
                {
                    continue;
                }

                // Asking Unity very nice to plz read the file. It needs a web-style address, so this turns the path into one.
                AudioType type = fileTypes[Path.GetExtension(file).ToLower()];
                UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(new Uri(file).AbsoluteUri, type);
                ((DownloadHandlerAudioClip)request.downloadHandler).streamAudio = false;

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Plugin.Log.LogWarning("Couldn't read " + file + " (" + request.error + ")");
                    continue;
                }

                AudioClip loaded = DownloadHandlerAudioClip.GetContent(request);
                if (loaded == null || loaded.samples == 0)
                {
                    Plugin.Log.LogWarning("Couldn't understand the audio/incorrect fileformat in " + file);
                    request.Dispose();
                    continue;
                }

                float[] data = new float[loaded.samples * loaded.channels];
                loaded.GetData(data, 0);
                request.Dispose();

                sounds[name] = new Sound { data = data, channels = loaded.channels, frequency = loaded.frequency };
                Plugin.Log.LogInfo("Loaded " + Path.GetFileName(file));
            }

            Plugin.Log.LogInfo("Loaded " + sounds.Count + " sounds from " + pack);
        }

        public static AudioClip GetClip(EUISoundType name)
        {
            Sound sound;
            if (!sounds.TryGetValue(name, out sound))
            {
                return null;
            }

            // clip == null: it never built one. clip.samples == 0: the game will throw it away wiii.
            if (sound.clip == null || sound.clip.samples == 0)
            {
                sound.clip = AudioClip.Create(name.ToString(), sound.data.Length / sound.channels, sound.channels, sound.frequency, false);
                sound.clip.SetData(sound.data, 0);
            }

            return sound.clip;
        }

        static string FindFile(string folder, string name)
        {
            foreach (string extension in fileTypes.Keys)
            {
                string file = Path.Combine(folder, name + extension);
                if (File.Exists(file))
                {
                    return file;
                }
            }
            return null;
        }
    }
}
