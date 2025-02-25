using System;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class FileUtil
{
    public static AudioType GetAudioTypeByDirectory(string directory)
    {
        AudioType type = AudioType.UNKNOWN;
        if(directory.EndsWith(".ogg") || directory.EndsWith(".egg"))
        {
            type = AudioType.OGGVORBIS;
        }
        else if(directory.EndsWith(".wav"))
        {
            type = AudioType.WAV;
        }

        return type;
    }


    public static async Task<AudioClip> GetAudioFromFile(string path, AudioType type)
    {
        if(!File.Exists(path))
        {
            Debug.LogWarning("Trying to load audio from a nonexistant file!");
            return null;
        }

        AudioClip song = null;
        try
        {
        using(UnityWebRequest audioUwr = UnityWebRequestMultimedia.GetAudioClip(path, type))
        {
            Debug.Log("Loading audio file.");
            audioUwr.SendWebRequest();

            while(!audioUwr.isDone) await Task.Delay(5);
            
            if(audioUwr.result == UnityWebRequest.Result.ConnectionError || audioUwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning($"{audioUwr.error}");
                return song;
            }
            else
            {
                song = DownloadHandlerAudioClip.GetContent(audioUwr);
            }
        }
        }
        catch(Exception err)
        {
            Debug.LogWarning($"Audio loading failed with exception: {err.Message}, {err.StackTrace}");
        }

        return song;
    }


    public static byte[] StreamToBytes(Stream sourceStream)
    {
        using(var memoryStream = new MemoryStream())
        {
            sourceStream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }


    public static Task<Stream> ReadFileData(string path)
    {
        if(!File.Exists(path))
        {
            return null;
        }

        try
        {
            return Task.FromResult<Stream>(File.OpenRead(path));
        }
        catch(Exception e)
        {
            Debug.LogWarning($"Unable to load file at {path} with error: {e.Message}, {e.StackTrace}");
            return null;
        }
    }
}


public sealed class TempFile : IDisposable
{
    private string path;
    public TempFile() : this(System.IO.Path.GetTempFileName()) { }

    public TempFile(string path)
    {
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
        this.path = path;
    }
    public string Path
    {
        get
        {
            if (path == null) throw new ObjectDisposedException(GetType().Name);
            return path;
        }
    }
    ~TempFile() { Dispose(false); }
    public void Dispose() { Dispose(true); }
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            GC.SuppressFinalize(this);                
        }
        if (path != null)
        {
            try { File.Delete(path); }
            catch { } // best effort
            path = null;
        }
    }
}