using SkiaSharp;

namespace DocumentTools.Core.Services.Exam;

/// <summary>图片上的矩形区域（像素坐标，原点在左上角）。</summary>
public sealed class ExamCircleRect
{
    public int Left { get; set; }

    public int Top { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }
}

/// <summary>一处匹配结果，相似度已归一化到 0..1。</summary>
public sealed class ExamCircleMatch
{
    public int Left { get; set; }

    public int Top { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public double Similarity { get; set; }
}

/// <summary>圆圈修复请求：查找 + 覆盖所需的全部输入。</summary>
public sealed class ExamCircleFixRequest
{
    public string SourceImage { get; set; } = string.Empty;

    public ExamCircleRect TemplateRect { get; set; } = new();

    public ExamCircleRect? SearchRect { get; set; }

    public double SimilarityThreshold { get; set; } = 0.8;

    public int MaxMatches { get; set; } = 100;

    public string? ReplacementImage { get; set; }

    public IReadOnlyList<ExamCircleMatch>? ManualMatches { get; set; }

    public string? OutputFile { get; set; }
}

public interface IExamCircleFixService
{
    (int Width, int Height) GetImageSize(string imagePath);

    IReadOnlyList<ExamCircleMatch> Match(
        string imagePath,
        ExamCircleRect templateRect,
        ExamCircleRect? searchRect = null,
        double similarityThreshold = 0.8,
        int maxMatches = 100);

    string Fix(ExamCircleFixRequest request);
}

/// <summary>
/// 圆圈修复（旧 WinForms 版 OpenCvSharp 模板匹配的纯 Core 移植）。
/// 匹配使用灰度归一化互相关（NCC，等价 <c>TemplateMatchModes.CCoeffNormed</c>）：
/// 窗口均值/方差用列累计（前缀和的 O(1) 增量形式，逐候选常数代价），
/// 互相关项 Σ(t·p) 走分带二维 FFT（O(N log N)，与模板面积无关）。
/// 全程只用 SkiaSharp，不依赖 OpenCvSharp。
/// </summary>
public sealed class ExamCircleFixService : IExamCircleFixService
{
    private const int JpegQuality = 92;

    /// <summary>单个 FFT 缓冲区允许的元素上限，用于在大图上选择带宽（约 36MB / 缓冲区）。</summary>
    private const long FrameBufferBudget = 4_500_000;

    /// <summary>按列做 FFT 时一次处理的列数，保证每个缓存行被复用。</summary>
    private const int ColumnTransformBlock = 16;

    /// <summary>元素数超过该阈值才启用并行变换，避免小图上的调度开销。</summary>
    private const long ParallelThreshold = 262144;

    /// <summary>模板方差低于该值视为纯色/极低对比，NCC 无意义。</summary>
    private const double TemplateVarianceEpsilon = 1e-6;

    /// <summary>窗口方差低于该值视为纯色窗口，跳过以避免除零放大噪声。</summary>
    private const double WindowVarianceEpsilon = 1e-6;

    /// <summary>
    /// 替换图缩放取样。dev <c>btnDoFix_Click</c> 用 <c>InterpolationFlags.Linear</c>（ExamCircleFixForm.cs:794），
    /// 这里保持同一插值，保证同一夹具两边回写像素一致（task-40 等价性要求）。
    /// </summary>
    private static readonly SKSamplingOptions ReplacementSampling = new(SKFilterMode.Linear, SKMipmapMode.None);

    public (int Width, int Height) GetImageSize(string imagePath)
    {
        ValidateImagePath(imagePath, nameof(imagePath));
        return ReadImageSize(imagePath);
    }

    public IReadOnlyList<ExamCircleMatch> Match(
        string imagePath,
        ExamCircleRect templateRect,
        ExamCircleRect? searchRect = null,
        double similarityThreshold = 0.8,
        int maxMatches = 100)
    {
        ValidateImagePath(imagePath, nameof(imagePath));

        if (templateRect is null)
        {
            throw new ArgumentException("模板矩形不能为空。", nameof(templateRect));
        }

        if (templateRect.Width <= 0 || templateRect.Height <= 0)
        {
            throw new ArgumentException("模板矩形的宽和高必须大于 0。", nameof(templateRect));
        }

        (byte[] gray, int imageWidth, int imageHeight) = LoadGrayscale(imagePath);

        if (!IsRectInsideImage(templateRect, imageWidth, imageHeight))
        {
            throw new ArgumentException("模板矩形超出图片范围。", nameof(templateRect));
        }

        int templateWidth = templateRect.Width;
        int templateHeight = templateRect.Height;
        (int Left, int Top, int Width, int Height) region = ResolveSearchRegion(
            searchRect, templateWidth, templateHeight, imageWidth, imageHeight);

        if (maxMatches <= 0)
        {
            return Array.Empty<ExamCircleMatch>();
        }

        double threshold = NormalizeThreshold(similarityThreshold);

        // 模板灰度与统计量（整数灰度值，double 求和精确无误差）。
        var template = new double[templateWidth * templateHeight];
        double templateSum = 0;
        double templateSquareSum = 0;
        for (int v = 0; v < templateHeight; v++)
        {
            int rowBase = (templateRect.Top + v) * imageWidth + templateRect.Left;
            int templateBase = v * templateWidth;
            for (int u = 0; u < templateWidth; u++)
            {
                double value = gray[rowBase + u];
                template[templateBase + u] = value;
                templateSum += value;
                templateSquareSum += value * value;
            }
        }

        double area = (double)templateWidth * templateHeight;
        double templateMean = templateSum / area;
        double templateVariance = templateSquareSum / area - templateMean * templateMean;
        if (templateVariance <= TemplateVarianceEpsilon)
        {
            // 纯色 / 极低对比模板没有可匹配的结构：返回空集合（中性结果），不做除法。
            return Array.Empty<ExamCircleMatch>();
        }

        double templateStdDev = global::System.Math.Sqrt(templateVariance);

        int searchWidth = region.Width;
        int searchHeight = region.Height;
        int outWidth = searchWidth - templateWidth + 1;
        int outHeight = searchHeight - templateHeight + 1;
        if (outWidth <= 0 || outHeight <= 0)
        {
            return Array.Empty<ExamCircleMatch>();
        }

        List<Candidate> candidates = CollectCandidates(
            gray, imageWidth, template, region.Left, region.Top, searchWidth,
            templateWidth, templateHeight, outWidth, outHeight,
            templateMean, templateStdDev, threshold);
        if (candidates.Count == 0)
        {
            return Array.Empty<ExamCircleMatch>();
        }

        // 相似度降序 + 非极大值抑制：与已保留框中心距离小于模板宽/高一半者丢弃。
        candidates.Sort(static (left, right) => right.Similarity.CompareTo(left.Similarity));

        double halfWidth = templateWidth / 2.0;
        double halfHeight = templateHeight / 2.0;
        var matches = new List<ExamCircleMatch>(global::System.Math.Min(maxMatches, candidates.Count));
        foreach (Candidate candidate in candidates)
        {
            if (matches.Count >= maxMatches)
            {
                break;
            }

            double centerX = candidate.Left + halfWidth;
            double centerY = candidate.Top + halfHeight;
            bool suppressed = false;
            foreach (ExamCircleMatch kept in matches)
            {
                if (global::System.Math.Abs(centerX - (kept.Left + halfWidth)) < halfWidth
                    && global::System.Math.Abs(centerY - (kept.Top + halfHeight)) < halfHeight)
                {
                    suppressed = true;
                    break;
                }
            }

            if (suppressed)
            {
                continue;
            }

            matches.Add(new ExamCircleMatch
            {
                Left = candidate.Left,
                Top = candidate.Top,
                Width = templateWidth,
                Height = templateHeight,
                Similarity = global::System.Math.Clamp(candidate.Similarity, 0.0, 1.0)
            });
        }

        return matches;
    }

    public string Fix(ExamCircleFixRequest request)
    {
        if (request is null)
        {
            throw new ArgumentException("修复请求不能为空。", nameof(request));
        }

        string sourcePath = request.SourceImage ?? string.Empty;
        ValidateImagePath(sourcePath, nameof(request));

        string sourceFullPath = Path.GetFullPath(sourcePath);
        string outputFullPath = ResolveOutputPath(sourceFullPath, request.OutputFile);
        if (string.Equals(sourceFullPath, outputFullPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("输出路径不能与源图片相同，源图片不予覆盖。", nameof(request));
        }

        (int imageWidth, int imageHeight) = ReadImageSize(sourceFullPath);
        SKBitmap? target = SKBitmap.Decode(
            sourceFullPath,
            new SKImageInfo(imageWidth, imageHeight, SKColorType.Bgra8888, SKAlphaType.Premul));
        if (target is null)
        {
            throw new ArgumentException("无法解码源图片。", nameof(request));
        }

        SKBitmap? replacement = null;
        SKBitmap? ownedPatch = null;
        try
        {
            IReadOnlyList<ExamCircleMatch> matches = request.ManualMatches is { Count: > 0 } manual
                ? manual
                : Match(sourceFullPath, request.TemplateRect, request.SearchRect, request.SimilarityThreshold, request.MaxMatches);

            if (!string.IsNullOrWhiteSpace(request.ReplacementImage))
            {
                replacement = LoadBitmap(request.ReplacementImage!);
            }

            SKBitmap patch;
            if (replacement is not null)
            {
                patch = replacement;
            }
            else
            {
                ownedPatch = ExtractTemplatePatch(target, request.TemplateRect);
                patch = ownedPatch;
            }

            // 逐个覆盖绘制：单 canvas、单线程写入，避免并发写同一画布。
            using (SKCanvas canvas = new(target))
            using (SKPaint paint = new() { IsAntialias = false })
            {
                foreach (ExamCircleMatch match in matches)
                {
                    if (!TryClampRect(
                            match.Left, match.Top, match.Width, match.Height,
                            imageWidth, imageHeight,
                            out int left, out int top, out int width, out int height))
                    {
                        continue;
                    }

                    var destination = new SKRect(left, top, left + width, top + height);
                    if (patch.Width == width && patch.Height == height)
                    {
                        canvas.DrawBitmap(patch, destination, paint);
                        continue;
                    }

                    using SKBitmap resized = patch.Resize(
                        new SKImageInfo(width, height, patch.ColorType, patch.AlphaType),
                        ReplacementSampling) ?? throw new InvalidOperationException("缩放替换图片失败。");
                    canvas.DrawBitmap(resized, destination, paint);
                }
            }

            WriteBitmap(target, outputFullPath);
        }
        finally
        {
            target.Dispose();
            ownedPatch?.Dispose();
            replacement?.Dispose();
        }

        return outputFullPath;
    }

    /// <summary>
    /// 逐行滑动窗口：均值/方差用列累计（前缀和的 O(1) 增量形式，内存 O(搜索宽度)），
    /// 互相关项 Σ(t·p) 按条带二维 FFT 计算，逐候选代价与模板面积无关。
    /// </summary>
    private static List<Candidate> CollectCandidates(
        byte[] gray,
        int imageWidth,
        double[] template,
        int left,
        int top,
        int searchWidth,
        int templateWidth,
        int templateHeight,
        int outWidth,
        int outHeight,
        double templateMean,
        double templateStdDev,
        double threshold)
    {
        var candidates = new List<Candidate>();
        var columnSum = new double[searchWidth];
        var columnSquareSum = new double[searchWidth];

        // 预置第一行窗口的列累计。
        for (int v = 0; v < templateHeight; v++)
        {
            int rowBase = (top + v) * imageWidth + left;
            for (int x = 0; x < searchWidth; x++)
            {
                double value = gray[rowBase + x];
                columnSum[x] += value;
                columnSquareSum[x] += value * value;
            }
        }

        double area = (double)templateWidth * templateHeight;

        // 频域互相关：缓冲区尺寸只取决于搜索宽度与带宽，不随模板面积增长。
        int fftWidth = NextPowerOfTwo(searchWidth);
        (int bandRows, int fftHeight) = ResolveBandPlan(outHeight, templateHeight, fftWidth);
        var planWidth = new FftPlan(fftWidth);
        var planHeight = new FftPlan(fftHeight);
        int bufferLength = fftWidth * fftHeight;
        var bandRe = new double[bufferLength];
        var bandIm = new double[bufferLength];
        var spectrumRe = new double[bufferLength];
        var spectrumIm = new double[bufferLength];
        var bandCorrelation = new double[bandRows * outWidth];
        BuildTemplateSpectrum(
            template, templateWidth, templateHeight,
            fftWidth, fftHeight, spectrumRe, spectrumIm, planWidth, planHeight);

        for (int bandStart = 0; bandStart < outHeight; bandStart += bandRows)
        {
            int rows = global::System.Math.Min(bandRows, outHeight - bandStart);
            int loadRows = rows + templateHeight - 1;
            ComputeBandCorrelation(
                gray, imageWidth, left, top, bandStart, rows, loadRows,
                searchWidth, outWidth, fftWidth, fftHeight,
                spectrumRe, spectrumIm, bandRe, bandIm, bandCorrelation, planWidth, planHeight);

            for (int row = 0; row < rows; row++)
            {
                int yy = bandStart + row;
                if (yy > 0)
                {
                    int enteringBase = (top + yy + templateHeight - 1) * imageWidth + left;
                    int leavingBase = (top + yy - 1) * imageWidth + left;
                    for (int x = 0; x < searchWidth; x++)
                    {
                        double entering = gray[enteringBase + x];
                        double leaving = gray[leavingBase + x];
                        columnSum[x] += entering - leaving;
                        columnSquareSum[x] += (entering * entering) - (leaving * leaving);
                    }
                }

                double windowSum = 0;
                double windowSquareSum = 0;
                for (int u = 0; u < templateWidth; u++)
                {
                    windowSum += columnSum[u];
                    windowSquareSum += columnSquareSum[u];
                }

                int rowOffset = row * outWidth;
                for (int xx = 0; xx < outWidth; xx++)
                {
                    if (xx > 0)
                    {
                        windowSum += columnSum[xx + templateWidth - 1] - columnSum[xx - 1];
                        windowSquareSum += columnSquareSum[xx + templateWidth - 1] - columnSquareSum[xx - 1];
                    }

                    double windowMean = windowSum / area;
                    double windowVariance = (windowSquareSum / area) - (windowMean * windowMean);
                    if (windowVariance <= WindowVarianceEpsilon)
                    {
                        continue;
                    }

                    // NCC = (Σtp - n·t̄p̄) / (n·σt·σp)
                    double numerator = bandCorrelation[rowOffset + xx] - (area * templateMean * windowMean);
                    double similarity = numerator / (area * templateStdDev * global::System.Math.Sqrt(windowVariance));
                    if (double.IsNaN(similarity) || similarity <= 0 || similarity < threshold)
                    {
                        continue;
                    }

                    candidates.Add(new Candidate(left + xx, top + yy, similarity));
                }
            }
        }

        return candidates;
    }

    /// <summary>大于等于给定值的最小 2 的幂。</summary>
    private static int NextPowerOfTwo(int value)
    {
        int result = 1;
        while (result < value)
        {
            result <<= 1;
        }

        return result;
    }

    /// <summary>基 2 迭代 FFT 的预计算表：位反转索引与旋转因子。</summary>
    private sealed class FftPlan
    {
        public FftPlan(int size)
        {
            Size = size;
            BitReverse = new int[size];
            Cos = new double[size / 2];
            Sin = new double[size / 2];

            int bits = 0;
            while ((1 << bits) < size)
            {
                bits++;
            }

            for (int i = 0; i < size; i++)
            {
                int reversed = 0;
                for (int b = 0; b < bits; b++)
                {
                    reversed |= ((i >> b) & 1) << (bits - 1 - b);
                }

                BitReverse[i] = reversed;
            }

            for (int k = 0; k < size / 2; k++)
            {
                double angle = -2.0 * global::System.Math.PI * k / size;
                Cos[k] = global::System.Math.Cos(angle);
                Sin[k] = global::System.Math.Sin(angle);
            }
        }

        public int Size { get; }

        public int[] BitReverse { get; }

        public double[] Cos { get; }

        public double[] Sin { get; }
    }

    /// <summary>对一维片段做原位基 2 FFT，用 offset/stride 同时支持行与列两种访问方式。</summary>
    private static void Transform1D(
        double[] re,
        double[] im,
        int offset,
        int stride,
        FftPlan plan,
        bool inverse)
    {
        int size = plan.Size;
        int[] bitReverse = plan.BitReverse;

        for (int i = 0; i < size; i++)
        {
            int target = bitReverse[i];
            if (target <= i)
            {
                continue;
            }

            int a = offset + (i * stride);
            int b = offset + (target * stride);
            (re[a], re[b]) = (re[b], re[a]);
            (im[a], im[b]) = (im[b], im[a]);
        }

        for (int length = 2; length <= size; length <<= 1)
        {
            int half = length >> 1;
            int twiddleStep = size / length;
            for (int start = 0; start < size; start += length)
            {
                for (int k = 0; k < half; k++)
                {
                    int twiddle = k * twiddleStep;
                    double wr = plan.Cos[twiddle];
                    double wi = inverse ? -plan.Sin[twiddle] : plan.Sin[twiddle];
                    int a = offset + ((start + k) * stride);
                    int b = offset + ((start + k + half) * stride);
                    double xr = (re[b] * wr) - (im[b] * wi);
                    double xi = (re[b] * wi) + (im[b] * wr);
                    re[b] = re[a] - xr;
                    im[b] = im[a] - xi;
                    re[a] += xr;
                    im[a] += xi;
                }
            }
        }

        if (!inverse)
        {
            return;
        }

        double scale = 1.0 / size;
        for (int i = 0; i < size; i++)
        {
            int index = offset + (i * stride);
            re[index] *= scale;
            im[index] *= scale;
        }
    }

    private static void Transform2D(
        double[] re,
        double[] im,
        int width,
        int height,
        FftPlan planWidth,
        FftPlan planHeight,
        bool inverse)
    {
        if ((long)width * height >= ParallelThreshold)
        {
            global::System.Threading.Tasks.Parallel.For(
                0, height, y => Transform1D(re, im, y * width, 1, planWidth, inverse));
        }
        else
        {
            for (int y = 0; y < height; y++)
            {
                Transform1D(re, im, y * width, 1, planWidth, inverse);
            }
        }

        TransformColumns(re, im, width, height, planHeight, inverse);
    }

    /// <summary>
    /// 按列做 FFT：同一蝴蝶内的旋转因子与列无关，因此按列块批处理可以把跨行访问
    /// 变成连续读取，避免 stride = width 造成的逐元素缓存缺失。
    /// </summary>
    private static void TransformColumns(
        double[] re,
        double[] im,
        int width,
        int height,
        FftPlan plan,
        bool inverse)
    {
        int size = plan.Size;
        int[] bitReverse = plan.BitReverse;
        double[] cos = plan.Cos;
        double[] sin = plan.Sin;

        int blockCount = (width + ColumnTransformBlock - 1) / ColumnTransformBlock;

        void TransformBlock(int blockIndex)
        {
            int block = blockIndex * ColumnTransformBlock;
            int columns = global::System.Math.Min(ColumnTransformBlock, width - block);

            for (int i = 0; i < size; i++)
            {
                int target = bitReverse[i];
                if (target <= i)
                {
                    continue;
                }

                int a = (i * width) + block;
                int b = (target * width) + block;
                for (int c = 0; c < columns; c++)
                {
                    (re[a + c], re[b + c]) = (re[b + c], re[a + c]);
                    (im[a + c], im[b + c]) = (im[b + c], im[a + c]);
                }
            }

            for (int length = 2; length <= size; length <<= 1)
            {
                int half = length >> 1;
                int twiddleStep = size / length;
                for (int start = 0; start < size; start += length)
                {
                    for (int k = 0; k < half; k++)
                    {
                        int twiddle = k * twiddleStep;
                        double wr = cos[twiddle];
                        double wi = inverse ? -sin[twiddle] : sin[twiddle];
                        int a = ((start + k) * width) + block;
                        int b = ((start + k + half) * width) + block;
                        for (int c = 0; c < columns; c++)
                        {
                            double xr = (re[b + c] * wr) - (im[b + c] * wi);
                            double xi = (re[b + c] * wi) + (im[b + c] * wr);
                            re[b + c] = re[a + c] - xr;
                            im[b + c] = im[a + c] - xi;
                            re[a + c] += xr;
                            im[a + c] += xi;
                        }
                    }
                }
            }

            if (!inverse)
            {
                return;
            }

            double scale = 1.0 / size;
            for (int i = 0; i < size; i++)
            {
                int rowIndex = (i * width) + block;
                for (int c = 0; c < columns; c++)
                {
                    re[rowIndex + c] *= scale;
                    im[rowIndex + c] *= scale;
                }
            }
        }

        if (blockCount > 1 && (long)width * height >= ParallelThreshold)
        {
            global::System.Threading.Tasks.Parallel.For(0, blockCount, TransformBlock);
            return;
        }

        for (int blockIndex = 0; blockIndex < blockCount; blockIndex++)
        {
            TransformBlock(blockIndex);
        }
    }

    /// <summary>
    /// 选择带宽与 FFT 高度：在缓冲区不超过 <see cref="FrameBufferBudget"/> 的前提下，
    /// 让「条带数 × FFT 面积」最小，从而让总变换量接近单次整幅变换的下界。
    /// </summary>
    private static (int BandRows, int FftHeight) ResolveBandPlan(
        int outHeight,
        int templateHeight,
        int fftWidth)
    {
        int minFftHeight = NextPowerOfTwo(templateHeight);
        int maxFftHeight = NextPowerOfTwo(outHeight + templateHeight - 1);
        int bestBand = outHeight;
        int bestFftHeight = maxFftHeight;
        long bestPoints = long.MaxValue;
        bool bestWithinBudget = false;

        for (int fftHeight = minFftHeight; fftHeight <= maxFftHeight; fftHeight <<= 1)
        {
            int band = global::System.Math.Min(outHeight, fftHeight - templateHeight + 1);
            if (band <= 0)
            {
                continue;
            }

            long bands = ((long)outHeight + band - 1) / band;
            long points = bands * fftWidth * fftHeight;
            bool withinBudget = (long)fftWidth * fftHeight <= FrameBufferBudget;

            bool better;
            if (withinBudget != bestWithinBudget)
            {
                better = withinBudget;
            }
            else
            {
                better = points < bestPoints;
            }

            if (!better)
            {
                continue;
            }

            bestPoints = points;
            bestBand = band;
            bestFftHeight = fftHeight;
            bestWithinBudget = withinBudget;
        }

        return (bestBand, bestFftHeight);
    }

    /// <summary>把模板填充到 (0,0) 并转换到频域，作为互相关的核。</summary>
    private static void BuildTemplateSpectrum(
        double[] template,
        int templateWidth,
        int templateHeight,
        int fftWidth,
        int fftHeight,
        double[] spectrumRe,
        double[] spectrumIm,
        FftPlan planWidth,
        FftPlan planHeight)
    {
        global::System.Array.Clear(spectrumRe, 0, spectrumRe.Length);
        global::System.Array.Clear(spectrumIm, 0, spectrumIm.Length);

        for (int v = 0; v < templateHeight; v++)
        {
            int sourceBase = v * templateWidth;
            int targetBase = v * fftWidth;
            for (int u = 0; u < templateWidth; u++)
            {
                spectrumRe[targetBase + u] = template[sourceBase + u];
            }
        }

        Transform2D(spectrumRe, spectrumIm, fftWidth, fftHeight, planWidth, planHeight, false);
    }

    /// <summary>
    /// 单条带互相关：装载 [top + bandStart, top + bandStart + loadRowCount) 的像素带，
    /// 频域乘以模板谱的共轭后逆变换，取左上 bandRowCount × outWidth 区域即 Σ(t·p)。
    /// </summary>
    private static void ComputeBandCorrelation(
        byte[] gray,
        int imageWidth,
        int left,
        int top,
        int bandStart,
        int bandRowCount,
        int loadRowCount,
        int searchWidth,
        int outWidth,
        int fftWidth,
        int fftHeight,
        double[] spectrumRe,
        double[] spectrumIm,
        double[] bandRe,
        double[] bandIm,
        double[] destination,
        FftPlan planWidth,
        FftPlan planHeight)
    {
        global::System.Array.Clear(bandRe, 0, bandRe.Length);
        global::System.Array.Clear(bandIm, 0, bandIm.Length);

        for (int r = 0; r < loadRowCount; r++)
        {
            int sourceBase = ((top + bandStart + r) * imageWidth) + left;
            int targetBase = r * fftWidth;
            for (int x = 0; x < searchWidth; x++)
            {
                bandRe[targetBase + x] = gray[sourceBase + x];
            }
        }

        Transform2D(bandRe, bandIm, fftWidth, fftHeight, planWidth, planHeight, false);

        int length = fftWidth * fftHeight;
        for (int i = 0; i < length; i++)
        {
            double ar = bandRe[i];
            double ai = bandIm[i];
            double cr = spectrumRe[i];
            double ci = spectrumIm[i];
            bandRe[i] = (ar * cr) + (ai * ci);
            bandIm[i] = (ai * cr) - (ar * ci);
        }

        Transform2D(bandRe, bandIm, fftWidth, fftHeight, planWidth, planHeight, true);

        for (int r = 0; r < bandRowCount; r++)
        {
            int sourceBase = r * fftWidth;
            int targetBase = r * outWidth;
            for (int x = 0; x < outWidth; x++)
            {
                destination[targetBase + x] = bandRe[sourceBase + x];
            }
        }
    }

    /// <summary>搜索范围：null 取整图；否则外扩一圈模板尺寸后收敛到图片内。</summary>
    private static (int Left, int Top, int Width, int Height) ResolveSearchRegion(
        ExamCircleRect? searchRect,
        int templateWidth,
        int templateHeight,
        int imageWidth,
        int imageHeight)
    {
        if (searchRect is null)
        {
            return (0, 0, imageWidth, imageHeight);
        }

        if (searchRect.Width <= 0 || searchRect.Height <= 0)
        {
            throw new ArgumentException("搜索矩形的宽和高必须大于 0。", nameof(searchRect));
        }

        long left = (long)searchRect.Left - templateWidth;
        long top = (long)searchRect.Top - templateHeight;
        long right = (long)searchRect.Left + searchRect.Width + templateWidth;
        long bottom = (long)searchRect.Top + searchRect.Height + templateHeight;

        left = global::System.Math.Clamp(left, 0, imageWidth);
        top = global::System.Math.Clamp(top, 0, imageHeight);
        right = global::System.Math.Clamp(right, 0, imageWidth);
        bottom = global::System.Math.Clamp(bottom, 0, imageHeight);

        int width = (int)(right - left);
        int height = (int)(bottom - top);

        // 收缩后仍装不下模板时，向内扩展并保持窗口在图片内。
        if (width < templateWidth)
        {
            width = templateWidth;
            left = global::System.Math.Clamp(left, 0, global::System.Math.Max(0, imageWidth - width));
        }

        if (height < templateHeight)
        {
            height = templateHeight;
            top = global::System.Math.Clamp(top, 0, global::System.Math.Max(0, imageHeight - height));
        }

        if (left + width > imageWidth)
        {
            left = imageWidth - width;
        }

        if (top + height > imageHeight)
        {
            top = imageHeight - height;
        }

        return ((int)left, (int)top, width, height);
    }

    private static double NormalizeThreshold(double similarityThreshold)
    {
        if (double.IsNaN(similarityThreshold))
        {
            return 0.8;
        }

        return global::System.Math.Clamp(similarityThreshold, 0.0, 1.0);
    }

    private static (int Width, int Height) ReadImageSize(string imagePath)
    {
        using FileStream stream = File.OpenRead(imagePath);
        using SKCodec? codec = SKCodec.Create(stream);
        if (codec is null)
        {
            throw new ArgumentException("无法读取图片尺寸，文件格式可能不受支持。", nameof(imagePath));
        }

        SKImageInfo info = codec.Info;
        if (info.Width <= 0 || info.Height <= 0)
        {
            throw new ArgumentException("图片尺寸无效。", nameof(imagePath));
        }

        return (info.Width, info.Height);
    }

    private static (byte[] Gray, int Width, int Height) LoadGrayscale(string imagePath)
    {
        (int width, int height) = ReadImageSize(imagePath);
        SKBitmap? bitmap = SKBitmap.Decode(
            imagePath,
            new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul));
        if (bitmap is null)
        {
            throw new ArgumentException("无法解码图片数据，文件可能已损坏。", nameof(imagePath));
        }

        try
        {
            int rowBytes = bitmap.RowBytes;
            if (rowBytes < width * 4)
            {
                throw new ArgumentException("图片像素格式不受支持。", nameof(imagePath));
            }

            Span<byte> pixels = bitmap.GetPixelSpan();
            var gray = new byte[width * height];
            for (int y = 0; y < height; y++)
            {
                int sourceRow = y * rowBytes;
                int targetRow = y * width;
                for (int x = 0; x < width; x++)
                {
                    int index = sourceRow + (x << 2);
                    // BT.601 亮度，与 OpenCV BGR2GRAY 的整数近似一致。
                    gray[targetRow + x] = (byte)(((pixels[index + 2] * 299) + (pixels[index + 1] * 587) + (pixels[index] * 114)) / 1000);
                }
            }

            return (gray, width, height);
        }
        finally
        {
            bitmap.Dispose();
        }
    }

    private static SKBitmap ExtractTemplatePatch(SKBitmap source, ExamCircleRect templateRect)
    {
        if (templateRect is null)
        {
            throw new ArgumentException("模板矩形不能为空。", nameof(templateRect));
        }

        if (templateRect.Width <= 0 || templateRect.Height <= 0)
        {
            throw new ArgumentException("模板矩形的宽和高必须大于 0。", nameof(templateRect));
        }

        if (!IsRectInsideImage(templateRect, source.Width, source.Height))
        {
            throw new ArgumentException("模板矩形超出图片范围。", nameof(templateRect));
        }

        var patch = new SKBitmap(
            new SKImageInfo(templateRect.Width, templateRect.Height, source.ColorType, source.AlphaType));
        using (SKCanvas canvas = new(patch))
        using (SKPaint paint = new() { IsAntialias = false })
        {
            canvas.DrawBitmap(
                source,
                new SKRect(
                    templateRect.Left,
                    templateRect.Top,
                    templateRect.Left + templateRect.Width,
                    templateRect.Top + templateRect.Height),
                new SKRect(0, 0, templateRect.Width, templateRect.Height),
                paint);
        }

        if (patch.ColorType != SKColorType.Bgra8888)
        {
            SKBitmap? converted = patch.Copy(SKColorType.Bgra8888);
            patch.Dispose();
            return converted ?? throw new InvalidOperationException("模板区域取样失败。");
        }

        return patch;
    }

    private static SKBitmap LoadBitmap(string path)
    {
        ValidateImagePath(path, nameof(path));
        SKBitmap? decoded = SKBitmap.Decode(path);
        if (decoded is null)
        {
            throw new ArgumentException("无法解码图片数据，文件可能已损坏。", nameof(path));
        }

        if (decoded.ColorType == SKColorType.Bgra8888)
        {
            return decoded;
        }

        SKBitmap? converted = decoded.Copy(SKColorType.Bgra8888);
        decoded.Dispose();
        return converted ?? throw new InvalidOperationException("图片颜色格式转换失败。");
    }

    private static void WriteBitmap(SKBitmap bitmap, string outputPath)
    {
        string? directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(ResolveEncodedFormat(outputPath), JpegQuality);
        using FileStream stream = File.Create(outputPath);
        data.SaveTo(stream);
    }

    private static SKEncodedImageFormat ResolveEncodedFormat(string outputPath)
        => Path.GetExtension(outputPath).ToLowerInvariant() switch
        {
            ".png" => SKEncodedImageFormat.Png,
            _ => SKEncodedImageFormat.Jpeg
        };

    private static string ResolveOutputPath(string sourceFullPath, string? outputFile)
    {
        if (!string.IsNullOrWhiteSpace(outputFile))
        {
            return Path.GetFullPath(outputFile);
        }

        string? directory = Path.GetDirectoryName(sourceFullPath);
        string stem = Path.GetFileNameWithoutExtension(sourceFullPath);
        string extension = Path.GetExtension(sourceFullPath);
        if (string.IsNullOrEmpty(extension))
        {
            extension = ".png";
        }

        string fileName = stem + "_fixed" + extension;
        return string.IsNullOrEmpty(directory)
            ? Path.GetFullPath(fileName)
            : Path.Combine(directory, fileName);
    }

    private static bool IsRectInsideImage(ExamCircleRect rect, int imageWidth, int imageHeight)
        => rect.Left >= 0
           && rect.Top >= 0
           && (long)rect.Left + rect.Width <= imageWidth
           && (long)rect.Top + rect.Height <= imageHeight;

    private static bool TryClampRect(
        int left,
        int top,
        int width,
        int height,
        int imageWidth,
        int imageHeight,
        out int clampedLeft,
        out int clampedTop,
        out int clampedWidth,
        out int clampedHeight)
    {
        clampedLeft = 0;
        clampedTop = 0;
        clampedWidth = 0;
        clampedHeight = 0;

        if (width <= 0 || height <= 0 || width > imageWidth || height > imageHeight)
        {
            return false;
        }

        clampedLeft = global::System.Math.Clamp(left, 0, imageWidth - width);
        clampedTop = global::System.Math.Clamp(top, 0, imageHeight - height);
        clampedWidth = width;
        clampedHeight = height;
        return true;
    }

    private static void ValidateImagePath(string? imagePath, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            throw new ArgumentException("图片路径不能为空。", parameterName);
        }

        if (!File.Exists(imagePath))
        {
            throw new FileNotFoundException("图片不存在。", imagePath);
        }
    }

    private readonly struct Candidate
    {
        public Candidate(int left, int top, double similarity)
        {
            Left = left;
            Top = top;
            Similarity = similarity;
        }

        public int Left { get; }

        public int Top { get; }

        public double Similarity { get; }
    }
}
