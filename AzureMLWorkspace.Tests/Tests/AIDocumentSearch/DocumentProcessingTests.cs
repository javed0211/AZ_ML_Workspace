using AzureMLWorkspace.Tests.Helpers;
using AzureMLWorkspace.Tests.Actions.Core;
using AzureMLWorkspace.Tests.Actions.DocumentProcessing;

namespace AzureMLWorkspace.Tests.Tests.AIDocumentSearch;

[TestFixture]
[Category("DocumentProcessing")]
[Category("AI")]
public class DocumentProcessingTests : BaseTest
{
    private string TestDocumentPath => Path.Combine(Config.TestDataPath, "sample-document.pdf");
    private string TestImagePath => Path.Combine(Config.TestDataPath, "sample-image.png");

    [Test]
    public async Task Test_PDF_Text_Extraction()
    {
        TestLogger.LogStep("Starting PDF text extraction test");

        // Create a sample PDF file for testing if it doesn't exist
        await EnsureTestDocumentExists();

        // Use Actions to perform PDF text extraction
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            .Add(DocumentProcessingActions.ExtractPdfText(Page, TestLogger, Config, TestDocumentPath))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "pdf_extraction_result"))
            .ExecuteAsync();

        TestLogger.LogStep("PDF text extraction test completed successfully");
    }

    [Test]
    public async Task Test_Image_Text_Extraction()
    {
        TestLogger.LogStep("Starting image OCR text extraction test");

        // Create a sample image file for testing if it doesn't exist
        await EnsureTestImageExists();

        // Use Actions to perform OCR text extraction
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            .Add(DocumentProcessingActions.ExtractImageText(Page, TestLogger, Config, TestImagePath))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "ocr_extraction_result"))
            .ExecuteAsync();

        TestLogger.LogStep("Image OCR text extraction test completed successfully");
    }

    [Test]
    public async Task Test_Document_Classification()
    {
        TestLogger.LogStep("Starting document classification test");

        var sampleDocument = "This is a research paper about machine learning algorithms and their applications in data science. " +
                           "The paper discusses various supervised and unsupervised learning techniques, including neural networks, " +
                           "decision trees, and clustering algorithms. The research methodology involves experimental validation " +
                           "on multiple datasets to demonstrate the effectiveness of the proposed approaches.";

        // Use Actions to perform document classification
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            .Add(DocumentProcessingActions.ClassifyDocument(Page, TestLogger, Config, sampleDocument))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "document_classification_result"))
            .ExecuteAsync();

        TestLogger.LogStep("Document classification test completed successfully");
    }

    [Test]
    public async Task Test_Key_Phrase_Extraction()
    {
        TestLogger.LogStep("Starting key phrase extraction test");

        var sampleDocument = "Azure Machine Learning provides cloud-based machine learning services for data scientists and developers. " +
                           "It offers automated machine learning capabilities, model deployment, and comprehensive MLOps features. " +
                           "The platform supports various programming languages including Python, R, and Scala for building ML models.";

        // Use Actions to perform key phrase extraction
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            .Add(DocumentProcessingActions.ExtractKeyPhrases(Page, TestLogger, Config, sampleDocument))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "key_phrases_result"))
            .ExecuteAsync();

        TestLogger.LogStep("Key phrase extraction test completed successfully");
    }

    [Test]
    public async Task Test_Document_Summarization()
    {
        TestLogger.LogStep("Starting document summarization test");

        var longDocument = GenerateLongDocument();

        // Use Actions to perform document summarization
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            .Add(DocumentProcessingActions.SummarizeDocument(Page, TestLogger, Config, longDocument))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "document_summary_result"))
            .ExecuteAsync();

        TestLogger.LogStep("Document summarization test completed successfully");
    }

    [Test]
    public async Task Test_Sentiment_Analysis()
    {
        TestLogger.LogStep("Starting sentiment analysis test");

        var sampleDocument = "I am extremely satisfied with the Azure Machine Learning platform. " +
                           "The user interface is intuitive and the automated ML features save a lot of time. " +
                           "The model deployment process is seamless and the documentation is comprehensive. " +
                           "Overall, it's an excellent tool for machine learning projects.";

        // Use Actions to perform sentiment analysis
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            .Add(DocumentProcessingActions.AnalyzeSentiment(Page, TestLogger, Config, sampleDocument))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "sentiment_analysis_result"))
            .ExecuteAsync();

        TestLogger.LogStep("Sentiment analysis test completed successfully");
    }

    [Test]
    public async Task Test_Entity_Extraction()
    {
        TestLogger.LogStep("Starting entity extraction test");

        var sampleDocument = "Microsoft Azure Machine Learning is a cloud service located in Seattle, Washington. " +
                           "The service was launched in 2014 and is used by companies like Netflix, BMW, and H&R Block. " +
                           "Satya Nadella, CEO of Microsoft, announced new AI capabilities at the Build 2023 conference. " +
                           "The platform supports integration with GitHub and Visual Studio Code.";

        // Use Actions to perform entity extraction
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            .Add(DocumentProcessingActions.ExtractEntities(Page, TestLogger, Config, sampleDocument))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "entity_extraction_result"))
            .ExecuteAsync();

        TestLogger.LogStep("Entity extraction test completed successfully");
    }

    [Test]
    public async Task Test_Complete_Document_Processing_Workflow()
    {
        TestLogger.LogStep("Starting complete document processing workflow test");

        await EnsureTestDocumentExists();
        var sampleText = "This is a comprehensive test of the document processing workflow in Azure ML.";

        // Use Actions to perform a complete workflow with conditional logic and retries
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            .AddWithRetry(DocumentProcessingActions.UploadDocument(Page, TestLogger, Config, TestDocumentPath), maxRetries: 3)
            .Add(DocumentProcessingActions.ExtractPdfText(Page, TestLogger, Config, TestDocumentPath))
            .Add(DocumentProcessingActions.ClassifyDocument(Page, TestLogger, Config, sampleText))
            .Add(DocumentProcessingActions.ExtractKeyPhrases(Page, TestLogger, Config, sampleText))
            .Add(DocumentProcessingActions.SummarizeDocument(Page, TestLogger, Config, GenerateLongDocument()))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "complete_workflow_result"))
            .ExecuteAsync();

        TestLogger.LogStep("Complete document processing workflow test completed successfully");
    }

    [Test]
    public async Task Test_Parallel_Document_Processing()
    {
        TestLogger.LogStep("Starting parallel document processing test");

        var document1 = "First document for parallel processing with machine learning content.";
        var document2 = "Second document for parallel processing with artificial intelligence topics.";
        var document3 = "Third document for parallel processing with data science methodologies.";

        // Use Actions to perform parallel processing
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            .AddParallel(
                DocumentProcessingActions.ClassifyDocument(Page, TestLogger, Config, document1),
                DocumentProcessingActions.ExtractKeyPhrases(Page, TestLogger, Config, document2),
                DocumentProcessingActions.AnalyzeSentiment(Page, TestLogger, Config, document3)
            )
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "parallel_processing_result"))
            .ExecuteAsync();

        TestLogger.LogStep("Parallel document processing test completed successfully");
    }

    private async Task EnsureTestDocumentExists()
    {
        if (!File.Exists(TestDocumentPath))
        {
            TestLogger.LogStep($"Creating test PDF document: {TestDocumentPath}");
            
            // Create a simple text file as a placeholder for PDF
            // In a real scenario, you would create an actual PDF file
            var content = "This is a sample PDF document for testing text extraction capabilities. " +
                         "It contains various types of content including technical terms, dates, and structured information.";
            
            await File.WriteAllTextAsync(TestDocumentPath.Replace(".pdf", ".txt"), content);
            TestLogger.LogStep("Test document created successfully");
        }
    }

    private async Task EnsureTestImageExists()
    {
        if (!File.Exists(TestImagePath))
        {
            TestLogger.LogStep($"Creating test image file: {TestImagePath}");
            
            // Create a placeholder text file for the image
            // In a real scenario, you would create an actual image file
            var content = "This is a placeholder for an image file that would contain text for OCR processing.";
            
            await File.WriteAllTextAsync(TestImagePath.Replace(".png", ".txt"), content);
            TestLogger.LogStep("Test image file created successfully");
        }
    }

    private static string GenerateLongDocument()
    {
        return "Azure Machine Learning is a comprehensive cloud-based platform that provides end-to-end machine learning capabilities. " +
               "The platform offers a wide range of tools and services designed to help data scientists, ML engineers, and developers " +
               "build, train, and deploy machine learning models at scale. With automated machine learning (AutoML) capabilities, " +
               "users can quickly create high-quality models without extensive machine learning expertise. The platform supports " +
               "various programming languages including Python, R, and Scala, making it accessible to developers with different backgrounds. " +
               "Azure ML provides robust data preparation tools, allowing users to clean, transform, and feature engineer their datasets " +
               "efficiently. The platform's compute resources can be scaled dynamically based on workload requirements, ensuring optimal " +
               "performance and cost-effectiveness. Model deployment is streamlined through containerization and integration with " +
               "Azure Kubernetes Service, enabling real-time and batch inference scenarios. The platform also includes comprehensive " +
               "MLOps capabilities, supporting model versioning, monitoring, and lifecycle management. Security and compliance features " +
               "ensure that sensitive data and models are protected throughout the machine learning workflow. Integration with other " +
               "Azure services such as Azure Data Factory, Azure Synapse Analytics, and Power BI creates a unified analytics ecosystem. " +
               "The platform's collaborative features enable teams to work together effectively, sharing experiments, models, and insights. " +
               "With built-in responsible AI tools, users can assess and mitigate potential biases in their models, ensuring fair and " +
               "ethical AI solutions. Azure Machine Learning continues to evolve with regular updates and new features, staying at the " +
               "forefront of machine learning technology and best practices.";
    }
}