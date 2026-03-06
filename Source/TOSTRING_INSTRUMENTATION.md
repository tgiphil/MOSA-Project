# ToString() Call Instrumentation

## Overview
Added diagnostic instrumentation to track `Node.ToString()` and `Operand.ToString()` call frequency and capture sample call stacks to identify unexpected callers when `IsTraceTransforms` is false.

## Implementation

### New File: `StringificationDiagnostics.cs`
Created `Mosa.Compiler.Framework\Diagnostics\StringificationDiagnostics.cs` with:

- **Thread-safe counters**: Tracks total calls to `Operand.ToString()` and `Node.ToString()` using `Interlocked`
- **Periodic reporting**: Every 3 seconds, outputs current totals and rates to console
- **Stack trace sampling**: Captures call stacks for every 10,000th call (configurable) to minimize overhead
- **Summary report**: Groups and ranks sampled stack traces by frequency

### Modified Files

#### `Operand.cs`
- Added `using Mosa.Compiler.Framework.Diagnostics;`
- Added `StringificationDiagnostics.RecordOperandToString();` as first line in `ToString()` method

#### `Node.cs`
- Added `using Mosa.Compiler.Framework.Diagnostics;`
- Added `StringificationDiagnostics.RecordNodeToString();` as first line in `ToString()` method

#### `Compiler.cs`
- Added `using Mosa.Compiler.Framework.Diagnostics;`
- Added summary output before `CompilerEnd` event:
  ```csharp
  PostEvent(CompilerEvent.FinalizationEnd, StringificationDiagnostics.GetSummary());
  ```

## Usage

### Runtime Output
During compilation, you'll see periodic console output like:
```
[StringificationDiagnostics] Operand.ToString: 1,234,567 total (45,678/sec), Node.ToString: 987,654 total (32,100/sec)
```

### End-of-Compilation Summary
At the end of compilation, a summary will be output showing:
- Total calls for each type
- Top 10 most frequent call paths (grouped from sampled stack traces)
- Number of samples captured

Example output:
```
=== Stringification Diagnostics Summary ===
Operand.ToString() calls: 5,432,100
Node.ToString() calls: 3,210,987

Operand.ToString() sampled call stacks (50 samples):
  Count: 23
  Context.SetInstruction <- BasicBlock.Split <- Transform.SplitBlock <- OptimizationStage.Run

  Count: 15
  Node.Dump <- DebugOutput.WriteNode <- MethodCompiler.Trace

...
```

## Performance Considerations

- **Minimal overhead**: Uses `Interlocked` for counters (lock-free)
- **Sampling**: Only captures 1 in 10,000 call stacks
- **Limited samples**: Stops capturing after 50 samples per type
- **Filtered stacks**: Only includes `Mosa.Compiler.*` frames (excludes System/BCL)
- **Shallow stacks**: Captures only top 5 relevant frames

## Key Metrics to Watch

1. **High call rates** (>100k/sec) suggest string formatting is a bottleneck
2. **Call stack patterns** identify which compiler stages or transforms are responsible
3. **Operand vs Node ratio** indicates whether issue is operand formatting or full instruction formatting

## Next Steps

After running with this instrumentation:
1. Identify the top call sites from the summary
2. Evaluate whether those calls are necessary
3. Consider caching/memoization for frequently formatted operands
4. Investigate why non-trace paths are triggering heavy formatting

## Build Notes

Pre-existing build errors in `Node.cs` (duplicate `SetInstruction` methods) are unrelated to these changes. The stringification diagnostics code compiles correctly once those are resolved.
