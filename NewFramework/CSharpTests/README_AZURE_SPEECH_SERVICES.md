# Azure Speech Services Test Suite

## Overview

This test suite provides comprehensive testing for Azure Speech Services, covering all major features including Speech-to-Text (STT), Text-to-Speech (TTS), Speech Translation, and Speaker Recognition.

## Features Tested

### 1. Speech-to-Text (STT)
- ✅ Basic audio transcription
- ✅ Real-time continuous recognition
- ✅ Custom speech models
- ✅ Batch transcription processing
- ✅ Multi-language support
- ✅ Multiple audio format support

### 2. Text-to-Speech (TTS)
- ✅ Basic text synthesis
- ✅ Neural voice support
- ✅ SSML markup processing
- ✅ Custom neural voices
- ✅ Batch synthesis

### 3. Speech Translation
- ✅ Basic speech translation
- ✅ Real-time translation
- ✅ Multi-language translation
- ✅ Translation with audio output

### 4. Speaker Recognition
- ✅ Speaker verification
- ✅ Speaker identification
- ✅ Profile enrollment
- ✅ Unknown speaker rejection

### 5. Integration Tests
- ✅ Speech + Azure AI Search integration
- ✅ Speech + Document Intelligence integration
- ✅ Multi-service workflows

### 6. Performance Tests
- ✅ Concurrent request handling
- ✅ Latency measurement
- ✅ Throughput testing

### 7. Error Handling
- ✅ Invalid audio format handling
- ✅ Empty file handling
- ✅ Network timeout handling
- ✅ Quota limit handling

### 8. Quality Validation
- ✅ Transcription accuracy (WER calculation)
- ✅ Audio quality validation
- ✅ Translation quality assessment

## Test Statistics

- **Total Scenarios**: 50+
- **Feature Files**: 1
- **Step Definitions**: 1
- **Helper Classes**: 1
- **Test Categories**: 8

## Prerequisites

### 1. Azure Resources
- Azure Speech Services resource
- Valid subscription key
- Configured region

### 2. NuGet Packages
```xml
<PackageReference Include="Microsoft.CognitiveServices.Speech" Version="1.38.0" />
```

### 3. Configuration
Update `appsettings.json` with your Speech Services credentials:

```json
{
  "Environments": {
    "dev": {
      "SpeechServices": {
        "SubscriptionKey": "your-speech-services-key",
        "Region": "uksouth",
        "DefaultLanguage": "en-GB",
        "DefaultVoice": "en-GB-RyanNeural"
      }
    }
  }
}
```

### 4. Test Data
Place audio files in `TestData/audio/` directory. See [TestData/audio/README.md](TestData/audio/README.md) for details.

## Project Structure

```
CSharpTests/
├── Features/
│   └── AzureSpeechServices.feature          # BDD scenarios
├── StepDefinitions/
│   └── AzureSpeechServicesSteps.cs          # Step implementations
├── Utils/
│   └── AzureSpeechServicesHelper.cs         # Helper methods
├── TestData/
│   └── audio/                                # Test audio files
│       ├── README.md
│       ├── sample-english.wav
│       ├── sample-spanish.wav
│       └── ...
└── README_AZURE_SPEECH_SERVICES.md          # This file
```

## Setup Instructions

### Step 1: Install Dependencies

```bash
cd /Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewFramework/CSharpTests
dotnet restore
```

### Step 2: Configure Credentials

Option A: Update `appsettings.json`
```json
"SpeechServices": {
  "SubscriptionKey": "your-actual-key",
  "Region": "uksouth"
}
```

Option B: Use Environment Variables
```bash
export AZURE_SPEECH_KEY="your-actual-key"
export AZURE_SPEECH_REGION="uksouth"
```

### Step 3: Prepare Test Audio Files

See [TestData/audio/README.md](TestData/audio/README.md) for instructions on creating or obtaining test audio files.

### Step 4: Build the Project

```bash
dotnet build
```

This will:
- Restore NuGet packages
- Generate Reqnroll code-behind files
- Compile the test project

### Step 5: Run Tests

```bash
# Run all Speech Services tests
dotnet test --filter "Category=speech"

# Run specific test categories
dotnet test --filter "Category=stt"           # Speech-to-Text only
dotnet test --filter "Category=tts"           # Text-to-Speech only
dotnet test --filter "Category=translation"   # Translation only
dotnet test --filter "Category=speaker"       # Speaker Recognition only

# Run smoke tests only
dotnet test --filter "Category=smoke&Category=speech"

# Run integration tests
dotnet test --filter "Category=integration&Category=speech"

# Run performance tests
dotnet test --filter "Category=performance&Category=speech"
```

## Running Tests

### Basic Test Execution

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific scenario
dotnet test --filter "DisplayName~transcribe audio file"
```

### Using Test Tags

Tests are tagged for selective execution:

| Tag | Description | Example |
|-----|-------------|---------|
| `@smoke` | Quick health checks | `dotnet test --filter "Category=smoke"` |
| `@speech` | All speech tests | `dotnet test --filter "Category=speech"` |
| `@stt` | Speech-to-Text | `dotnet test --filter "Category=stt"` |
| `@tts` | Text-to-Speech | `dotnet test --filter "Category=tts"` |
| `@translation` | Translation tests | `dotnet test --filter "Category=translation"` |
| `@speaker` | Speaker Recognition | `dotnet test --filter "Category=speaker"` |
| `@integration` | Integration tests | `dotnet test --filter "Category=integration"` |
| `@performance` | Performance tests | `dotnet test --filter "Category=performance"` |
| `@error` | Error handling | `dotnet test --filter "Category=error"` |
| `@quality` | Quality validation | `dotnet test --filter "Category=quality"` |

### Parallel Execution

```bash
# Run tests in parallel (4 workers)
dotnet test --parallel 4

# Configure in appsettings.json
"TestSettings": {
  "ParallelWorkers": 4
}
```

## Test Scenarios

### Health Check
```gherkin
Scenario: Verify Speech Services endpoint is accessible
  Given I have valid Azure Speech Services credentials
  When I check the Speech Services endpoint health
  Then the service should be accessible
```

### Speech-to-Text
```gherkin
Scenario: Transcribe audio file to text
  Given I have valid Azure Speech Services credentials
  And I have an audio file "sample-english.wav"
  When I send the audio for transcription
  Then I should receive the transcribed text
  And the transcription confidence should be above 0.8
```

### Text-to-Speech
```gherkin
Scenario: Synthesize text to speech
  Given I have valid Azure Speech Services credentials
  And I have text "Hello, this is a test of text to speech"
  When I synthesize the text to speech
  Then I should receive an audio file
  And the audio file should be valid
```

### Translation
```gherkin
Scenario: Translate speech from English to Spanish
  Given I have valid Azure Speech Services credentials
  And I have English audio "Hello, how are you?"
  When I translate the speech to Spanish
  Then I should receive Spanish text "Hola, ¿cómo estás?"
```

## Helper Methods

### AzureSpeechServicesHelper

The helper class provides the following methods:

#### Health Check
```csharp
Task<bool> CheckHealthAsync()
```

#### Speech-to-Text
```csharp
Task<SpeechRecognitionResult> TranscribeAudioFileAsync(string audioFilePath, string language = "en-US")
Task<List<string>> ContinuousSpeechRecognitionAsync(string audioFilePath, int durationSeconds = 30)
Task<SpeechRecognitionResult> TranscribeWithCustomModelAsync(string audioFilePath, string customModelEndpoint)
Task<Dictionary<string, SpeechRecognitionResult>> BatchTranscribeAsync(List<string> audioFilePaths)
```

#### Text-to-Speech
```csharp
Task<byte[]> SynthesizeTextToSpeechAsync(string text, string voiceName = "en-US-JennyNeural")
Task<byte[]> SynthesizeSSMLAsync(string ssml)
Task<string> SynthesizeToFileAsync(string text, string outputPath, string voiceName = "en-US-JennyNeural")
Task<List<VoiceInfo>> GetAvailableVoicesAsync(string locale = "en-US")
```

#### Translation
```csharp
Task<TranslationRecognitionResult> TranslateSpeechAsync(string audioFilePath, string sourceLanguage, string targetLanguage)
Task<List<TranslationRecognitionResult>> ContinuousTranslationAsync(string audioFilePath, string sourceLanguage, List<string> targetLanguages)
```

#### Speaker Recognition
```csharp
Task<string> CreateSpeakerProfileAsync(VoiceProfileType profileType)
Task<VoiceProfileEnrollmentResult> EnrollSpeakerProfileAsync(string profileId, string audioFilePath)
Task<SpeakerVerificationResult> VerifySpeakerAsync(string profileId, string audioFilePath)
Task<SpeakerIdentificationResult> IdentifySpeakerAsync(List<string> profileIds, string audioFilePath)
```

#### Utilities
```csharp
double CalculateWordErrorRate(string reference, string hypothesis)
bool ValidateAudioFile(string audioFilePath)
```

## Configuration Options

### Speech Services Settings

```json
{
  "SpeechServices": {
    "SubscriptionKey": "your-key",
    "Region": "uksouth",
    "Endpoint": "https://uksouth.api.cognitive.microsoft.com/sts/v1.0/issuetoken",
    "DefaultLanguage": "en-GB",
    "DefaultVoice": "en-GB-RyanNeural",
    "SupportedLanguages": [
      "en-GB",
      "en-US",
      "es-ES",
      "fr-FR",
      "de-DE"
    ],
    "CustomModels": {
      "MedicalTerms": "https://uksouth.api.cognitive.microsoft.com/speechtotext/v3.0/models/medical-model-id",
      "LegalTerms": "https://uksouth.api.cognitive.microsoft.com/speechtotext/v3.0/models/legal-model-id"
    }
  }
}
```

### Environment-Specific Configuration

The test suite supports multiple environments (dev, qa, prod). Configure each environment separately:

```json
{
  "Environment": "dev",
  "Environments": {
    "dev": {
      "SpeechServices": { /* dev settings */ }
    },
    "qa": {
      "SpeechServices": { /* qa settings */ }
    },
    "prod": {
      "SpeechServices": { /* prod settings */ }
    }
  }
}
```

## Supported Languages

### Speech-to-Text Languages
- English (US, GB, AU, CA, IN)
- Spanish (ES, MX)
- French (FR, CA)
- German (DE)
- Italian (IT)
- Portuguese (PT, BR)
- Chinese (ZH-CN, ZH-TW)
- Japanese (JA)
- Korean (KO)
- And 100+ more...

### Neural Voices (TTS)
- **English**: Jenny, Guy, Aria, Davis, Jane, Jason, Nancy, Tony
- **Spanish**: Elvira, Alvaro
- **French**: Denise, Henri
- **German**: Katja, Conrad
- **And many more...**

See [Azure Speech Services Language Support](https://docs.microsoft.com/azure/cognitive-services/speech-service/language-support) for complete list.

## Performance Benchmarks

Expected performance metrics:

| Operation | Expected Time | Threshold |
|-----------|--------------|-----------|
| Health Check | < 1 second | 2 seconds |
| STT (10s audio) | < 5 seconds | 10 seconds |
| TTS (100 chars) | < 2 seconds | 5 seconds |
| Translation | < 7 seconds | 15 seconds |
| Batch (10 files) | < 30 seconds | 60 seconds |

## Error Handling

The test suite handles various error scenarios:

### Common Errors
- **Invalid Subscription Key**: Check credentials in appsettings.json
- **Region Mismatch**: Ensure region matches your Azure resource
- **Audio Format Error**: Verify audio file format is supported
- **Network Timeout**: Check internet connectivity
- **Quota Exceeded**: Monitor your Azure usage limits

### Error Codes
- `401`: Authentication failed
- `403`: Forbidden (quota exceeded)
- `429`: Too many requests (rate limiting)
- `500`: Internal server error

## Troubleshooting

### Tests Failing with Authentication Error

```bash
# Verify credentials
echo $AZURE_SPEECH_KEY
echo $AZURE_SPEECH_REGION

# Or check appsettings.json
cat Config/appsettings.json | grep -A 5 "SpeechServices"
```

### Audio File Not Found

```bash
# Check if audio files exist
ls -la TestData/audio/

# Create sample audio (macOS)
say "This is a test" -o TestData/audio/sample-english.wav
```

### Build Errors

```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### Reqnroll Code Generation Issues

```bash
# Regenerate feature code-behind files
dotnet build /t:ReqnrollGenerate
```

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Speech Services Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
        working-directory: NewFramework/CSharpTests
      
      - name: Build
        run: dotnet build --no-restore
        working-directory: NewFramework/CSharpTests
      
      - name: Run Speech Services Tests
        run: dotnet test --filter "Category=speech&Category=smoke"
        working-directory: NewFramework/CSharpTests
        env:
          AZURE_SPEECH_KEY: ${{ secrets.AZURE_SPEECH_KEY }}
          AZURE_SPEECH_REGION: ${{ secrets.AZURE_SPEECH_REGION }}
```

### Azure DevOps Pipeline Example

```yaml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

variables:
  - group: AzureSpeechServices

steps:
- task: UseDotNet@2
  inputs:
    version: '9.0.x'

- script: dotnet restore
  displayName: 'Restore packages'
  workingDirectory: 'NewFramework/CSharpTests'

- script: dotnet build --no-restore
  displayName: 'Build project'
  workingDirectory: 'NewFramework/CSharpTests'

- script: dotnet test --filter "Category=speech" --logger trx
  displayName: 'Run Speech Services tests'
  workingDirectory: 'NewFramework/CSharpTests'
  env:
    AZURE_SPEECH_KEY: $(SpeechServicesKey)
    AZURE_SPEECH_REGION: $(SpeechServicesRegion)

- task: PublishTestResults@2
  inputs:
    testResultsFormat: 'VSTest'
    testResultsFiles: '**/*.trx'
```

## Reporting

### Console Output
Tests output detailed logs to console using Serilog.

### Allure Reports
Generate Allure reports for comprehensive test results:

```bash
# Run tests with Allure
dotnet test

# Generate Allure report
allure generate ./allure-results --clean -o ./allure-report

# Open report
allure open ./allure-report
```

### Test Results
Test results are saved in:
- `TestResults/` - NUnit test results
- `allure-results/` - Allure test results
- `Reports/logs/` - Detailed execution logs

## Best Practices

### 1. Audio File Management
- Use consistent audio formats (WAV PCM 16kHz mono)
- Keep audio files under 30 seconds for unit tests
- Use longer files only for performance tests
- Don't commit large audio files to repository

### 2. Test Data
- Use generic, non-sensitive content
- Avoid PII in test audio
- Document expected transcripts
- Version control test data separately

### 3. Error Handling
- Always check for null results
- Log detailed error information
- Use appropriate assertions
- Handle network timeouts gracefully

### 4. Performance
- Run performance tests separately
- Monitor API quota usage
- Use batch operations for multiple files
- Implement retry logic for transient failures

### 5. Maintenance
- Keep SDK versions updated
- Review Azure service updates
- Update language models periodically
- Monitor test execution times

## Contributing

When adding new tests:

1. Follow existing naming conventions
2. Add appropriate tags (@smoke, @stt, @tts, etc.)
3. Update this README with new scenarios
4. Ensure tests are idempotent
5. Add proper error handling
6. Document expected behavior

## Resources

### Documentation
- [Azure Speech Services](https://docs.microsoft.com/azure/cognitive-services/speech-service/)
- [Speech SDK Documentation](https://docs.microsoft.com/azure/cognitive-services/speech-service/speech-sdk)
- [Reqnroll Documentation](https://docs.reqnroll.net/)
- [NUnit Documentation](https://docs.nunit.org/)

### Samples
- [Azure Speech SDK Samples](https://github.com/Azure-Samples/cognitive-services-speech-sdk)
- [Speech Services Quickstarts](https://docs.microsoft.com/azure/cognitive-services/speech-service/get-started)

### Support
- [Azure Speech Services Forum](https://docs.microsoft.com/answers/topics/azure-speech.html)
- [Stack Overflow](https://stackoverflow.com/questions/tagged/azure-speech)
- [GitHub Issues](https://github.com/Azure-Samples/cognitive-services-speech-sdk/issues)

## License

This test suite is part of the Azure ML Workspace automation framework.

## Version History

- **v1.0.0** (2024) - Initial release
  - 50+ test scenarios
  - Full STT, TTS, Translation, Speaker Recognition coverage
  - Integration with existing Azure AI Services tests
  - Comprehensive error handling and quality validation

---

**Last Updated**: 2024
**Maintained By**: Azure ML Automation Team