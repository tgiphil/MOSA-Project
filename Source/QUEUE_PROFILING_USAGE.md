# Using Queue Profiling - Practical Example

## Step 1: Run Your Compilation

The queue profiling is now **automatically enabled** in MethodScheduler. Just run your normal compilation:

```csharp
var compiler = new MosaCompiler(mosaSettings, compilerHooks, moduleLoader, typeResolver);
compiler.Load();
compiler.Initialize();
compiler.Setup();
compiler.ScheduleAll();
compiler.ExecuteCompile();  // or compiler.ThreadedCompile()
```

## Step 2: Monitor Console Output During Compilation

You'll see periodic queue status reports every 2 seconds:

```
[Queue] Size: 245 | Peak: 512 | Empty Events: 0 | Enqueue Rate: 178.5/s | Dequeue Rate: 165.2/s | Total Enqueued: 2456 | Total Dequeued: 2211
[Queue] Size: 198 | Peak: 512 | Empty Events: 0 | Enqueue Rate: 172.3/s | Dequeue Rate: 171.8/s | Total Enqueued: 3102 | Total Dequeued: 2904
[Queue] Size: 156 | Peak: 512 | Empty Events: 1 | Enqueue Rate: 168.1/s | Dequeue Rate: 169.4/s | Total Enqueued: 3689 | Total Dequeued: 3533
```

### What to Watch For:

**✅ Healthy Pattern (Good CPU Utilization):**
- Queue Size stays above 100 during active compilation
- Empty Events increases by 0 or 1 only at the very end
- Enqueue Rate ≈ Dequeue Rate (balanced)

**⚠️ Starvation Pattern (Poor CPU Utilization):**
- Queue Size frequently drops below 50
- Empty Events increases multiple times during compilation
- Dequeue Rate consistently higher than Enqueue Rate

## Step 3: Watch for Starvation Warnings

If the queue empties during compilation, you'll see:

```
[Queue Starvation] Queue became empty! Threads may be idle. Empty count: 5 | Peak size was: 450 | Total methods: 2500 | Completed: 1832
```

**This means:**
- All queued work is done
- Worker threads are now idle (wasting CPU)
- Only 1832 out of 2500 methods completed
- More methods will be added later (from inlining), but there's a gap

## Step 4: Get Final Statistics

After compilation completes, call:

```csharp
var queueStats = compiler.MethodScheduler.GetQueueStatisticsSummary();
Console.WriteLine(queueStats);
```

**Output:**
```
Queue Statistics Summary:
  Total Methods Scheduled: 5000
  Peak Queue Size: 450
  Queue Empty Events: 12
  Total Enqueue Operations: 5000
  Total Dequeue Operations: 5000
  Average Enqueue Rate: 186.52 methods/sec
  Average Dequeue Rate: 186.52 methods/sec
  Total Elapsed Time: 26.81 seconds
```

## Step 5: Interpret Results

### Example 1: Healthy Queue (Good Utilization)

```
Queue Statistics Summary:
  Total Methods Scheduled: 5000
  Peak Queue Size: 850           ✅ High peak = good backlog
  Queue Empty Events: 1           ✅ Only empty at the very end
  Total Enqueue Operations: 5000
  Total Dequeue Operations: 5000
  Average Enqueue Rate: 195.5 methods/sec
  Average Dequeue Rate: 195.5 methods/sec  ✅ Balanced rates
  Total Elapsed Time: 25.58 seconds
```

**Diagnosis:** ✅ Excellent! All cores being fully utilized.

---

### Example 2: Starving Queue (Poor Utilization)

```
Queue Statistics Summary:
  Total Methods Scheduled: 5000
  Peak Queue Size: 75            ❌ Low peak = insufficient backlog
  Queue Empty Events: 18         ❌ Queue emptied 18 times!
  Total Enqueue Operations: 5000
  Total Dequeue Operations: 5000
  Average Enqueue Rate: 98.2 methods/sec  ❌ Much slower than dequeue
  Average Dequeue Rate: 164.7 methods/sec
  Total Elapsed Time: 50.92 seconds       ❌ 2x slower!
```

**Diagnosis:** ❌ Work starvation! Cores idle frequently.

**Root Cause:** Methods being scheduled too slowly (98/sec) compared to compilation speed (165/sec)

---

## Step 6: Cross-Reference with CPU Usage

Use Task Manager (Windows) or `htop` (Linux) to monitor CPU usage during compilation:

**Healthy Pattern:**
- CPU usage: 85-95% across all cores
- Queue empty events: 0-2
- Peak queue size: > 200

**Starvation Pattern:**
- CPU usage: 30-60% (cores idle!)
- Queue empty events: > 5
- Peak queue size: < 100

## Step 7: Identifying the Bottleneck

If you have work starvation, find out why:

### A. Check Initial Scheduling

Add logging to `ScheduleAll()`:

```csharp
Console.WriteLine($"[Scheduling] Starting ScheduleAll for {typeSystem.AllTypes.Count} types");
methodScheduler.ScheduleAll(typeSystem);
Console.WriteLine($"[Scheduling] Initial queue size: {methodScheduler.TotalQueuedMethods}");
```

**What to look for:**
- If initial queue size < 200: Not enough upfront scheduling
- If initial queue size > 500: Good! Problem is elsewhere

### B. Track Inline-Driven Scheduling

The InlineEvaluationStage may be adding methods slowly. Check when queue empties:

```
Time: 5.2s  - [Queue] Size: 125 | Empty Events: 0
Time: 7.8s  - [Queue Starvation] Queue became empty! Threads may be idle.
Time: 8.1s  - [Queue] Size: 45 | Empty Events: 1    ← New methods added from inlining
Time: 10.5s - [Queue Starvation] Queue became empty! Threads may be idle.
Time: 10.7s - [Queue] Size: 22 | Empty Events: 2
```

**Pattern:** Queue empties, then slowly refills. This means:
- Inlining is discovering new methods
- But only adding them one-at-a-time (sequential)
- **Solution:** Batch the discovered methods before scheduling

## Step 8: Common Fixes

### Fix 1: More Aggressive Upfront Scheduling

In `ScheduleAll()`, don't defer scheduling:

```csharp
public void ScheduleAll(TypeSystem typeSystem)
{
    foreach (var type in typeSystem.AllTypes)
    {
        if (!IsCompilable(type))
            continue;
            
        // Schedule ALL methods immediately, not just referenced ones
        foreach (var method in type.Methods)
        {
            Schedule(method);
        }
    }
}
```

**Expected improvement:**
- Initial queue size: 500-1000 (up from < 100)
- Empty events: 1-2 (down from > 10)
- CPU usage: 85%+ (up from 40-60%)

### Fix 2: Batch Inline-Discovered Methods

When inlining discovers new methods, collect them and schedule as a batch:

```csharp
var discoveredMethods = new HashSet<MosaMethod>();

// ... during inline evaluation ...
discoveredMethods.Add(methodToInline);

// After processing all inline candidates:
if (discoveredMethods.Count > 0)
{
    MethodScheduler.Add(discoveredMethods);  // Batch add
}
```

**Expected improvement:**
- Queue refills faster after emptying
- Fewer empty events
- Smoother queue size curve

### Fix 3: Adjust Priority Algorithm

If small methods are dominating the queue and draining it too fast:

```csharp
private static int GetCompilePriorityLevel(MethodData methodData)
{
    // ... existing code ...
    
    // REDUCE penalties for large methods to keep queue balanced
    if (methodData.VirtualCodeSize > 100)
        adjustment -= 30;  // Was -75, now -30
        
    if (methodData.VirtualCodeSize > 50)
        adjustment -= 20;  // Was -50, now -20
        
    // ... rest of code ...
}
```

**Expected improvement:**
- Mix of small and large methods in queue
- More stable queue size
- Better CPU utilization across threads

## Step 9: Verify the Fix

After making changes:

1. **Run compilation again**
2. **Compare metrics:**

| Metric | Before Fix | After Fix | Target |
|--------|-----------|-----------|--------|
| Peak Queue Size | 75 | 520 ✅ | > 200 |
| Empty Events | 18 | 2 ✅ | < 5 |
| Avg Enqueue Rate | 98/s | 188/s ✅ | ≈ Dequeue Rate |
| Avg Dequeue Rate | 165/s | 185/s ✅ | ≈ Enqueue Rate |
| Total Time | 50.9s | 27.0s ✅ | Minimize |
| CPU Usage | 45% | 92% ✅ | > 85% |

**Success!** Queue starvation eliminated, CPU fully utilized, compilation 47% faster!

## Summary Checklist

- [x] Queue profiling automatically enabled
- [x] Monitor console output during compilation
- [x] Look for starvation warnings
- [x] Get final statistics after compilation
- [x] Compare queue metrics with CPU usage
- [x] Identify bottleneck (upfront scheduling, inline discovery, priority)
- [x] Apply appropriate fix
- [x] Re-run and verify improvement

## Next Steps

If after fixing queue starvation you still have performance issues:
- The bottleneck is now in the actual compilation work (good problem to have!)
- Use the CPU profiler to find hot spots in transformation stages
- Optimize the ClrMetadataResolver (14% CPU in previous profile)
- Optimize transformation logic (12% CPU in previous profile)
