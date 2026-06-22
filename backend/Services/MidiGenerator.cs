using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace backend.Services;

public class MidiGenerator(IHostEnvironment environment, IConfiguration configuration, ILogger<MidiGenerator> logger)
{
    private static readonly string[] DefaultSoundFontCandidates =
    [
        "/usr/share/sounds/sf2/FluidR3_GM.sf2",
        "/usr/share/soundfonts/FluidR3_GM.sf2",
    ];

    private readonly string _cacheDirectory = Path.Combine(environment.ContentRootPath, "midi-cache");
    private readonly SemaphoreSlim _generationLock = new(1, 1);

    public static string BuildMidiUrl(string seed, int index, string genre, string language)
        => BuildTrackUrl("midi", seed, index, genre, language);

    public static string BuildPreviewUrl(string seed, int index, string genre, string language)
        => BuildTrackUrl("preview", seed, index, genre, language);

    private static string BuildTrackUrl(string endpoint, string seed, int index, string genre, string language)
    {
        return string.Concat(
            "/api/generate/",
            endpoint,
            "?seed=",
            Uri.EscapeDataString(seed),
            "&index=",
            index,
            "&genre=",
            Uri.EscapeDataString(genre),
            "&lang=",
            Uri.EscapeDataString(language));
    }

    public static int ComputeRngSeed(ulong seed, int index, string language, string genre)
    {
        return HashCode.Combine(
            unchecked((int)(seed & uint.MaxValue)),
            unchecked((int)(seed >> 32)),
            index,
            SongLexicons.LanguageSeed(language),
            StringComparer.Ordinal.GetHashCode(genre));
    }

    public Task<byte[]?> TryGenerateMidiAsync(
        ulong seed,
        int index,
        string genre,
        string language,
        CancellationToken cancellationToken)
        => TryGetCachedFileAsync(seed, index, genre, language, ".mid", cancellationToken);

    public async Task<byte[]?> TryGeneratePreviewAsync(
        ulong seed,
        int index,
        string genre,
        string language,
        CancellationToken cancellationToken)
    {
        var midiPath = await EnsureMidiFileAsync(seed, index, genre, language, cancellationToken);
        if (midiPath is null)
        {
            return null;
        }

        var wavPath = GetCachePath(ComputeRngSeed(seed, index, language, genre), genre, ".wav");
        if (File.Exists(wavPath))
        {
            return await File.ReadAllBytesAsync(wavPath, cancellationToken);
        }

        await _generationLock.WaitAsync(cancellationToken);
        try
        {
            if (File.Exists(wavPath))
            {
                return await File.ReadAllBytesAsync(wavPath, cancellationToken);
            }

            var encoded = await RunFluidSynthAsync(midiPath, wavPath, cancellationToken);
            if (encoded is null)
            {
                return null;
            }

            return await File.ReadAllBytesAsync(wavPath, cancellationToken);
        }
        finally
        {
            _generationLock.Release();
        }
    }

    private async Task<byte[]?> TryGetCachedFileAsync(
        ulong seed,
        int index,
        string genre,
        string language,
        string extension,
        CancellationToken cancellationToken)
    {
        if (index < 1)
        {
            return null;
        }

        var cachePath = GetCachePath(ComputeRngSeed(seed, index, language, genre), genre, extension);
        if (extension == ".mid")
        {
            var ensured = await EnsureMidiFileAsync(seed, index, genre, language, cancellationToken);
            return ensured is null ? null : await File.ReadAllBytesAsync(ensured, cancellationToken);
        }

        if (!File.Exists(cachePath))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(cachePath, cancellationToken);
    }

    private async Task<string?> EnsureMidiFileAsync(
        ulong seed,
        int index,
        string genre,
        string language,
        CancellationToken cancellationToken)
    {
        if (index < 1)
        {
            return null;
        }

        var rngSeed = ComputeRngSeed(seed, index, language, genre);
        var cachePath = GetCachePath(rngSeed, genre, ".mid");

        if (File.Exists(cachePath))
        {
            return cachePath;
        }

        await _generationLock.WaitAsync(cancellationToken);
        try
        {
            if (File.Exists(cachePath))
            {
                return cachePath;
            }

            var generated = await RunPythonGeneratorAsync(rngSeed, genre, cachePath, cancellationToken);
            return generated is null ? null : cachePath;
        }
        finally
        {
            _generationLock.Release();
        }
    }

    private string GetCachePath(int rngSeed, string genre, string extension)
    {
        Directory.CreateDirectory(_cacheDirectory);
        var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{rngSeed}:{genre}")))[..16];
        return Path.Combine(_cacheDirectory, $"{key}{extension}");
    }

    private string ResolvePythonPath()
    {
        var pythonPath = configuration["MusicGeneration:PythonPath"];
        if (!string.IsNullOrWhiteSpace(pythonPath))
        {
            return pythonPath;
        }

        var bundledPython = Path.Combine(environment.ContentRootPath, "venv", "bin", "python3");
        return File.Exists(bundledPython) ? bundledPython : "python3";
    }

    private string? ResolveSoundFontPath()
    {
        var configured = configuration["MusicGeneration:SoundFontPath"];
        if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured))
        {
            return configured;
        }

        return DefaultSoundFontCandidates.FirstOrDefault(File.Exists);
    }

    private async Task<bool?> RunPythonGeneratorAsync(
        int rngSeed,
        string genre,
        string outputPath,
        CancellationToken cancellationToken)
    {
        var scriptPath = ResolveGenerateMidiScriptPath();
        if (scriptPath is null)
        {
            logger.LogWarning("MIDI script not found under content root {ContentRoot}", environment.ContentRootPath);
            return null;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = ResolvePythonPath(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        startInfo.ArgumentList.Add(scriptPath);
        startInfo.ArgumentList.Add("--rng-seed");
        startInfo.ArgumentList.Add(rngSeed.ToString());
        startInfo.ArgumentList.Add("--genre");
        startInfo.ArgumentList.Add(genre);
        startInfo.ArgumentList.Add("--output");
        startInfo.ArgumentList.Add(outputPath);

        return await RunProcessAsync(startInfo, outputPath, "MIDI generation", cancellationToken);
    }

    private async Task<bool?> RunFluidSynthAsync(
        string midiPath,
        string wavPath,
        CancellationToken cancellationToken)
    {
        var soundFontPath = ResolveSoundFontPath();
        if (soundFontPath is null)
        {
            logger.LogWarning("SoundFont file not found for WAV encoding");
            return null;
        }

        var fluidSynthPath = configuration["MusicGeneration:FluidSynthPath"];
        if (string.IsNullOrWhiteSpace(fluidSynthPath))
        {
            fluidSynthPath = "fluidsynth";
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = fluidSynthPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        startInfo.ArgumentList.Add("-ni");
        startInfo.ArgumentList.Add("-F");
        startInfo.ArgumentList.Add(wavPath);
        startInfo.ArgumentList.Add("-r");
        startInfo.ArgumentList.Add("44100");
        startInfo.ArgumentList.Add(soundFontPath);
        startInfo.ArgumentList.Add(midiPath);

        return await RunProcessAsync(startInfo, wavPath, "WAV encoding", cancellationToken);
    }

    private async Task<bool?> RunProcessAsync(
        ProcessStartInfo startInfo,
        string expectedOutputPath,
        string operationName,
        CancellationToken cancellationToken)
    {
        using var process = new Process { StartInfo = startInfo };
        try
        {
            if (!process.Start())
            {
                logger.LogWarning("Failed to start process for {Operation}", operationName);
                return null;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Process for {Operation} is not available", operationName);
            return null;
        }

        var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0 || !File.Exists(expectedOutputPath))
        {
            logger.LogError(
                "{Operation} failed with exit code {ExitCode}. stderr: {StdErr}. stdout: {StdOut}",
                operationName,
                process.ExitCode,
                stderr,
                stdout);
            return null;
        }

        return true;
    }

    private string? ResolveGenerateMidiScriptPath()
    {
        var candidates = new[]
        {
            Path.Combine(environment.ContentRootPath, "scripts", "generate_midi.py"),
            Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "scripts", "generate_midi.py")),
        };

        return candidates.FirstOrDefault(File.Exists);
    }
}
