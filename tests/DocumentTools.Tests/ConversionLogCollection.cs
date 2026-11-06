namespace DocumentTools.Tests;

/// <summary>
/// 会改动 <c>ConversionLog.LogDirectoryVariable</c> 的测试必须共用这个 collection，且禁止并行。
/// 那个环境变量是**进程级**的：并行跑时 A 用例设的日志目录会被 B 用例读到，
/// 结果 A 的断言在自己的目录里读不到自己的日志行（曾表现为 <c>Assert.Single</c> 收到 3 条，
/// 单跑必过、与其它 opt-in 用例同跑必红）。
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ConversionLogCollection
{
    public const string Name = "ConversionLog";
}
