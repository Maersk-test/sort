using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Maersk.Sorting.Api
{
    public class SortJobProcessor : ISortJobProcessor
    {
        private readonly ILogger<SortJobProcessor> _logger;        
        private static ConcurrentDictionary<Guid, SortJob> _jobsDetails = new ConcurrentDictionary<Guid, SortJob>();

        public SortJobProcessor(ILogger<SortJobProcessor> logger)
        {
            _logger = logger;           
        }

        public async Task<SortJob> Process(SortJob job)
        {
            _logger.LogInformation("Processing job with ID '{JobId}'.", job.Id);
            
            _jobsDetails.TryAdd(job.Id,job);
            var stopwatch = Stopwatch.StartNew();

            var output = job.Input.OrderBy(n => n).ToArray();
            await Task.Delay(5000); // NOTE: This is just to simulate a more expensive operation

            var duration = stopwatch.Elapsed;

            _logger.LogInformation("Completed processing job with ID '{JobId}'. Duration: '{Duration}'.", job.Id, duration);
            
            var completed = new SortJob(
               id: job.Id,
               status: SortJobStatus.Completed,
               duration: duration,
               input: job.Input,
               output: output);

            _jobsDetails.TryUpdate(job.Id, completed, job);

            return completed;
        }

        public SortJob? GetJob(Guid jobId)
        {
            int[] input = {};
            SortJob? value = new SortJob(jobId, SortJobStatus.Pending, null, input, null);

            _jobsDetails.TryGetValue(jobId, out value);
                
            return value;            
        }

        public IEnumerable<SortJob> GetJobs()
        {
            return _jobsDetails.Values.ToList();
        }
    }
}
