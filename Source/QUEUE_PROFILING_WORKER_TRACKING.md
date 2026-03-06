# Queue Profiling Enhancement: Active Worker Tracking

## What Was Added

Enhanced the queue profiling system to track **active worker threads** alongside queue metrics, providing complete visibility into CPU utilization.

## New Metrics

### 1. Active Worker Count
- Number of threads currently compiling methods
- Updated in real-time as workers start/finish work

### 2. Worker Utilization Percentage
- **Active Workers / Total Workers × 100**
- Direct indicator of CPU core usage
- Example: `Active: 8/16 (50.0%)` = 50% CPU utilization

### 3. Idle Worker Count
- Number of threads waiting for work
- **High idle count = wasted CPU capacity**
- Example: `Idle: 8` means 8 cores sitting idle

## Updated Console Output

### Before Enhancement
```
[Queue] Size: 125 | Peak: 450 | Empty Events: 3 | Enqueue Rate: 156.3/s | Dequeue Rate: 152.1/s
```

### After Enhancement
```
[Queue] Size: 125 | Peak: 450 | Empty Events: 3 | Active: 12/16 (75.0%) | Idle: 4 | Enqueue: 156.3/s | Dequeue: 152.1/s
```

**New Information:**
- `Active: 12/16` - 12 out of 16 worker threads are active
- `(75.0%)` - 75% worker utilization (expect ~75% CPU usage)
- `Idle: 4` - 4 threads are idle waiting for work

### Starvation Warnings Enhanced
```
[Queue Starvation] Queue became empty! Active: 2/16 | Idle: 14 threads waiting for work | Empty count: 5 | Peak size was: 450
```

**This clearly shows:**
- Only 2 threads still working
- 14 threads (87.5%) are completely idle
- **Explains why CPU is underutilized!**

## Code Changes Summary

### 1. PipelinePool.cs
**Added public properties:**
- `ActiveWorkers` - Exposes the `active` field (thread-safe)
- `MaxWorkers` - Returns total worker thread count

```csharp
public int ActiveWorkers => Volatile.Read(ref active);
public int MaxWorkers { get; }
```

### 2. MethodScheduler.cs
**Added:**
- `PipelinePool pipelinePool` field
- `SetPipelinePool(PipelinePool)` method
- Active worker metrics in reporting methods
- Worker utilization calculation

**Modified methods:**
- `ReportQueueStatus()` - Now includes active/idle worker counts and utilization %
- `ReportQueueStarvation()` - Now shows how many threads are idle
- `GetQueueStatisticsSummary()` - Includes average worker utilization

### 3. Compiler.cs
**Modified:**
- `ExecuteCompile()` - Calls `MethodScheduler.SetPipelinePool(pool)` after creating PipelinePool

## Why This Matters for Your 50% CPU Issue

### Direct Correlation: Worker Utilization = CPU Usage

| Worker Utilization | Expected CPU Usage | Your Scenario |
|-------------------|-------------------|---------------|
| 100% | 95-100% | |
| 75% | 70-80% | |
| **50%** | **45-55%** | **← Your reported CPU!** |
| 25% | 20-30% | |

**Your 50% CPU usage likely means ~50% worker utilization!**

### What You'll Discover

Running with enhanced profiling will show:

**If work starvation is the problem:**
```
[Queue] Size: 45 | Active: 8/16 (50.0%) | Idle: 8
```
✅ Confirmed! Only 8 of 16 cores working = 50% CPU

**If NOT work starvation:**
```
[Queue] Size: 250 | Active: 16/16 (100%) | Idle: 0
```
❌ All cores busy, but CPU still at 50%? Problem is elsewhere (IO, locks, external library)

## Using the Enhanced Profiling

### Step 1: Run Your Compilation
Just run normally - profiling is automatic:
```csharp
compiler.ScheduleAll();
compiler.ExecuteCompile();
```

### Step 2: Watch for Utilization Metrics
Every 2 seconds you'll see:
```
[Queue] Size: X | Active: Y/Z (W%) | Idle: I
```

**Key question: What is W% (utilization) during active compilation?**

- **> 85%** = ✅ Excellent CPU usage, problem is elsewhere
- **60-85%** = ⚠️ Some work starvation, room for improvement
- **< 60%** = ❌ Significant work starvation, fix scheduling

### Step 3: Check Idle Worker Count

During active compilation (middle phase), check idle count:

- **0-2 idle** = ✅ Almost all cores busy
- **3-5 idle** = ⚠️ Some waste, can improve
- **> 5 idle** = ❌ Major waste, many cores sitting idle

### Step 4: Correlate with Queue Size

**Problem Pattern:**
```
[Queue] Size: 25 | Active: 6/16 (37.5%) | Idle: 10
```
**Diagnosis:** Queue too small (25), so 10 workers idle = low CPU

**Solution:** Increase queue depth by scheduling more aggressively

## Expected Findings

### Hypothesis: Work Starvation Causing 50% CPU

**What you should see:**
1. Worker utilization hovering around 50% during active compilation
2. Idle worker count around 8 (half your threads)
3. Queue size frequently < 100
4. Multiple starvation events

**Confirmation:**
```
[Queue] Size: 45 | Active: 8/16 (50.0%) | Idle: 8
[Queue Starvation] Active: 5/16 | Idle: 11 threads waiting for work
```

**Next steps:** Fix scheduling to keep queue full

### Alternative: Not Work Starvation

**What you'd see:**
1. Worker utilization consistently 90%+
2. Idle worker count 0-1
3. Queue size healthy (> 100)
4. No starvation events

**But CPU still at 50%?**

**Then problem is NOT work starvation. Investigate:**
- Lock contention (use earlier lock profiling)
- External library bottleneck (dnlib - 12% CPU in earlier profile)
- IO operations
- Memory bandwidth

## Summary

### Before Enhancement
- Could see queue depth
- Could infer starvation from empty events
- Could NOT directly see CPU utilization

### After Enhancement
- ✅ Direct visibility into worker utilization
- ✅ Clear idle worker count
- ✅ **Definitive answer: Is work starvation causing low CPU?**

### Your Next Step

1. **Run the enhanced profiling**
2. **Look at the utilization %** during active compilation
3. **If ~50% utilization** → Work starvation confirmed, fix scheduling
4. **If 90%+ utilization** → Problem is elsewhere, investigate locks/IO/external libs

The enhanced profiling will give you a **definitive answer** about whether your 50% CPU usage is due to work starvation or something else!

## Files Modified

- `Mosa.Compiler.Framework\PipelinePool.cs` - Added ActiveWorkers and MaxWorkers properties
- `Mosa.Compiler.Framework\MethodScheduler.cs` - Added worker tracking and reporting
- `Mosa.Compiler.Framework\Compiler.cs` - Connected PipelinePool to MethodScheduler
- `QUEUE_PROFILING_QUICK_REFERENCE.md` - Updated with worker utilization guidance

## Build Status

✅ All changes compile successfully
✅ No breaking changes to public API
✅ Ready to run and diagnose!
