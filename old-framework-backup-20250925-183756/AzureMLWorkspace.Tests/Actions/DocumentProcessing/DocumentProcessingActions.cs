using Microsoft.Playwright;
using AzureMLWorkspace.Tests.Actions.Core;
using AzureMLWorkspace.Tests.Helpers;
using AzureMLWorkspace.Tests.Configuration;

namespace AzureMLWorkspace.Tests.Actions.DocumentProcessing;

/// <summary>
/// Document processing actions for AI-powered document analysis
/// </summary>
public static class DocumentProcessingActions
{
    /// <summary>
    /// Upload document for processing
    /// </summary>
    public static UploadDocumentAction UploadDocument(IPage page, TestLogger logger, TestConfiguration config, string filePath)
        => new(page, logger, config, filePath);

    /// <summary>
    /// Extract text from PDF
    /// </summary>
    public static ExtractPdfTextAction ExtractPdfText(IPage page, TestLogger logger, TestConfiguration config, string pdfPath)
        => new(page, logger, config, pdfPath);

    /// <summary>
    /// Extract text from image using OCR
    /// </summary>
    public static ExtractImageTextAction ExtractImageText(IPage page, TestLogger logger, TestConfiguration config, string imagePath)
        => new(page, logger, config, imagePath);

    /// <summary>
    /// Classify document
    /// </summary>
    public static ClassifyDocumentAction ClassifyDocument(IPage page, TestLogger logger, TestConfiguration config, string documentContent)
        => new(page, logger, config, documentContent);

    /// <summary>
    /// Extract key phrases from document
    /// </summary>
    public static ExtractKeyPhrasesAction ExtractKeyPhrases(IPage page, TestLogger logger, TestConfiguration config, string documentContent)
        => new(page, logger, config, documentContent);

    /// <summary>
    /// Summarize document
    /// </summary>
    public static SummarizeDocumentAction SummarizeDocument(IPage page, TestLogger logger, TestConfiguration config, string documentContent)
        => new(page, logger, config, documentContent);

    /// <summary>
    /// Analyze document sentiment
    /// </summary>
    public static AnalyzeSentimentAction AnalyzeSentiment(IPage page, TestLogger logger, TestConfiguration config, string documentContent)
        => new(page, logger, config, documentContent);

    /// <summary>
    /// Extract entities from document
    /// </summary>
    public static ExtractEntitiesAction ExtractEntities(IPage page, TestLogger logger, TestConfiguration config, string documentContent)
        => new(page, logger, config, documentContent);
}

/// <summary>
/// Upload document for processing
/// </summary>
public class UploadDocumentAction : BaseAction
{
    private readonly string _filePath;

    public UploadDocumentAction(IPage page, TestLogger logger, TestConfiguration config, string filePath)
        : base(page, logger, config)
    {
        _filePath = filePath;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep($"Uploading document for processing: {_filePath}");
        
        // Ensure file exists and use cross-platform path
        var normalizedPath = Path.GetFullPath(_filePath);
        if (!File.Exists(normalizedPath))
        {
            throw new FileNotFoundException($"Document file not found: {normalizedPath}");
        }
        
        // Navigate to document processing section
        await WaitForElementAsync("[data-testid='document-processing-section']");
        await Page.ClickAsync("[data-testid='document-processing-section']");
        
        // Upload file
        await WaitForElementAsync("input[type='file'][accept*='pdf,image']");
        await Page.SetInputFilesAsync("input[type='file'][accept*='pdf,image']", normalizedPath);
        
        // Wait for upload to complete
        await WaitForElementAsync("[data-testid='upload-success']", 60000);
        Logger.LogStep($"Document uploaded successfully: {Path.GetFileName(_filePath)}");
    }
}

/// <summary>
/// Extract text from PDF using Azure Form Recognizer or similar service
/// </summary>
public class ExtractPdfTextAction : BaseAction
{
    private readonly string _pdfPath;

    public ExtractPdfTextAction(IPage page, TestLogger logger, TestConfiguration config, string pdfPath)
        : base(page, logger, config)
    {
        _pdfPath = pdfPath;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep($"Extracting text from PDF: {_pdfPath}");
        
        // Ensure PDF exists
        var normalizedPath = Path.GetFullPath(_pdfPath);
        if (!File.Exists(normalizedPath))
        {
            throw new FileNotFoundException($"PDF file not found: {normalizedPath}");
        }
        
        // Navigate to text extraction service
        await WaitForElementAsync("[data-testid='text-extraction-service']");
        await Page.ClickAsync("[data-testid='text-extraction-service']");
        
        // Upload PDF
        await WaitForElementAsync("input[type='file'][accept='.pdf']");
        await Page.SetInputFilesAsync("input[type='file'][accept='.pdf']", normalizedPath);
        
        // Start extraction
        await WaitForElementAsync("button[data-testid='start-extraction']");
        await Page.ClickAsync("button[data-testid='start-extraction']");
        
        // Wait for extraction to complete
        await WaitForElementAsync("[data-testid='extraction-results']", 120000);
        
        // Verify text was extracted
        var extractedText = await GetTextAsync("[data-testid='extracted-text']");
        if (string.IsNullOrWhiteSpace(extractedText))
        {
            throw new InvalidOperationException("No text was extracted from the PDF");
        }
        
        Logger.LogStep($"Text extraction completed. Extracted {extractedText.Length} characters");
    }
}

/// <summary>
/// Extract text from image using OCR
/// </summary>
public class ExtractImageTextAction : BaseAction
{
    private readonly string _imagePath;

    public ExtractImageTextAction(IPage page, TestLogger logger, TestConfiguration config, string imagePath)
        : base(page, logger, config)
    {
        _imagePath = imagePath;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep($"Extracting text from image using OCR: {_imagePath}");
        
        // Ensure image exists
        var normalizedPath = Path.GetFullPath(_imagePath);
        if (!File.Exists(normalizedPath))
        {
            throw new FileNotFoundException($"Image file not found: {normalizedPath}");
        }
        
        // Navigate to OCR service
        await WaitForElementAsync("[data-testid='ocr-service']");
        await Page.ClickAsync("[data-testid='ocr-service']");
        
        // Upload image
        await WaitForElementAsync("input[type='file'][accept='image/*']");
        await Page.SetInputFilesAsync("input[type='file'][accept='image/*']", normalizedPath);
        
        // Start OCR processing
        await WaitForElementAsync("button[data-testid='start-ocr']");
        await Page.ClickAsync("button[data-testid='start-ocr']");
        
        // Wait for OCR to complete
        await WaitForElementAsync("[data-testid='ocr-results']", 60000);
        
        // Verify text was extracted
        var extractedText = await GetTextAsync("[data-testid='ocr-text']");
        if (string.IsNullOrWhiteSpace(extractedText))
        {
            Logger.Warning("No text was extracted from the image - this may be expected for images without text");
        }
        else
        {
            Logger.LogStep($"OCR completed. Extracted {extractedText.Length} characters");
        }
    }
}

/// <summary>
/// Classify document using AI
/// </summary>
public class ClassifyDocumentAction : BaseAction
{
    private readonly string _documentContent;

    public ClassifyDocumentAction(IPage page, TestLogger logger, TestConfiguration config, string documentContent)
        : base(page, logger, config)
    {
        _documentContent = documentContent;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep("Classifying document using AI");
        
        // Navigate to document classification service
        await WaitForElementAsync("[data-testid='document-classification']");
        await Page.ClickAsync("[data-testid='document-classification']");
        
        // Enter document content
        await WaitForElementAsync("textarea[data-testid='document-content']");
        await Page.FillAsync("textarea[data-testid='document-content']", _documentContent);
        
        // Start classification
        await WaitForElementAsync("button[data-testid='classify-document']");
        await Page.ClickAsync("button[data-testid='classify-document']");
        
        // Wait for classification results
        await WaitForElementAsync("[data-testid='classification-results']", 30000);
        
        // Verify classification was performed
        var classification = await GetTextAsync("[data-testid='document-category']");
        if (string.IsNullOrWhiteSpace(classification))
        {
            throw new InvalidOperationException("Document classification failed - no category returned");
        }
        
        Logger.LogStep($"Document classified as: {classification}");
    }
}

/// <summary>
/// Extract key phrases from document
/// </summary>
public class ExtractKeyPhrasesAction : BaseAction
{
    private readonly string _documentContent;

    public ExtractKeyPhrasesAction(IPage page, TestLogger logger, TestConfiguration config, string documentContent)
        : base(page, logger, config)
    {
        _documentContent = documentContent;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep("Extracting key phrases from document");
        
        // Navigate to key phrase extraction service
        await WaitForElementAsync("[data-testid='key-phrase-extraction']");
        await Page.ClickAsync("[data-testid='key-phrase-extraction']");
        
        // Enter document content
        await WaitForElementAsync("textarea[data-testid='document-content']");
        await Page.FillAsync("textarea[data-testid='document-content']", _documentContent);
        
        // Start key phrase extraction
        await WaitForElementAsync("button[data-testid='extract-key-phrases']");
        await Page.ClickAsync("button[data-testid='extract-key-phrases']");
        
        // Wait for extraction results
        await WaitForElementAsync("[data-testid='key-phrases-results']", 30000);
        
        // Verify key phrases were extracted
        var keyPhrases = await GetTextAsync("[data-testid='key-phrases-list']");
        if (string.IsNullOrWhiteSpace(keyPhrases))
        {
            throw new InvalidOperationException("Key phrase extraction failed - no phrases returned");
        }
        
        Logger.LogStep($"Key phrases extracted: {keyPhrases}");
    }
}

/// <summary>
/// Summarize document using AI
/// </summary>
public class SummarizeDocumentAction : BaseAction
{
    private readonly string _documentContent;

    public SummarizeDocumentAction(IPage page, TestLogger logger, TestConfiguration config, string documentContent)
        : base(page, logger, config)
    {
        _documentContent = documentContent;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep("Summarizing document using AI");
        
        // Navigate to document summarization service
        await WaitForElementAsync("[data-testid='document-summarization']");
        await Page.ClickAsync("[data-testid='document-summarization']");
        
        // Enter document content
        await WaitForElementAsync("textarea[data-testid='document-content']");
        await Page.FillAsync("textarea[data-testid='document-content']", _documentContent);
        
        // Start summarization
        await WaitForElementAsync("button[data-testid='summarize-document']");
        await Page.ClickAsync("button[data-testid='summarize-document']");
        
        // Wait for summarization results
        await WaitForElementAsync("[data-testid='summary-results']", 60000);
        
        // Verify summary was generated
        var summary = await GetTextAsync("[data-testid='document-summary']");
        if (string.IsNullOrWhiteSpace(summary))
        {
            throw new InvalidOperationException("Document summarization failed - no summary returned");
        }
        
        // Verify summary is shorter than original
        if (summary.Length >= _documentContent.Length)
        {
            Logger.Warning("Summary is not shorter than original document");
        }
        
        Logger.LogStep($"Document summarized. Original: {_documentContent.Length} chars, Summary: {summary.Length} chars");
    }
}

/// <summary>
/// Analyze document sentiment
/// </summary>
public class AnalyzeSentimentAction : BaseAction
{
    private readonly string _documentContent;

    public AnalyzeSentimentAction(IPage page, TestLogger logger, TestConfiguration config, string documentContent)
        : base(page, logger, config)
    {
        _documentContent = documentContent;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep("Analyzing document sentiment");
        
        // Navigate to sentiment analysis service
        await WaitForElementAsync("[data-testid='sentiment-analysis']");
        await Page.ClickAsync("[data-testid='sentiment-analysis']");
        
        // Enter document content
        await WaitForElementAsync("textarea[data-testid='document-content']");
        await Page.FillAsync("textarea[data-testid='document-content']", _documentContent);
        
        // Start sentiment analysis
        await WaitForElementAsync("button[data-testid='analyze-sentiment']");
        await Page.ClickAsync("button[data-testid='analyze-sentiment']");
        
        // Wait for analysis results
        await WaitForElementAsync("[data-testid='sentiment-results']", 30000);
        
        // Verify sentiment was analyzed
        var sentiment = await GetTextAsync("[data-testid='sentiment-score']");
        if (string.IsNullOrWhiteSpace(sentiment))
        {
            throw new InvalidOperationException("Sentiment analysis failed - no sentiment returned");
        }
        
        Logger.LogStep($"Sentiment analysis completed: {sentiment}");
    }
}

/// <summary>
/// Extract entities from document
/// </summary>
public class ExtractEntitiesAction : BaseAction
{
    private readonly string _documentContent;

    public ExtractEntitiesAction(IPage page, TestLogger logger, TestConfiguration config, string documentContent)
        : base(page, logger, config)
    {
        _documentContent = documentContent;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep("Extracting entities from document");
        
        // Navigate to entity extraction service
        await WaitForElementAsync("[data-testid='entity-extraction']");
        await Page.ClickAsync("[data-testid='entity-extraction']");
        
        // Enter document content
        await WaitForElementAsync("textarea[data-testid='document-content']");
        await Page.FillAsync("textarea[data-testid='document-content']", _documentContent);
        
        // Start entity extraction
        await WaitForElementAsync("button[data-testid='extract-entities']");
        await Page.ClickAsync("button[data-testid='extract-entities']");
        
        // Wait for extraction results
        await WaitForElementAsync("[data-testid='entities-results']", 30000);
        
        // Verify entities were extracted
        var entities = await GetTextAsync("[data-testid='entities-list']");
        if (string.IsNullOrWhiteSpace(entities))
        {
            Logger.Warning("No entities were extracted from the document - this may be expected for some content");
        }
        else
        {
            Logger.LogStep($"Entities extracted: {entities}");
        }
    }
}