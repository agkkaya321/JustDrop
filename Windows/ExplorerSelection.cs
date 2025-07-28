using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

public class SelectionRequestedEventArgs : EventArgs
{
    public IReadOnlyList<string> Paths { get; }
    public SelectionRequestedEventArgs(IReadOnlyList<string> paths) => Paths = paths;
}

public class ExplorerSelectionService : IDisposable
{
    // Win32 constants
    const int WM_HOTKEY = 0x0312;
    const int MOD_ALT = 0x0001;
    const int VK_S = 0x53;
    const int HOTKEY_ID = 1;

    // P/Invoke
    [DllImport("user32.dll")] static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll")] static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    [DllImport("user32.dll")] static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMin, uint wMax);
    [DllImport("user32.dll")] static extern bool TranslateMessage([In] ref MSG lpMsg);
    [DllImport("user32.dll")] static extern IntPtr DispatchMessage([In] ref MSG lpMsg);
    [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();

    struct MSG { public IntPtr hwnd; public uint message; public UIntPtr wParam; public IntPtr lParam; public uint time; public POINT pt; }
    struct POINT { public int x, y; }

    private volatile bool _running;
    private Thread? _thread;

    /// <summary>
    /// Déclenché quand l'utilisateur appuie Alt+S. Contient la liste des chemins sélectionnés.
    /// </summary>
    public event EventHandler<SelectionRequestedEventArgs>? SelectionRequested;

    /// <summary>
    /// Démarre la boucle de surveillance dans un thread STA.
    /// </summary>
    public void Start()
    {
        if (_running) return;
        _running = true;
        _thread = new Thread(RunMessageLoop) { IsBackground = true };
        _thread.SetApartmentState(ApartmentState.STA);
        _thread.Start();
    }

    /// <summary>
    /// Arrête proprement la surveillance et la boucle de messages.
    /// </summary>
    public void Stop()
    {
        _running = false;
        PostThreadQuit();
        _thread?.Join();
        _thread = null;
    }

    private void RunMessageLoop()
    {
        if (!RegisterHotKey(IntPtr.Zero, HOTKEY_ID, MOD_ALT, VK_S))
            throw new InvalidOperationException("Impossible d'enregistrer Alt+S");

        try
        {
            while (_running && GetMessage(out var msg, IntPtr.Zero, 0, 0))
            {
                if (msg.message == WM_HOTKEY && (int)msg.wParam == HOTKEY_ID)
                {
                    var paths = GetSelectedPathsInExplorer();
                    SelectionRequested?.Invoke(this, new SelectionRequestedEventArgs(paths));
                }
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }
        finally
        {
            UnregisterHotKey(IntPtr.Zero, HOTKEY_ID);
        }
    }

    private static void PostThreadQuit()
    {
        const int WM_QUIT = 0x0012;
        PostThreadMessage(GetCurrentThreadId(), WM_QUIT, UIntPtr.Zero, IntPtr.Zero);
    }

    [DllImport("kernel32.dll")] static extern uint GetCurrentThreadId();
    [DllImport("user32.dll")] static extern bool PostThreadMessage(uint id, int msg, UIntPtr wParam, IntPtr lParam);

    private static List<string> GetSelectedPathsInExplorer()
    {
        var result = new List<string>();
        IntPtr hwnd = GetForegroundWindow();

        Type? shellType = Type.GetTypeFromProgID("Shell.Application");
        if (shellType is null) return result;

        dynamic shell = Activator.CreateInstance(shellType)!;

        foreach (dynamic window in shell.Windows())
        {
            IntPtr winHwnd = new IntPtr((long)window.HWND);
            if (winHwnd != hwnd) continue;

            dynamic doc = window.Document;
            if (doc is null) break;

            dynamic items = doc.SelectedItems();
            int count = (int)items.Count;

            for (int i = 0; i < count; i++)
            {
                dynamic item = items.Item(i);
                string? path = item.Path as string;
                if (!string.IsNullOrEmpty(path))
                    result.Add(path);
            }
            break;
        }
        return result;
    }

    public void Dispose() => Stop();
}
