using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ServiceChecker
{
    internal class ServicesChecker
    {
        List<TaskDefinition> tasks;
        ConcurrentBag<TaskResult> results;
        private IReadOnlyList<TaskDefinition> taskList;
        private int maxThreads;
        private CancellationToken ct;

        private static CheckStatusCodes FromHttpStatusCode(int statusCode)
        {
            if (statusCode < 400)
                return CheckStatusCodes.OK;
            else if (statusCode == 408)
                return CheckStatusCodes.TIMEOUT;
            else
                return CheckStatusCodes.ERROR;
        }

        public ServicesChecker(IReadOnlyList<TaskDefinition> taskList, int maxThreads, CancellationToken ct)
        {
            this.taskList = taskList;
            this.maxThreads = maxThreads;
            this.ct = ct;
            this.results = new ConcurrentBag<TaskResult>();
        }

        public async Task Run()
        {
            int completedCount = 0;
            var semaphore = new SemaphoreSlim(maxThreads);
            object _progressLock = new object();
            var progress = new ProgressReporter(taskList.Count());

            var tasks = taskList.Select(async task =>
            {

                await semaphore.WaitAsync();

                int remainingRetries = task.Retry+1;
                Stopwatch sw = new Stopwatch();
                TaskResult? result = null;

                using (HttpClient client = new HttpClient())
                {
                    var timeoutSpan = TimeSpan.FromSeconds(task.Timeout);

                    while (remainingRetries-- != 0)
                    {

                        using var timeoutCts = new CancellationTokenSource(timeoutSpan);
                        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                            ct, timeoutCts.Token);

                        try
                        {
                            ct.ThrowIfCancellationRequested();

                            sw.Restart();
                            var response = await client.GetAsync(task.Url, linkedCts.Token);
                            sw.Stop();

                            if ((int)response.StatusCode == task.ExpectedStatusCode)
                                result = new TaskResult(task, sw.ElapsedMilliseconds, (int)response.StatusCode, FromHttpStatusCode((int)response.StatusCode));
                            else
                                if (remainingRetries == 0)
                                    result = new TaskResult(task, sw.ElapsedMilliseconds, (int)response.StatusCode, CheckStatusCodes.UNEXPECTED_STATUS);
                                else
                                    continue;

                            break;

                        }
                        catch (OperationCanceledException ex) when (timeoutCts.IsCancellationRequested)
                        {
                            if (remainingRetries != 0)
                                continue;

                            result = new TaskResult(task, -1, -1, CheckStatusCodes.TIMEOUT);
                            break;
                        }
                        catch (OperationCanceledException) when (ct.IsCancellationRequested)
                        {
                            result = new TaskResult(task, -1, -1, CheckStatusCodes.CANCELED);
                            break;
                        }
                        catch (Exception)
                        {
                            if (remainingRetries != 0)
                                continue;

                            result = new TaskResult(task, -1, -1, CheckStatusCodes.ERROR);
                            break;
                        }
                        finally
                        {
                            if (result is not null)
                            {
                                results.Add(result);
                                semaphore.Release();
                                lock (_progressLock)
                                {
                                    progress.Report(++completedCount);
                                }
                            }
                        }

                    }
                }

            });

            await Task.WhenAll(tasks);

        }

        public List<TaskResult> GetResults()
        {
            return results.ToList();
        }

    }
}
