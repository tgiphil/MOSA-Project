# Queue Profiling Quick Reference

## What You'll See During Compilation

### 1. Periodic Queue Status (every 2 seconds)
```
[Queue] Size: 125 | Peak: 450 | Empty Events: 3 | Active: 12/16 (75.0%) | Idle: 4 | Enqueue: 156.3/s | Dequeue: 152.1/s
```

**New Metrics:**
- **Active: 12/16** - 12 threads actively compiling out of 16 total
- **(75.0%)** - 75% worker utilization
- **Idle: 4** - 4 threads waiting for work

### 2. Starvation Warnings (when queue empties)
```
[Queue Starvation] Queue became empty! Active: 2/16 | Idle: 14 threads waiting for work | Empty count: 5 | Peak size was: 450
```

**This clearly shows:**
- Only 2 threads still have work
- 14 threads (87.5%) are idle waiting
- **This is why CPU usage is only 50%!**

## Quick Diagnosis Table

| Metric | Good ✅ | Warning ⚠️ | Critical ❌ |
|--------|---------|-----------|-----------|
| **Queue Empty Events** | 0-2 | 3-10 | > 10 |
| **Peak Queue Size** | > 200 | 50-200 | < 50 |
| **Current Queue Size** (during active compilation) | > 100 | 20-100 | < 20 |
| **Worker Utilization** | > 85% | 60-85% | < 60% |
| **Idle Workers** (during active compilation) | 0-2 | 3-5 | > 5 |
| **Enqueue vs Dequeue Rate** | Enqueue ≥ Dequeue | Enqueue = 0.8-0.9 × Dequeue | Enqueue < 0.7 × Dequeue |

## What Worker Utilization Means

| Utilization | CPU Usage (expected) | Interpretation |
|-------------|---------------------|----------------|
| **90-100%** | 85-95% | ✅ Excellent! All cores busy |
| **75-90%** | 70-85% | ⚠️ Good, some idle time during queue refill |
| **50-75%** | 45-70% | ⚠️ Suboptimal, moderate work starvation |
| **< 50%** | < 45% | ❌ Poor! Half your cores idle most of the time |

**Your reported CPU at 50%** likely correlates with **~50% worker utilization**.

## Expected Queue & Worker Pattern

### Phase 1: Initial Burst (First 5-10 seconds)
- Queue size rapidly increases: 0 → 500+
- Worker utilization: 20-40% (building backlog)
- High enqueue rate, low dequeue rate
- **This is good!** Building up work backlog

### Phase 2: Steady State (Middle of compilation)
- Queue size stays relatively stable: 100-300
- **Worker utilization: 85-95%** ← This is what you want!
- **Active: 15/16 workers** ← All cores busy
- **Idle: 0-1 workers** ← Minimal idle time
- Empty events should be 0
- **All CPU cores working!**

### Phase 3: Wind Down (Last 10-20 seconds)
- Queue size gradually decreases: 200 → 0
- Worker utilization drops: 90% → 50% → 10%
- Active workers decrease as work completes
- May see 1-2 empty events as work completes
- **This is expected**

## Red Flags 🚩

1. **Worker utilization < 60% during Phase 2**
   - **Direct correlation to low CPU usage!**
   - More than 40% of cores idle
   - Work starvation confirmed

2. **Idle workers > 5 during active compilation**
   - Queue can't keep workers fed
   - Cores going idle unnecessarily

3. **Queue empties while workers drop from 16 → 5 → 2**
   - Workers finishing, but no new work arriving
   - Classic starvation pattern

4. **Utilization oscillates: 90% → 30% → 80% → 20%**
   - Bursty scheduling (bad)
   - Queue fills in bursts, then drains completely

## Diagnosing Low CPU Usage

**Your Scenario: CPU at 50%**

**Look for these patterns in queue output:**

### Pattern 1: Low Worker Utilization
```
[Queue] Size: 45 | Peak: 120 | Active: 8/16 (50.0%) | Idle: 8
```
**Diagnosis:** ✅ Confirmed! Only 50% of workers active = 50% CPU usage
**Root Cause:** Not enough work in queue to keep all cores busy

### Pattern 2: Frequent Starvation
```
[Queue Starvation] Active: 4/16 | Idle: 12 threads waiting for work
[Queue Starvation] Active: 2/16 | Idle: 14 threads waiting for work  
[Queue Starvation] Active: 1/16 | Idle: 15 threads waiting for work
```
**Diagnosis:** ✅ Queue keeps emptying, workers go idle
**Root Cause:** Scheduling can't keep pace with compilation

### Pattern 3: Low Queue Size, High Idle Count
```
[Queue] Size: 12 | Active: 5/16 (31.2%) | Idle: 11
```
**Diagnosis:** ✅ Queue nearly empty, 11 workers idle = ~31% CPU
**Root Cause:** Insufficient work backlog

## Solutions by Problem

### Problem: Low Worker Utilization (< 60%)
**Your issue: CPU at 50% suggests ~50% utilization**

**Likely causes:**
- Queue not staying full enough
- Methods being scheduled too slowly
- Queue draining faster than filling

**Solutions:**
1. **Increase initial queue size** (schedule more upfront)
2. **Batch-schedule discovered methods** (from inlining)
3. **Adjust priority algorithm** (balance small/large methods)

### Problem: High Idle Worker Count (> 5 during active compilation)
**Likely causes:**
- Work starvation
- Queue empty or nearly empty

**Solutions:**
1. Monitor when idle count spikes
2. Correlate with queue empty events
3. Fix underlying scheduling rate issue

### Problem: Utilization < Peak Queue Size
**Example: 50% utilization but peak queue was 300**
**Diagnosis:** You HAD enough work scheduled, but it drained too fast

**Solution:**
- Problem isn't total work, it's work distribution over time
- Need to schedule work more continuously, not in bursts
- Adjust when/how methods are added to queue

## Real-Time Monitoring Strategy

**Watch these metrics together:**

1. **Queue Size** - Should stay > 100 during Phase 2
2. **Active Workers** - Should stay > 85% during Phase 2  
3. **Idle Workers** - Should stay < 2 during Phase 2
4. **Empty Events** - Should stay at 0 during Phase 2

**If any deviate from targets → work starvation occurring**

## Example: Perfect Run vs Problem Run

### ✅ Perfect Run (90%+ CPU)
```
Time 5s:  [Queue] Size: 450 | Active: 16/16 (100%) | Idle: 0
Time 10s: [Queue] Size: 320 | Active: 16/16 (100%) | Idle: 0
Time 15s: [Queue] Size: 280 | Active: 15/16 (93.8%) | Idle: 1
Time 20s: [Queue] Size: 150 | Active: 16/16 (100%) | Idle: 0
Time 25s: [Queue] Size: 80  | Active: 14/16 (87.5%) | Idle: 2
```
**Result:** Consistent 90%+ utilization, CPU at 85-95%

### ❌ Problem Run (50% CPU) - Your Case
```
Time 5s:  [Queue] Size: 85  | Active: 8/16 (50.0%) | Idle: 8
Time 7s:  [Queue Starvation] Active: 3/16 | Idle: 13
Time 10s: [Queue] Size: 45  | Active: 7/16 (43.8%) | Idle: 9
Time 12s: [Queue Starvation] Active: 2/16 | Idle: 14
Time 15s: [Queue] Size: 32  | Active: 6/16 (37.5%) | Idle: 10
```
**Result:** 40-50% utilization, CPU at 40-50%, multiple starvation events

## Action Plan

1. **Run your compilation** and watch for:
   - Worker utilization percentage
   - Idle worker count
   - Correlation between low queue size and high idle count

2. **Confirm the diagnosis:**
   - If utilization < 60% during active compilation → **work starvation confirmed**
   - If idle workers > 5 → **not enough work in queue**

3. **Fix based on findings:**
   - Low peak queue (< 100) → Increase upfront scheduling
   - Queue empties frequently → Batch-schedule discovered methods  
   - Utilization drops but queue had work → Adjust priority algorithm

4. **Re-run and verify:**
   - Target: Worker utilization > 85%
   - Target: Idle workers < 2 during active compilation
   - Expected: CPU usage increases from 50% to 85%+
