using System.Diagnostics;
using NanoidDotNet;
using SkiaSharp;
using Svg.Skia;
using TeleCasino.BaccaratGameService.Models;
using TeleCasino.BaccaratGameService.Services.Interface;

namespace TeleCasino.BaccaratGameService.Services;

public class BaccaratGameService : IBaccaratGameService
{
    private readonly string _sharedDir;
    private readonly string _htmlDir;
    private const int Width = 400;
    private const int Height = 300;
    private static readonly Random Rand = new();
    private static readonly string FramesSubDir = "frames";
    private static readonly string VideosSubDir = "videos";
    private static readonly string ImagesSubDir = "images";

    public BaccaratGameService(IConfiguration config)
    {
        _sharedDir = config["SharedDirectory"] ?? "/shared";
        _htmlDir = config["HtmlDir"] ?? "/app/wwwroot";
    }

    public async Task<BaccaratResult> PlayGameAsync(int wager, string betArg, int gameSessionId)
    {
        var baccaratResultId = await Nanoid.GenerateAsync();
        var baccaratSharedRootPath = Path.Combine(_sharedDir, "Baccarat");
        var framesDir = Path.Combine(baccaratSharedRootPath, baccaratResultId, FramesSubDir);
        var videoDir = Path.Combine(baccaratSharedRootPath, baccaratResultId, VideosSubDir);
        var videoFile = Path.Combine(videoDir, $"{baccaratResultId}.mp4");
        var imagesDir = Path.Combine(baccaratSharedRootPath, ImagesSubDir);
        var tmpDir = Path.Combine(baccaratSharedRootPath, baccaratResultId);

        PrepareDirectory(framesDir);
        DeleteThisFile(videoFile);
        PrepareDirectory(videoDir);

        // load svgs
        var svgs = await LoadSvgsAsync(imagesDir);

        // generate game result
        var result = PlayBaccarat(baccaratResultId, betArg, wager, gameSessionId, svgs);

        // generate frames
        GenerateFrames(result, svgs, framesDir);

        // assemble video
        int exitCode = await AssembleVideoAsync(framesDir, videoFile);
        if (exitCode != 0)
            throw new ArgumentException("Invalid exit code from AssembleVideoAsync");

        // move file to public dir
        string finalVideoPath = Path.Combine(_htmlDir, Path.GetFileName(videoFile));
        if (File.Exists(videoFile))
        {
            File.Move(videoFile, finalVideoPath, overwrite: true);

            // cleanup
            Directory.Delete(tmpDir, true);
        }

        return result;
    }

    private static void PrepareDirectory(string dir)
    {
        if (Directory.Exists(dir))
            Directory.Delete(dir, true);
        Directory.CreateDirectory(dir);
    }

    private static void DeleteThisFile(string filePath)
    {
        if (File.Exists(filePath)) File.Delete(filePath);
    }

    private static Task<Dictionary<string, SKSvg>> LoadSvgsAsync(string dir)
    {
        return Task.Run(() =>
        {
            var dict = new Dictionary<string, SKSvg>();
            foreach (var file in Directory.GetFiles(dir, "*.svg"))
            {
                var key = Path.GetFileNameWithoutExtension(file)!;
                var svg = new SKSvg();
                svg.Load(file);  // still sync, but now on background thread
                dict[key] = svg;
            }
            return dict;
        });
    }

    private static BaccaratResult PlayBaccarat(string spinId, string bet, int wager, int gameSessionId, Dictionary<string, SKSvg> svgs)
    {
        var deck = svgs.Keys
            .Where(k => k != "back")
            .OrderBy(_ => Rand.Next())
            .ToList();

        if (deck.Count < 6)
            throw new InvalidOperationException("Not enough cards in deck.");

        var player = new List<string> { deck[0], deck[2] };
        var banker = new List<string> { deck[1], deck[3] };
        int idx = 4;

        int playerTotal = CalcTotal(player);
        int bankerTotal = CalcTotal(banker);

        string? playerThird = null;
        if (playerTotal <= 5)
        {
            playerThird = deck[idx++];
            player.Add(playerThird);
            playerTotal = CalcTotal(player);
        }

        bool drawBankerThird = playerThird is null
            ? bankerTotal <= 5
            : bankerTotal switch
            {
                <= 2 => true,
                3 => CardValue(playerThird!) != 8,
                4 => CardValue(playerThird!) is >= 2 and <= 7,
                5 => CardValue(playerThird!) is >= 4 and <= 7,
                6 => CardValue(playerThird!) is 6 or 7,
                _ => false
            };

        if (drawBankerThird)
        {
            var bThird = deck[idx++];
            banker.Add(bThird);
            bankerTotal = CalcTotal(banker);
        }

        string winner = playerTotal > bankerTotal
            ? "player"
            : bankerTotal > playerTotal
                ? "banker"
                : "tie";

        decimal payout = winner == bet
            ? winner switch
            {
                "player" => wager,
                "banker" => Math.Round(wager * 0.95m, 2),
                "tie" => wager * 8,
                _ => 0m
            }
            : 0m;

        if (!Enum.TryParse<BaccaratBetType>(bet, true, out var betTypeEnum))
        {
            throw new Exception($"Unknown bet type '{bet}'");
        }

        if (!Enum.TryParse<BaccaratBetType>(winner, true, out var winnerTypeEnum))
        {
            throw new Exception($"Unknown winner type '{winner}'");
        }

        var netGain = payout - wager;

        return new BaccaratResult
        {
            Id = spinId,
            Wager = wager,
            Payout = payout,
            NetGain = netGain,
            VideoFile = string.Empty,
            Win = netGain > 0,
            BetType = betTypeEnum,
            PlayerCards = player,
            BankerCards = banker,
            PlayerTotal = playerTotal,
            BankerTotal = bankerTotal,
            Winner = winnerTypeEnum,
            GameSessionId = gameSessionId,
        };
    }

    private static int CardValue(string name)
        => name.StartsWith("ace_") ? 1
            : int.TryParse(name.Split('_')[0], out var v) ? v % 10 : 0;

    private static int CalcTotal(IEnumerable<string> cards)
        => cards.Sum(CardValue) % 10;

    private static void GenerateFrames(BaccaratResult r, Dictionary<string, SKSvg> svgs, string drawFramesDir)
    {
        int f = 0;
        DrawTable(svgs, new[] { "back", "back", "back", "back" }, f++, drawFramesDir);
        DrawTable(svgs, r.PlayerCards.Take(2).Concat(r.BankerCards.Take(2)), f++, drawFramesDir);
        if (r.PlayerCards.Count == 3) DrawTable(svgs, r.PlayerCards.Concat(r.BankerCards.Take(2)), f++, drawFramesDir);
        if (r.BankerCards.Count == 3) DrawTable(svgs, r.PlayerCards.Concat(r.BankerCards), f++, drawFramesDir);
        DrawSummary(svgs, r, f++, drawFramesDir);
    }

    private static void DrawTable(Dictionary<string, SKSvg> svgs, IEnumerable<string> cards, int idx, string framesDir)
    {
        using var bmp = new SKBitmap(Width, Height);
        using var cnv = new SKCanvas(bmp);
        cnv.Clear(SKColors.ForestGreen);

        float cardW = Width * 0.3f;
        float cardH = Height * 0.3f;
        float marginX = Width * 0.1f;
        float marginY = Height * 0.15f;
        float xLeft = marginX;
        float xRight = Width - marginX - cardW;
        float yTop = marginY;
        float yBottom = Height - marginY - cardH;

        var list = cards.ToList();
        for (int i = 0; i < list.Count; i++)
        {
            if (!svgs.ContainsKey(list[i])) continue;

            var pic = svgs[list[i]].Picture!;
            var rect = pic.CullRect;
            float scale = Math.Min(cardW / rect.Width, cardH / rect.Height);
            float x = (i % 2 == 0 ? xLeft : xRight);
            float y = (i < 2 ? yTop : yBottom);
            var m = SKMatrix.CreateScale(scale, scale);
            m.TransX = x;
            m.TransY = y;
            cnv.DrawPicture(pic, in m);
        }

        var path = Path.Combine(framesDir, $"frame_{idx:D03}.png");
        using var img = SKImage.FromBitmap(bmp);
        using var data = img.Encode(SKEncodedImageFormat.Png, 90);
        File.WriteAllBytes(path, data.ToArray());
    }

    private static void DrawSummary(Dictionary<string, SKSvg> svgs, BaccaratResult r, int idx, string framesDir)
    {
        using var bmp = new SKBitmap(Width, Height);
        using var cnv = new SKCanvas(bmp);
        cnv.Clear(SKColors.ForestGreen);

        var paint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true
        };
        var font = new SKFont { Size = 24 };

        cnv.DrawText($"Player: {r.PlayerTotal}", 20, Height - 80, SKTextAlign.Left, font, paint);
        cnv.DrawText($"Banker: {r.BankerTotal}", 20, Height - 50, SKTextAlign.Left, font, paint);
        cnv.DrawText($"Winner: {r.Winner}", Width - 200, Height - 50, SKTextAlign.Left, font, paint);

        var outPath = Path.Combine(framesDir, $"frame_{idx:D03}.png");
        using var img2 = SKImage.FromBitmap(bmp);
        using var data2 = img2.Encode(SKEncodedImageFormat.Png, 90);
        File.WriteAllBytes(outPath, data2.ToArray());
    }

    private static async Task<int> AssembleVideoAsync(string framesDir, string videoFile)
    {
        var ffArgs = $"-y -framerate 1 -i \"{framesDir}/frame_%03d.png\" -c:v libx264 -pix_fmt yuv420p \"{videoFile}\"";
        var ffPsi = new ProcessStartInfo("ffmpeg", ffArgs)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var ffProc = Process.Start(ffPsi);
        if (ffProc == null)
        {
            Console.Error.WriteLine("Failed to start ffmpeg process");
            return 1;
        }

        // Read logs (optional for debugging)
        ffProc.OutputDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
        ffProc.ErrorDataReceived += (_, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };
        ffProc.BeginOutputReadLine();
        ffProc.BeginErrorReadLine();

        await ffProc.WaitForExitAsync();
        return ffProc.ExitCode;
    }
}
