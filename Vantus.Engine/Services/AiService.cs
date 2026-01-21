using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System;

namespace Vantus.Engine.Services;

public class AiService
{
    private readonly TagService _tagService;
    private readonly ILogger<AiService> _logger;
    private InferenceSession? _classificationSession;
    private InferenceSession? _embeddingSession;

    // Model paths (relative to execution directory)
    private const string ClassificationModelPath = "classification_model.onnx";
    private const string EmbeddingModelPath = "embedding_model.onnx";

    public AiService(TagService tagService, ILogger<AiService> logger)
    {
        _tagService = tagService;
        _logger = logger;
        InitializeModels();
    }

    private void InitializeModels()
    {
        try
        {
            // In a real scenario, we would configure SessionOptions (e.g. CUDA execution provider) here based on Settings
            if (File.Exists(ClassificationModelPath))
            {
                _classificationSession = new InferenceSession(ClassificationModelPath);
                _logger.LogInformation("Loaded AI Classification Model");
            }
            if (File.Exists(EmbeddingModelPath))
            {
                _embeddingSession = new InferenceSession(EmbeddingModelPath);
                _logger.LogInformation("Loaded AI Embedding Model");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load AI models");
        }
    }

    public async Task ProcessFileAsync(string filePath, string content)
    {
        var tags = new List<string>();

        if (_classificationSession != null)
        {
            tags.AddRange(RunInference(content));
        }

        // Always run fallback/keyword rules as well, or merge them
        var fallbackTags = ClassifyContentFallback(content);
        foreach(var t in fallbackTags)
        {
            if(!tags.Contains(t)) tags.Add(t);
        }

        foreach (var tag in tags)
        {
            await _tagService.TagFileAsync(filePath, tag, 0.85);
            _logger.LogInformation("AI Tagged {Path} with {Tag}", filePath, tag);
        }

        if (_embeddingSession != null)
        {
            var embedding = GenerateEmbedding(content);
            // TODO: Save embedding to vector store or DB
        }
    }

    private List<string> RunInference(string content)
    {
        try
        {
            if (_classificationSession == null) return new List<string>();

            // Assuming model takes a string tensor named "input" and returns "label"
            // This assumes a model architecture that accepts raw strings (end-to-end)
            var inputTensor = new DenseTensor<string>(new[] { content }, new[] { 1, 1 });
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input", inputTensor)
            };

            using var results = _classificationSession.Run(inputs);

            var output = results.FirstOrDefault(r => r.Name == "label");
            if (output != null)
            {
                var labels = output.AsTensor<string>().ToArray();
                return labels.ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running classification inference");
        }
        return new List<string>();
    }

    private float[] GenerateEmbedding(string content)
    {
        try
        {
            if (_embeddingSession == null) return Array.Empty<float>();

            // Assuming a model that accepts raw strings for embedding generation
            var inputTensor = new DenseTensor<string>(new[] { content }, new[] { 1, 1 });
             var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input", inputTensor)
            };

            using var results = _embeddingSession.Run(inputs);
            var output = results.FirstOrDefault();
            if (output != null)
            {
                return output.AsTensor<float>().ToArray();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running embedding inference");
        }
        return Array.Empty<float>();
    }

    private List<string> ClassifyContentFallback(string content)
    {
        var tags = new List<string>();
        if (string.IsNullOrWhiteSpace(content)) return tags;

        var lower = content.ToLowerInvariant();

        if (lower.Contains("invoice") || lower.Contains("total due")) tags.Add("Finance");
        if (lower.Contains("contract") || lower.Contains("agreement")) tags.Add("Legal");
        if (lower.Contains("meeting") || lower.Contains("minutes")) tags.Add("Meeting");
        if (lower.Contains("c#") || lower.Contains("dotnet") || lower.Contains("python")) tags.Add("Development");
        if (lower.Contains("secret") || lower.Contains("confidential")) tags.Add("Sensitive");
        if (lower.Contains("resume") || lower.Contains("cv")) tags.Add("HR");

        return tags;
    }
}
