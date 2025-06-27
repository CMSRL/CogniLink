using System;
using System.Runtime.InteropServices;

public class RecordingPluginWrapper
{
    // Import native functions from the DLL
    [DllImport("RecordingPlugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool SetRecordingMaxBufferSize(int recorderId, int bufferSize);

    [DllImport("RecordingPlugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool SetSoundRecordingMaxBufferSize(int recorderId, int bufferSize);

    [DllImport("RecordingPlugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool SetReplayBufferNumber(int recorderId, int bufferNumber);

    [DllImport("RecordingPlugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool SetReplayBufferStoredTimeInterval(int recorderId, float timeInterval);

    [DllImport("RecordingPlugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool CreateNewRecordingFile(int recorderId, string directory, int directoryLength, string recordName, int recordNameLength);

    [DllImport("RecordingPlugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool RecordObjectAtTimestamp(int recorderId, string name, int nameLength, int id, float[] matrixDTO, float recordTime, int[] infoDTO);

    [DllImport("RecordingPlugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool RecordSoundDataAtTimestamp(int recorderId, float[] audioData, int recordedSamples, int sampleRate, int startIndex, float recordTime, int audioSourceId);

    [DllImport("RecordingPlugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool RecordGenericAtTimestamp(int recorderId, float recordTime, int genericInfoId, int[] intData, float[] floatData, char[] charData);

    [DllImport("RecordingPlugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool StopRecording(int recorderId);

    [DllImport("RecordingPlugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool OpenExistingRecordingFile(int recorderId, string directory, int directoryLength, string recordName, int recordNameLength);

    [DllImport("RecordingPlugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool GetTransformAndInformationAtTime(int recorderId, string name, int nameLength, int id, float loadTime, IntPtr matrixDTO, IntPtr infoDTO);

    [DllImport("RecordingPlugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool GetSoundChunkForTime(int recorderId, int soundSourceId, float loadTime, IntPtr soundDTO);

    [DllImport("RecordingPlugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool GetGenericAtTime(int recorderId, float loadTime, int id, IntPtr intData, IntPtr floatData, IntPtr charData);

    [DllImport("RecordingPlugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool StopReplay(int recorderId);

    


}
