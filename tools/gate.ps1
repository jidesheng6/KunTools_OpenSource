<#
.SYNOPSIS
    DocumentTools 发布闸门：一条命令跑完六道门，任一不过立即停并返回非 0。

.DESCRIPTION
    顺序固定：build → test → FeatureSmoke → UiSnapshot → QaFunction(--bonus) → QaCompat。
    每道门单独打印「门 / 结论 / 关键数字 / 耗时」，最后给一张汇总表与总判定。
    判定口径：
      build          0 个警告 且 0 个错误（警告不为 0 也算失败）
      test           失败 0，且跳过 <= -MaxSkips（默认 3；见 -MaxSkips 说明「白名单，不是免检额度」）
      FeatureSmoke   47/47（创建数 = 布局数 = 功能总数），启动落点检查失败 0
      UiSnapshot     fatal=0 high=0 medium=0；low 只记录不判失败，但必须出现在汇总里
      QaFunction     退出码 0，且输出里既无 [FAIL] 也无 [SKIP]
      QaCompat       无新增 FAIL（基线名单为空；ace.missing-provider / ace.simulate-switch / html.no-browser 已随 Core 文案同步修好并于 2026-11-05 移出名单，本机不可验证项单独计「无法验证」）
    失败时打印可直接复制的复跑命令；每道门原始输出尾部 5 行写入 %TEMP%\dt-gate-<时间戳>.log。

.EXAMPLE
    powershell -NoProfile -File tools\gate.ps1

.EXAMPLE
    pwsh -NoProfile -File tools\gate.ps1 -LogDirectory D:\temp
#>
[CmdletBinding()]
param(
    # 日志目录，默认 %TEMP%
    [string]$LogDirectory = '',

    # 允许的跳过用例上限。默认 3 = 本机装了 Word 时开三个 opt-in 用例：
    #   WordLocalOfficeTests、WordLocalOfficeDiagnosticsTests、
    #   OperationOptionKeyMappingTests.WordToPdf_LocalOffice_*
    # 它们都靠 DOCUMENTTOOLS_TEST_LOCAL_OFFICE=1 才跑；没装 Word 的机器上它们本来就 Skip，
    # 所以那种机器传 -MaxSkips 0 也不该红（上限只是白名单，不是免检额度）。
    # 第 4 个静默跳过会立刻暴露：门 2 会把「被跳过用例的名字」逐条打印出来。
    [int]$MaxSkips = 3
)

$ErrorActionPreference = 'Stop'

$RepoRoot = Split-Path -Parent $PSScriptRoot
$Stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
if ([string]::IsNullOrWhiteSpace($LogDirectory)) {
    $LogDirectory = [System.IO.Path]::GetTempPath()
}
$LogFile = Join-Path $LogDirectory ('dt-gate-{0}.log' -f $Stamp)

$GateText = New-Object 'System.Collections.Generic.List[string]'
$Results = New-Object 'System.Collections.ArrayList'
$TotalGates = 6

function Write-GateLine {
    param([string]$Text = '')
    [void]$GateText.Add($Text)
    Write-Host $Text
}

function Show-GateHeader {
    param([int]$Index, [string]$Title, [string]$Command)
    Write-GateLine ''
    Write-GateLine ('=== 门 {0}/{1}：{2} ===' -f $Index, $TotalGates, $Title)
    Write-GateLine ('命令：{0}' -f $Command)
}

function Invoke-Dotnet {
    # 统一执行 dotnet 子命令：捕获 stdout+stderr、保留退出码、记耗时。
    # 必须临时把 ErrorActionPreference 降为 Continue：PowerShell 5.1 会把原生命令的
    # 第一行 stderr（例如 testhost 崩溃）当成 terminating error，在 Stop 下直接把闸门打断，
    # 于是"某道门失败"变成"闸门自己崩了"，汇总与日志都不会打印。
    param([string[]]$Arguments)
    $previous = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    $watch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $output = @(& dotnet @Arguments 2>&1 | ForEach-Object { [string]$_ })
        $code = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previous
        $watch.Stop()
    }
    [pscustomobject]@{
        ExitCode = $code
        Output   = $output
        Seconds  = [math]::Round($watch.Elapsed.TotalSeconds, 1)
    }
}

function Get-Count {
    # 从输出里抓第 N 个捕获组的整数，抓不到返回 -1（调用方按失败处理）。
    param([string]$Text, [string]$Pattern, [int]$Group = 1)
    $match = [regex]::Match($Text, $Pattern)
    if (-not $match.Success) { return -1 }
    return [int]$match.Groups[$Group].Value
}

function Complete-Gate {
    # 打印结论/关键数字/耗时，登记到汇总，返回该门是否通过。
    param(
        [string]$Title,
        [bool]$Pass,
        [string]$Numbers,
        $Run,
        [string[]]$Rerun = @()
    )
    $seconds = 0
    if ($null -ne $Run) { $seconds = $Run.Seconds }
    $verdict = 'FAIL'
    if ($Pass) { $verdict = 'PASS' }
    Write-GateLine ('结论：{0}' -f $verdict)
    Write-GateLine ('关键数字：{0}' -f $Numbers)
    Write-GateLine ('耗时：{0}s' -f $seconds)

    $tail = @()
    if ($null -ne $Run -and $Run.Output.Count -gt 0) {
        $tail = @($Run.Output | Select-Object -Last 5)
    }

    [void]$Results.Add([pscustomobject]@{
            Title   = $Title
            Pass    = $Pass
            Numbers = $Numbers
            Seconds = $seconds
            Rerun   = @($Rerun)
            Tail    = $tail
        })
    return $Pass
}

function Get-BlockingProcesses {
    # 别人正在跑 GUI 或测试宿主时，build/test 的失败会被误读成代码坏了，先挡住。
    $blocking = @()
    foreach ($name in @('DocumentTools', 'testhost')) {
        $procs = @(Get-Process -Name $name -ErrorAction SilentlyContinue)
        if ($procs.Count -gt 0) {
            $ids = ($procs | ForEach-Object { $_.Id }) -join ','
            $blocking += ('{0}（{1} 个，PID {2}）' -f $name, $procs.Count, $ids)
        }
    }
    return $blocking
}

function Test-BuildGate {
    $command = 'dotnet build DocumentTools.sln --no-incremental -v q --nologo'
    Show-GateHeader -Index 1 -Title 'build' -Command $command
    $run = Invoke-Dotnet -Arguments @('build', 'DocumentTools.sln', '--no-incremental', '-v', 'q', '--nologo')
    $text = $run.Output -join "`n"
    $warnings = Get-Count -Text $text -Pattern '(\d+)\s*个警告'
    $errors = Get-Count -Text $text -Pattern '(\d+)\s*个错误'
    $pass = ($run.ExitCode -eq 0) -and ($warnings -eq 0) -and ($errors -eq 0)
    $numbers = '警告 {0} / 错误 {1} / 退出码 {2}' -f $warnings, $errors, $run.ExitCode
    return Complete-Gate -Title 'build' -Pass $pass -Numbers $numbers -Run $run -Rerun @(
        'dotnet build DocumentTools.sln --no-incremental -v m'
    )
}

function Test-TestGate {
    # -v n 只为抓「被跳过的用例名」：-v q 不打印个案，跳过数就只能看汇总里的一个数字，
    # 第 4 个静默跳过会藏在那一个数字里。
    $command = 'dotnet test tests\DocumentTools.Tests\DocumentTools.Tests.csproj -v n --nologo'
    Show-GateHeader -Index 2 -Title 'test' -Command $command
    $run = Invoke-Dotnet -Arguments @('test', 'tests\DocumentTools.Tests\DocumentTools.Tests.csproj', '-v', 'n', '--nologo')
    $text = $run.Output -join "`n"

    # 汇总行的两种写法都要认（换 -v n 之后 quiet 那行没了）：
    #   -v q：失败: 0，通过: 505，已跳过: 3，总计: 508
    #   -v n：测试总数: 508 / 通过数: 505 / 跳过数: 3（失败时另有 失败数: n）
    $failedCount = Get-Count -Text $text -Pattern '失败数:\s*(\d+)'
    if ($failedCount -lt 0) {
        $failedCount = Get-Count -Text $text -Pattern '失败:\s*(\d+)，通过:'
    }
    $passedCount = Get-Count -Text $text -Pattern '通过数:\s*(\d+)'
    if ($passedCount -lt 0) {
        $passedCount = Get-Count -Text $text -Pattern '通过:\s*(\d+)，已跳过:'
    }
    $skippedCount = Get-Count -Text $text -Pattern '跳过数:\s*(\d+)'
    if ($skippedCount -lt 0) {
        $skippedCount = Get-Count -Text $text -Pattern '已跳过:\s*(\d+)，总计:'
    }
    $totalCount = Get-Count -Text $text -Pattern '测试总数:\s*(\d+)'
    if ($totalCount -lt 0) {
        $totalCount = Get-Count -Text $text -Pattern '总计:\s*(\d+)'
    }
    if ($failedCount -lt 0 -and $run.ExitCode -eq 0) {
        # 全绿时 vstest 在某些版本上不打 失败数 行：没失败就是 0，别因此判「无法解析」。
        $failedCount = 0
    }
    if ($failedCount -lt 0 -and $totalCount -ge 0 -and $passedCount -ge 0 -and $skippedCount -ge 0) {
        # 没有 失败数 行但能拿到总数：用它反推，比直接报「无法解析」更有用（红的时候仍会被退出码拦住）。
        $derived = $totalCount - $passedCount - $skippedCount
        if ($derived -ge 0) { $failedCount = $derived }
    }

    # 两种写法都收：`已跳过 <FullName> [1 ms]` 与 xUnit 的 `<FullName> [SKIP]`。
    $skippedNames = New-Object System.Collections.Generic.List[string]
    foreach ($line in $run.Output) {
        $hit = [regex]::Match($line, '^\s*已跳过\s+([^\s\[]+)')
        if (-not $hit.Success) {
            $hit = [regex]::Match($line, '\]\s+([^\s\[]+)\s+\[SKIP\]')
        }
        if ($hit.Success) {
            $name = $hit.Groups[1].Value
            if (-not $skippedNames.Contains($name)) { $skippedNames.Add($name) }
        }
    }

    $pass = ($run.ExitCode -eq 0) -and ($failedCount -eq 0) -and ($skippedCount -ge 0) -and ($skippedCount -le $MaxSkips)
    if ($failedCount -lt 0) {
        $numbers = '未能解析测试汇总行（多半是编译失败）/ 退出码 {0}' -f $run.ExitCode
    }
    elseif ($skippedCount -le 0) {
        $numbers = '通过 {0} / 失败 {1} / 跳过 0 / 总计 {2}' -f $passedCount, $failedCount, $totalCount
    }
    else {
        $numbers = '通过 {0} / 失败 {1} / 跳过 {2}（上限 {3}）/ 总计 {4}' -f $passedCount, $failedCount, $skippedCount, $MaxSkips, $totalCount
    }

    if ($skippedNames.Count -gt 0) {
        # 上限是白名单不是免检额度：名字全打出来，第 4 条静默跳过就藏不住。
        foreach ($name in $skippedNames) { Write-GateLine ('跳过用例：{0}' -f $name) }
    }
    if ($skippedCount -gt $MaxSkips) {
        Write-GateLine '跳过数超限：先确认是「新加 opt-in 用例」还是「原本能跑的用例被环境挡掉」。'
        Write-GateLine '本仓库 opt-in 用例靠 DOCUMENTTOOLS_TEST_LOCAL_OFFICE=1 打开（会真拉起 WINWORD.EXE）。'
        Write-GateLine '若确认是新加的真 opt-in，再调 -MaxSkips；不要为了过门把它调大。'
    }
    return Complete-Gate -Title 'test' -Pass $pass -Numbers $numbers -Run $run -Rerun @(
        'dotnet test tests\DocumentTools.Tests\DocumentTools.Tests.csproj -v n'
    )
}

function Test-FeatureSmokeGate {
    $command = 'dotnet run --project tools\FeatureSmoke'
    Show-GateHeader -Index 3 -Title 'FeatureSmoke' -Command $command
    # 末尾的 -- 不能省：dotnet run 会把不认识的选项（-v q --nologo）原样转给应用，
    # 应用按未知参数退出（QaCompat 就因此退过 2），那会伪装成产品失败。
    $run = Invoke-Dotnet -Arguments @('run', '--project', 'tools\FeatureSmoke', '--')
    $text = $run.Output -join "`n"
    $tally = [regex]::Match($text, '功能总数\s*(\d+)，创建成功\s*(\d+)，布局通过\s*(\d+)')
    $start = [regex]::Match($text, '启动落点检查\s*(\d+)\s*项，失败\s*(\d+)')
    $featureTotal = -1
    $created = -1
    $rendered = -1
    if ($tally.Success) {
        $featureTotal = [int]$tally.Groups[1].Value
        $created = [int]$tally.Groups[2].Value
        $rendered = [int]$tally.Groups[3].Value
    }
    $startTotal = -1
    $startFailures = -1
    if ($start.Success) {
        $startTotal = [int]$start.Groups[1].Value
        $startFailures = [int]$start.Groups[2].Value
    }
    $pass = ($run.ExitCode -eq 0) -and ($featureTotal -gt 0) -and ($created -eq $featureTotal) -and
            ($rendered -eq $featureTotal) -and ($startFailures -eq 0)
    $numbers = '功能 {0} / 创建 {1} / 布局 {2} / 启动落点 {3} 项失败 {4} / 退出码 {5}' -f `
        $featureTotal, $created, $rendered, $startTotal, $startFailures, $run.ExitCode
    return Complete-Gate -Title 'FeatureSmoke' -Pass $pass -Numbers $numbers -Run $run -Rerun @(
        'dotnet run --project tools\FeatureSmoke -v n'
    )
}

function Get-UiSnapshotRerunCommands {
    # 从 _issues.csv 里挑 High/Medium 的功能 ID，拼一条能直接复制的 --only 命令。
    $issuesPath = Join-Path $RepoRoot 'docs\qa-screens\_issues.csv'
    if (-not (Test-Path -LiteralPath $issuesPath)) {
        return @('dotnet run --project tools\UiSnapshot')
    }
    try {
        $rows = @(Import-Csv -LiteralPath $issuesPath -Encoding UTF8)
    }
    catch {
        return @('dotnet run --project tools\UiSnapshot')
    }
    $hot = @($rows |
            Where-Object { $_.'严重度' -eq 'High' -or $_.'严重度' -eq 'Medium' } |
            ForEach-Object { $_.'功能ID' } |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
            Select-Object -Unique)
    if ($hot.Count -eq 0) {
        return @('dotnet run --project tools\UiSnapshot')
    }
    return @(('dotnet run --project tools\UiSnapshot -- --only {0}' -f ($hot -join ',')))
}

function Test-UiSnapshotGate {
    $command = 'dotnet run --project tools\UiSnapshot'
    Show-GateHeader -Index 4 -Title 'UiSnapshot' -Command $command

    # 上一轮被强杀时可能留下独占句柄文件，先清掉再跑，否则会误报 BUSY。
    $lockPath = Join-Path $RepoRoot 'docs\qa-screens\.running.lock'
    if (Test-Path -LiteralPath $lockPath) {
        Write-GateLine ('提示：发现残留锁文件 {0}，先删除。' -f $lockPath)
        Remove-Item -LiteralPath $lockPath -Force -ErrorAction SilentlyContinue
        if (Test-Path -LiteralPath $lockPath) {
            Write-GateLine '锁文件删不掉，说明另一个 UiSnapshot 正在写同一目录，本轮判失败。'
            return Complete-Gate -Title 'UiSnapshot' -Pass $false -Numbers '锁文件 .running.lock 无法删除' -Run $null `
                -Rerun @('dotnet run --project tools\UiSnapshot')
        }
    }

    $run = Invoke-Dotnet -Arguments @('run', '--project', 'tools\UiSnapshot', '--')
    $text = $run.Output -join "`n"
    if ($text -match 'BUSY\|') {
        Write-GateLine '检测到 BUSY：另一个 UiSnapshot 正在写 docs\qa-screens。'
        return Complete-Gate -Title 'UiSnapshot' -Pass $false -Numbers 'BUSY（输出目录被别的实例占用）' -Run $run `
            -Rerun @('dotnet run --project tools\UiSnapshot')
    }
    $summary = [regex]::Match($text, 'SUMMARY pages=(\d+) rendered=(\d+) fatal=(\d+) high=(\d+) medium=(\d+) low=(\d+) oversized=(\d+)')
    $pages = -1
    $rendered = -1
    $fatal = -1
    $high = -1
    $medium = -1
    $low = -1
    $oversized = -1
    if ($summary.Success) {
        $pages = [int]$summary.Groups[1].Value
        $rendered = [int]$summary.Groups[2].Value
        $fatal = [int]$summary.Groups[3].Value
        $high = [int]$summary.Groups[4].Value
        $medium = [int]$summary.Groups[5].Value
        $low = [int]$summary.Groups[6].Value
        $oversized = [int]$summary.Groups[7].Value
    }
    # low 是观察项：只记录不判失败，但必须进汇总，免得它慢慢变成垃圾桶。
    $pass = ($run.ExitCode -eq 0) -and ($fatal -eq 0) -and ($high -eq 0) -and ($medium -eq 0)
    if (-not $summary.Success) {
        $numbers = '未能解析 SUMMARY 行 / 退出码 {0}' -f $run.ExitCode
    }
    else {
        $numbers = 'pages {0} / rendered {1} / fatal {2} / high {3} / medium {4} / low {5}（观察项）/ oversized {6}' -f `
            $pages, $rendered, $fatal, $high, $medium, $low, $oversized
    }
    $rerun = @(Get-UiSnapshotRerunCommands)
    return Complete-Gate -Title 'UiSnapshot' -Pass $pass -Numbers $numbers -Run $run -Rerun $rerun
}

function Test-QaFunctionGate {
    $command = 'dotnet run --project tools\QaFunction -- --bonus'
    Show-GateHeader -Index 5 -Title 'QaFunction' -Command $command
    $run = Invoke-Dotnet -Arguments @('run', '--project', 'tools\QaFunction', '--', '--bonus')
    $text = $run.Output -join "`n"
    $required = [regex]::Match($text, '必测\s*(\d+)\s*项：PASS\s*(\d+)\s*/\s*FAIL\s*(\d+)')
    $extended = [regex]::Match($text, '扩展\s*(\d+)\s*项：PASS\s*(\d+)\s*/\s*FAIL\s*(\d+)')
    $bonus = [regex]::Match($text, '追加\s*(\d+)\s*项：PASS\s*(\d+)\s*/\s*FAIL\s*(\d+)')
    $failLines = @($run.Output | Where-Object { $_ -match '^\[FAIL\]' })
    $skipLines = @($run.Output | Where-Object { $_ -match '^\[SKIP\]' })
    $pass = ($run.ExitCode -eq 0) -and $required.Success -and $extended.Success -and $bonus.Success -and
            ($failLines.Count -eq 0) -and ($skipLines.Count -eq 0)
    $numbers = '必测 {0} / 扩展 {1} / 追加 {2} / FAIL 行 {3} / SKIP 行 {4} / 退出码 {5}' -f `
        $(if ($required.Success) { '{0} 项 {1} PASS' -f $required.Groups[1].Value, $required.Groups[2].Value } else { '未解析' }), `
        $(if ($extended.Success) { '{0} 项 {1} PASS' -f $extended.Groups[1].Value, $extended.Groups[2].Value } else { '未解析' }), `
        $(if ($bonus.Success) { '{0} 项 {1} PASS' -f $bonus.Groups[1].Value, $bonus.Groups[2].Value } else { '未解析' }), `
        $failLines.Count, $skipLines.Count, $run.ExitCode

    $rerun = @()
    if ($failLines.Count -gt 0) {
        $ids = @($failLines | ForEach-Object { ($_ -split '\|')[1] } | Where-Object { $_ } | ForEach-Object { $_.Trim() } | Select-Object -Unique)
        if ($ids.Count -gt 0) {
            $rerun += ('dotnet run --project tools\QaFunction -- --bonus --filter "{0}"' -f ($ids -join ','))
        }
    }
    if ($rerun.Count -eq 0) {
        $rerun += 'dotnet run --project tools\QaFunction -- --bonus'
    }
    return Complete-Gate -Title 'QaFunction' -Pass $pass -Numbers $numbers -Run $run -Rerun $rerun
}

function Test-QaCompatGate {
    $command = 'dotnet run --project tools\QaCompat -- --all'
    Show-GateHeader -Index 6 -Title 'QaCompat' -Command $command
    $run = Invoke-Dotnet -Arguments @('run', '--project', 'tools\QaCompat', '--', '--all')
    $text = $run.Output -join "`n"
    $tally = [regex]::Match($text, 'PASS\s*(\d+)\s*/\s*FAIL\s*(\d+)\s*/\s*无法验证\s*(\d+)')
    $passCount = -1
    $failCount = -1
    $unverified = -1
    if ($tally.Success) {
        $passCount = [int]$tally.Groups[1].Value
        $failCount = [int]$tally.Groups[2].Value
        $unverified = [int]$tally.Groups[3].Value
    }
    # 口径是「无新增 FAIL」，不是「FAIL=0」：基线名单只保留「本机无法验证」的项。
    # ace.missing-provider / ace.simulate-switch / html.no-browser 已随 Core 文案同步修好（2026-11-05），从名单移除。
    # 修掉之后从名单里删掉即可；如果出现名单外的 FAIL，闸门必须红。
    $knownCompatFailures = @(
    )
    $currentFailures = @()
    foreach ($line in $run.Output) {
        $match = [regex]::Match($line, '^\s*([A-Za-z][A-Za-z0-9._-]*)\s+FAIL\s')
        if ($match.Success) {
            $currentFailures += $match.Groups[1].Value
        }
    }
    $newFailures = @($currentFailures | Where-Object { $knownCompatFailures -notcontains $_ })
    $fixedFailures = @($knownCompatFailures | Where-Object { $currentFailures -notcontains $_ })
    $parseable = $tally.Success -and ($currentFailures.Count -eq $failCount -or $failCount -eq 0)
    $pass = $parseable -and ($newFailures.Count -eq 0) -and ($run.ExitCode -le 1)

    if (-not $tally.Success) {
        $numbers = '未能解析 [QaCompat] 汇总行 / 退出码 {0}' -f $run.ExitCode
    }
    else {
        $numbers = 'PASS {0} / FAIL {1}（新增 {2}，基线项 {3}）/ 无法验证 {4}' -f `
            $passCount, $failCount, $newFailures.Count, ($currentFailures.Count - $newFailures.Count), $unverified
    }
    if ($newFailures.Count -gt 0) {
        Write-GateLine ('新增 FAIL（不在基线名单里）：{0}' -f ($newFailures -join '、'))
    }
    if ($fixedFailures.Count -gt 0) {
        Write-GateLine ('基线项已转 PASS：{0}（记得从 gate.ps1 的基线名单里删掉）' -f ($fixedFailures -join '、'))
    }
    return Complete-Gate -Title 'QaCompat' -Pass $pass -Numbers $numbers -Run $run -Rerun @(
        'dotnet run --project tools\QaCompat -- --all --work testdata\compat\work\manual --report docs\qa-report-compat.md'
    )
}

function Write-GateSummary {
    param($FailedResult)
    Write-GateLine ''
    Write-GateLine '=== 汇总 ==='
    Write-GateLine ('{0,-3} {1,-14} {2,-6} {3}' -f '#', '门', '结论', '关键数字 / 耗时')
    $index = 0
    foreach ($result in $Results) {
        $index++
        $verdict = 'FAIL'
        if ($result.Pass) { $verdict = 'PASS' }
        Write-GateLine ('{0,-3} {1,-14} {2,-6} {3} / {4}s' -f $index, $result.Title, $verdict, $result.Numbers, $result.Seconds)
    }
    Write-GateLine ''
    if ($null -eq $FailedResult) {
        Write-GateLine '总判定：PASS（六道门全过）'
    }
    else {
        Write-GateLine ('总判定：FAIL（停在门「{0}」）' -f $FailedResult.Title)
        foreach ($command in $FailedResult.Rerun) {
            Write-GateLine ('复跑：{0}' -f $command)
        }
    }
}

function Write-GateLog {
    $builder = New-Object System.Text.StringBuilder
    [void]$builder.AppendLine(('gate 日志 {0}' -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss')))
    [void]$builder.AppendLine(('仓库 {0}' -f $RepoRoot))
    [void]$builder.AppendLine('')
    foreach ($result in $Results) {
        $verdict = 'FAIL'
        if ($result.Pass) { $verdict = 'PASS' }
        [void]$builder.AppendLine(('--- {0} · {1} · {2} · {3}s ---' -f $result.Title, $verdict, $result.Numbers, $result.Seconds))
        foreach ($line in $result.Tail) {
            [void]$builder.AppendLine($line)
        }
        [void]$builder.AppendLine('')
    }
    [void]$builder.AppendLine('--- 闸门自身输出（尾部 20 行）---')
    foreach ($line in @($GateText | Select-Object -Last 20)) {
        [void]$builder.AppendLine($line)
    }
    [System.IO.File]::WriteAllText($LogFile, $builder.ToString(), (New-Object System.Text.UTF8Encoding($false)))
    Write-GateLine ('日志：{0}' -f $LogFile)
}

# ---- 主流程 ----
Push-Location -LiteralPath $RepoRoot
try {
    Write-GateLine ('发布闸门 · 仓库 {0}' -f $RepoRoot)
    Write-GateLine ('跳过上限 {0}（opt-in 用例不计入正常基线）/ 日志目录 {1}' -f $MaxSkips, $LogDirectory)
    Write-GateLine ('开始 {0}' -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'))

    $blocking = @(Get-BlockingProcesses)
    if ($blocking.Count -gt 0) {
        Write-GateLine ''
        Write-GateLine '有进程正在占用构建产物或截图目录，先关掉它们再跑（否则失败会被误读成代码坏了）：'
        foreach ($item in $blocking) {
            Write-GateLine ('  - {0}' -f $item)
        }
        Write-GateLine '提示：GUI 关掉即可；testhost 是别人正在跑测试，等它结束。'
        Write-GateLine ''
        Write-GateLine '总判定：ABORT（未进入任何门）'
        exit 3
    }

    $failedResult = $null
    $gates = @(
        'Test-BuildGate'
        'Test-TestGate'
        'Test-FeatureSmokeGate'
        'Test-UiSnapshotGate'
        'Test-QaFunctionGate'
        'Test-QaCompatGate'
    )

    try {
        foreach ($gate in $gates) {
            $passed = & $gate
            if (-not $passed) {
                $failedResult = $Results[$Results.Count - 1]
                break
            }
        }
    }
    catch {
        # 闸门自己出问题时也要给结论：否则调用方只看到非 0 退出码，不知道该修哪。
        Write-GateLine ''
        Write-GateLine ('闸门被异常打断：{0}' -f $_.Exception.Message)
        if ($Results.Count -gt 0) {
            $failedResult = $Results[$Results.Count - 1]
        }
        else {
            $failedResult = [pscustomobject]@{ Title = '闸门自身'; Rerun = @() }
        }
    }

    Write-GateSummary -FailedResult $failedResult
    Write-GateLine ('结束 {0}' -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'))
    Write-GateLog

    if ($null -eq $failedResult) {
        exit 0
    }
    exit 1
}
finally {
    Pop-Location
}
