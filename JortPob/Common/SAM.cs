using SoulsFormats;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Text;

/* This exists for me to test if full voice acting will work properly before we get voice actors involved */
namespace JortPob.Common
{
    public class SAM
    {
        private readonly SpeechSynthesizer synthesizer;

        public SAM()
        {
            synthesizer = new SpeechSynthesizer();
        }

        public string Generate(Dialog.DialogRecord dialog, Dialog.DialogInfoRecord info, string line, string hashName, NpcContent npc)
        {
            // Get the exact location this file will be in
            string wavPath = $"{Const.CACHE_PATH}dialog\\{npc.race}\\{npc.sex}\\{dialog.id}\\{hashName}.wav";
            string wemPath = $"{Const.CACHE_PATH}dialog\\{npc.race}\\{npc.sex}\\{dialog.id}\\{hashName}.wem";

            try
            {
                // Check if this audio file exists in the cache already // @TODO: ideally we generate a voice cache later but guh w/e filesystem check for now
                if (System.IO.File.Exists(wemPath)) { return wemPath; }

                if (npc.sex == NpcContent.Sex.Female) { synthesizer.SelectVoice("Microsoft Zira Desktop"); }
                else { synthesizer.SelectVoice("Microsoft David Desktop"); }

                // Make folder if doesn't exist (this is so ugly lmao)
                if (!System.IO.Directory.Exists($"{Const.CACHE_PATH}dialog\\{npc.race}\\{npc.sex}\\{dialog.id}")) { System.IO.Directory.CreateDirectory($"{Const.CACHE_PATH}dialog\\{npc.race}\\{npc.sex}\\{dialog.id}"); }

                // Write 32bit 44100hz wav file (required format for wem)
                synthesizer.SetOutputToWaveFile(wavPath, new SpeechAudioFormatInfo(44100, AudioBitsPerSample.Sixteen, AudioChannel.Mono));
                synthesizer.Speak(line);

                // Convert wav to wem
                while (!System.IO.File.Exists(wemPath))  // @TODO: THIS IS A CURSED LINE OF CODE BUT HOLY SHIT THIS THING JUST WONT DO ITS JOB
                {
                    string pythonPath = @"C:\Users\dmtin\AppData\Local\Programs\Python\Python313\python.exe";
                    string scriptPath = $"\"{Utility.ResourcePath(@"tools\WemConversionWrapper\wem_conversion_wrapper.py")}\"";
                    ProcessStartInfo startInfo = new(pythonPath, $"{scriptPath} \"{wavPath}\"")
                    {
                        WorkingDirectory = Utility.ResourcePath(@"tools\WemConversionWrapper"),
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    var process = Process.Start(startInfo);
                    process.WaitForExit();
                }
            }
            catch
            {
                Lort.Log($"## ERROR ## Faile to generate dialog {wavPath}", Lort.Type.Debug);
            }

            // Return wem path
            return wemPath;
        }

        public void Dispose()
        {
            synthesizer.Dispose();
        }
    }
}
