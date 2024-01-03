using System;
using System.Diagnostics;
using System.Threading.Tasks;

public static class ProcessExtensions
{
    public static Task<int> WaitForExitAsync(this Process process)
    {
        var tcs = new TaskCompletionSource<int>();
        process.EnableRaisingEvents = true;
        process.Exited += (sender, args) => tcs.TrySetResult(process.ExitCode);
        return tcs.Task;
    }
}
