using System;
using System.Collections.Generic;

namespace GameEditor.BuildPipeline
{
    /// <summary>
    /// 管线执行结果封装
    /// </summary>
    public class PipelineExecutionResult
    {
        public bool IsSuccess { get; set; }
        public Dictionary<EPipeLine, TimeSpan> ExecutionTimes { get; set; } = new();
        public EPipeLine FailedPipeline { get; set; }
        public Exception Error { get; set; }
    }
}