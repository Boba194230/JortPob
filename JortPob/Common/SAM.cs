using Jint;
using NAudio.Wave;
using SoulsFormats;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

/* Uses Jint and SamJS to generate wav files of sam saying a string */
/* This exists for me to test if full voice acting will work properly before we get voice actors involved */
/* Porting the SamJS code directly to C# would be better than this but fuck man that's a lot of work */
namespace JortPob.Common
{
    public class SAM
    {
        private readonly Engine engine;

        public SAM()
        {
            string js = System.IO.File.ReadAllText(@"I:\Dev\sam\samjs.js");

            engine = new();
            engine.Execute(js);
            engine.Execute(@"var console = {}; console.log = function(text) {}; let sam = new SamJs();");
        }

        public string Generate(Dialog.DialogRecord dialog, Dialog.DialogInfoRecord info, string line, uint lineId, NpcContent npc)
        {
            // Get the exact location this file will be in
            string wavPath = $"{Const.CACHE_PATH}dialog\\{npc.race}\\{npc.sex}\\{dialog.id}\\{lineId}.wav";
            string wemPath = $"{Const.CACHE_PATH}dialog\\{npc.race}\\{npc.sex}\\{dialog.id}\\{lineId}.wem";

            // Check if this audio file exists in the cache already // @TODO: ideally we generate a voice cache later but guh w/e filesystem check for now
            if (System.IO.File.Exists(wemPath)) { return wemPath; }

            engine.Execute($"sam.download('{Sanitize(line)}');");
            Jint.Native.JsValue output = engine.GetValue("DATA_OUTPUT");
            byte[] data = output.AsArray()[0].AsUint8Array();


            // Make folder if doesn't exist (this is so ugly lmao)
            if (!System.IO.Directory.Exists($"{Const.CACHE_PATH}dialog\\{npc.race}\\{npc.sex}\\{dialog.id}")) { System.IO.Directory.CreateDirectory($"{Const.CACHE_PATH}dialog\\{npc.race}\\{npc.sex}\\{dialog.id}"); }

            // Convert from 22hz to 44hz and write to file
            using (MemoryStream inputStream = new MemoryStream(data))
            using (WaveFileReader reader = new WaveFileReader(inputStream))
            {
                // Define the target WaveFormat
                var outputFormat = new WaveFormat(441000, reader.WaveFormat.Channels);

                // Create the resampler
                using (var resampler = new MediaFoundationResampler(reader, outputFormat)
                {
                    ResamplerQuality = 60 // Highest quality
                })
                {
                    // Write the resampled audio to a new WAV file
                    WaveFileWriter.CreateWaveFile(wavPath, resampler);
                }
            }

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

            // Return wem path
            return wemPath;
        }

        /* Sanitize an input string for sam. Remove special characters and shit */
        private string Sanitize(string text)
        {
            string sani = text.Replace("'", "");
            sani = sani.Replace("\"", "");

            while(sani.Contains(". ."))
            {
                sani = sani.Replace(". .", "."); // long strings of periods cause issues idk
            }

            while (sani.Contains(".."))
            {
                sani = sani.Replace("..", "."); // long strings of periods cause issues idk
            }

            return sani;
        }
    }
}
