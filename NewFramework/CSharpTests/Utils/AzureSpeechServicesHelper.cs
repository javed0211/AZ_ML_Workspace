using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;
using Microsoft.CognitiveServices.Speech.Speaker;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PlaywrightFramework.Utils
{
    /// <summary>
    /// Helper class for Azure Speech Services operations
    /// Provides methods for Speech-to-Text, Text-to-Speech, Translation, and Speaker Recognition
    /// </summary>
    public class AzureSpeechServicesHelper
    {
        private readonly string _subscriptionKey;
        private readonly string _region;
        private readonly ILogger _logger;
        private SpeechConfig _speechConfig;

        public AzureSpeechServicesHelper(string subscriptionKey, string region)
        {
            _subscriptionKey = subscriptionKey ?? throw new ArgumentNullException(nameof(subscriptionKey));
            _region = region ?? throw new ArgumentNullException(nameof(region));
            _logger = Log.ForContext<AzureSpeechServicesHelper>();
            
            InitializeSpeechConfig();
        }

        private void InitializeSpeechConfig()
        {
            _speechConfig = SpeechConfig.FromSubscription(_subscriptionKey, _region);
            _speechConfig.SpeechRecognitionLanguage = "en-US";
            _logger.Information("Speech Services initialized for region: {Region}", _region);
        }

        #region Health Check

        /// <summary>
        /// Check if Speech Services endpoint is accessible
        /// </summary>
        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                _logger.Information("Checking Speech Services health...");
                
                // Try to create a recognizer to verify connectivity
                using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
                using var recognizer = new SpeechRecognizer(_speechConfig, audioConfig);
                
                _logger.Information("Speech Services health check passed");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Speech Services health check failed");
                return false;
            }
        }

        #endregion

        #region Speech-to-Text (STT)

        /// <summary>
        /// Transcribe audio file to text
        /// </summary>
        public async Task<SpeechRecognitionResult> TranscribeAudioFileAsync(string audioFilePath, string language = "en-US")
        {
            try
            {
                _logger.Information("Transcribing audio file: {FilePath}", audioFilePath);
                
                if (!File.Exists(audioFilePath))
                {
                    throw new FileNotFoundException($"Audio file not found: {audioFilePath}");
                }

                _speechConfig.SpeechRecognitionLanguage = language;
                
                using var audioConfig = AudioConfig.FromWavFileInput(audioFilePath);
                using var recognizer = new SpeechRecognizer(_speechConfig, audioConfig);
                
                var result = await recognizer.RecognizeOnceAsync();
                
                _logger.Information("Transcription result: {Result}, Reason: {Reason}", 
                    result.Text, result.Reason);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error transcribing audio file");
                throw;
            }
        }

        /// <summary>
        /// Real-time continuous speech recognition
        /// </summary>
        public async Task<List<string>> ContinuousSpeechRecognitionAsync(string audioFilePath, int durationSeconds = 10)
        {
            var recognizedTexts = new List<string>();
            
            try
            {
                _logger.Information("Starting continuous speech recognition for {Duration} seconds", durationSeconds);
                
                using var audioConfig = AudioConfig.FromWavFileInput(audioFilePath);
                using var recognizer = new SpeechRecognizer(_speechConfig, audioConfig);
                
                var stopRecognition = new TaskCompletionSource<int>();
                
                recognizer.Recognizing += (s, e) =>
                {
                    _logger.Debug("Recognizing: {Text}", e.Result.Text);
                };
                
                recognizer.Recognized += (s, e) =>
                {
                    if (e.Result.Reason == ResultReason.RecognizedSpeech)
                    {
                        _logger.Information("Recognized: {Text}", e.Result.Text);
                        recognizedTexts.Add(e.Result.Text);
                    }
                };
                
                recognizer.Canceled += (s, e) =>
                {
                    _logger.Warning("Recognition canceled: {Reason}", e.Reason);
                    stopRecognition.TrySetResult(0);
                };
                
                recognizer.SessionStopped += (s, e) =>
                {
                    _logger.Information("Session stopped");
                    stopRecognition.TrySetResult(0);
                };
                
                await recognizer.StartContinuousRecognitionAsync();
                
                // Wait for duration or completion
                await Task.WhenAny(stopRecognition.Task, Task.Delay(TimeSpan.FromSeconds(durationSeconds)));
                
                await recognizer.StopContinuousRecognitionAsync();
                
                _logger.Information("Continuous recognition completed. Total phrases: {Count}", recognizedTexts.Count);
                
                return recognizedTexts;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in continuous speech recognition");
                throw;
            }
        }

        /// <summary>
        /// Transcribe using custom speech model
        /// </summary>
        public async Task<SpeechRecognitionResult> TranscribeWithCustomModelAsync(
            string audioFilePath, 
            string customModelEndpoint)
        {
            try
            {
                _logger.Information("Transcribing with custom model: {Endpoint}", customModelEndpoint);
                
                var customConfig = SpeechConfig.FromEndpoint(new Uri(customModelEndpoint), _subscriptionKey);
                
                using var audioConfig = AudioConfig.FromWavFileInput(audioFilePath);
                using var recognizer = new SpeechRecognizer(customConfig, audioConfig);
                
                var result = await recognizer.RecognizeOnceAsync();
                
                _logger.Information("Custom model transcription: {Text}", result.Text);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error transcribing with custom model");
                throw;
            }
        }

        /// <summary>
        /// Batch transcription of multiple files
        /// </summary>
        public async Task<Dictionary<string, SpeechRecognitionResult>> BatchTranscribeAsync(
            List<string> audioFilePaths, 
            string language = "en-US")
        {
            var results = new Dictionary<string, SpeechRecognitionResult>();
            
            try
            {
                _logger.Information("Starting batch transcription for {Count} files", audioFilePaths.Count);
                
                var tasks = audioFilePaths.Select(async filePath =>
                {
                    var result = await TranscribeAudioFileAsync(filePath, language);
                    return new { FilePath = filePath, Result = result };
                });
                
                var completedTasks = await Task.WhenAll(tasks);
                
                foreach (var task in completedTasks)
                {
                    results[task.FilePath] = task.Result;
                }
                
                _logger.Information("Batch transcription completed. Success: {Count}", results.Count);
                
                return results;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in batch transcription");
                throw;
            }
        }

        #endregion

        #region Text-to-Speech (TTS)

        /// <summary>
        /// Synthesize text to speech
        /// </summary>
        public async Task<byte[]> SynthesizeTextToSpeechAsync(
            string text, 
            string voiceName = "en-US-JennyNeural",
            string outputFormat = "audio-16khz-128kbitrate-mono-mp3")
        {
            try
            {
                _logger.Information("Synthesizing text to speech. Voice: {Voice}", voiceName);
                
                _speechConfig.SpeechSynthesisVoiceName = voiceName;
                _speechConfig.SetSpeechSynthesisOutputFormat(
                    SpeechSynthesisOutputFormat.Audio16Khz128KBitRateMonoMp3);
                
                using var synthesizer = new SpeechSynthesizer(_speechConfig, null);
                
                var result = await synthesizer.SpeakTextAsync(text);
                
                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    _logger.Information("Speech synthesized successfully. Audio length: {Length} bytes", 
                        result.AudioData.Length);
                    return result.AudioData;
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                    _logger.Error("Speech synthesis canceled: {Reason} - {ErrorDetails}", 
                        cancellation.Reason, cancellation.ErrorDetails);
                    throw new Exception($"Speech synthesis failed: {cancellation.ErrorDetails}");
                }
                
                throw new Exception("Speech synthesis failed with unknown reason");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error synthesizing text to speech");
                throw;
            }
        }

        /// <summary>
        /// Synthesize SSML to speech
        /// </summary>
        public async Task<byte[]> SynthesizeSSMLAsync(string ssml, string voiceName = "en-US-JennyNeural")
        {
            try
            {
                _logger.Information("Synthesizing SSML to speech");
                
                _speechConfig.SpeechSynthesisVoiceName = voiceName;
                
                using var synthesizer = new SpeechSynthesizer(_speechConfig, null);
                
                var result = await synthesizer.SpeakSsmlAsync(ssml);
                
                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    _logger.Information("SSML synthesized successfully");
                    return result.AudioData;
                }
                
                throw new Exception("SSML synthesis failed");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error synthesizing SSML");
                throw;
            }
        }

        /// <summary>
        /// Save synthesized audio to file
        /// </summary>
        public async Task<string> SynthesizeToFileAsync(
            string text, 
            string outputFilePath, 
            string voiceName = "en-US-JennyNeural")
        {
            try
            {
                _logger.Information("Synthesizing text to file: {FilePath}", outputFilePath);
                
                _speechConfig.SpeechSynthesisVoiceName = voiceName;
                
                using var audioConfig = AudioConfig.FromWavFileOutput(outputFilePath);
                using var synthesizer = new SpeechSynthesizer(_speechConfig, audioConfig);
                
                var result = await synthesizer.SpeakTextAsync(text);
                
                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    _logger.Information("Audio saved to: {FilePath}", outputFilePath);
                    return outputFilePath;
                }
                
                throw new Exception("Failed to save audio file");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error synthesizing to file");
                throw;
            }
        }

        /// <summary>
        /// Get available voices
        /// </summary>
        public async Task<List<VoiceInfo>> GetAvailableVoicesAsync(string locale = "en-US")
        {
            try
            {
                _logger.Information("Getting available voices for locale: {Locale}", locale);
                
                using var synthesizer = new SpeechSynthesizer(_speechConfig, null);
                
                var result = await synthesizer.GetVoicesAsync(locale);
                
                if (result.Reason == ResultReason.VoicesListRetrieved)
                {
                    _logger.Information("Retrieved {Count} voices", result.Voices.Count);
                    return result.Voices.ToList();
                }
                
                return new List<VoiceInfo>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting available voices");
                throw;
            }
        }

        #endregion

        #region Speech Translation

        /// <summary>
        /// Translate speech from audio file
        /// </summary>
        public async Task<TranslationRecognitionResult> TranslateSpeechAsync(
            string audioFilePath, 
            string sourceLanguage, 
            string targetLanguage)
        {
            try
            {
                _logger.Information("Translating speech from {Source} to {Target}", 
                    sourceLanguage, targetLanguage);
                
                var translationConfig = SpeechTranslationConfig.FromSubscription(_subscriptionKey, _region);
                translationConfig.SpeechRecognitionLanguage = sourceLanguage;
                translationConfig.AddTargetLanguage(targetLanguage);
                
                using var audioConfig = AudioConfig.FromWavFileInput(audioFilePath);
                using var recognizer = new TranslationRecognizer(translationConfig, audioConfig);
                
                var result = await recognizer.RecognizeOnceAsync();
                
                if (result.Reason == ResultReason.TranslatedSpeech)
                {
                    _logger.Information("Translation successful. Original: {Original}, Translated: {Translated}", 
                        result.Text, result.Translations[targetLanguage]);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error translating speech");
                throw;
            }
        }

        /// <summary>
        /// Continuous speech translation
        /// </summary>
        public async Task<List<(string Original, Dictionary<string, string> Translations)>> 
            ContinuousTranslationAsync(
                string audioFilePath, 
                string sourceLanguage, 
                List<string> targetLanguages,
                int durationSeconds = 30)
        {
            var results = new List<(string, Dictionary<string, string>)>();
            
            try
            {
                _logger.Information("Starting continuous translation");
                
                var translationConfig = SpeechTranslationConfig.FromSubscription(_subscriptionKey, _region);
                translationConfig.SpeechRecognitionLanguage = sourceLanguage;
                
                foreach (var targetLang in targetLanguages)
                {
                    translationConfig.AddTargetLanguage(targetLang);
                }
                
                using var audioConfig = AudioConfig.FromWavFileInput(audioFilePath);
                using var recognizer = new TranslationRecognizer(translationConfig, audioConfig);
                
                var stopRecognition = new TaskCompletionSource<int>();
                
                recognizer.Recognized += (s, e) =>
                {
                    if (e.Result.Reason == ResultReason.TranslatedSpeech)
                    {
                        var translations = new Dictionary<string, string>();
                        foreach (var translation in e.Result.Translations)
                        {
                            translations[translation.Key] = translation.Value;
                        }
                        results.Add((e.Result.Text, translations));
                        _logger.Information("Translated: {Original}", e.Result.Text);
                    }
                };
                
                recognizer.Canceled += (s, e) => stopRecognition.TrySetResult(0);
                recognizer.SessionStopped += (s, e) => stopRecognition.TrySetResult(0);
                
                await recognizer.StartContinuousRecognitionAsync();
                await Task.WhenAny(stopRecognition.Task, Task.Delay(TimeSpan.FromSeconds(durationSeconds)));
                await recognizer.StopContinuousRecognitionAsync();
                
                return results;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in continuous translation");
                throw;
            }
        }

        #endregion

        #region Speaker Recognition

        /// <summary>
        /// Create speaker profile for verification
        /// RETIRED: Speaker Recognition API was retired on September 30, 2025
        /// </summary>
        [Obsolete("Speaker Recognition API has been retired by Microsoft as of September 30, 2025")]
        public async Task<string> CreateSpeakerProfileAsync()
        {
            await Task.CompletedTask;
            throw new NotSupportedException(
                "Speaker Recognition API has been retired by Microsoft as of September 30, 2025. " +
                "See: https://learn.microsoft.com/en-us/azure/ai-services/speech-service/speaker-recognition-overview");
        }

        /// <summary>
        /// Enroll speaker profile with audio samples
        /// RETIRED: Speaker Recognition API was retired on September 30, 2025
        /// </summary>
        [Obsolete("Speaker Recognition API has been retired by Microsoft as of September 30, 2025")]
        public async Task<bool> EnrollSpeakerProfileAsync(string profileId, List<string> audioFilePaths)
        {
            await Task.CompletedTask;
            throw new NotSupportedException(
                "Speaker Recognition API has been retired by Microsoft as of September 30, 2025. " +
                "See: https://learn.microsoft.com/en-us/azure/ai-services/speech-service/speaker-recognition-overview");
        }

        /// <summary>
        /// Verify speaker identity
        /// NOTE: Speaker Recognition API was retired on September 30, 2025
        /// This method is kept for reference but will throw NotSupportedException
        /// </summary>
        [Obsolete("Speaker Recognition API was retired on September 30, 2025")]
        public Task<SpeakerRecognitionResult> VerifySpeakerAsync(string profileId, string audioFilePath)
        {
            throw new NotSupportedException(
                "Speaker Recognition API was retired on September 30, 2025. " +
                "See: https://learn.microsoft.com/en-us/azure/ai-services/speech-service/speaker-recognition-overview");
        }

        /// <summary>
        /// Identify speaker from multiple profiles
        /// NOTE: Speaker Recognition API was retired on September 30, 2025
        /// This method is kept for reference but will throw NotSupportedException
        /// </summary>
        [Obsolete("Speaker Recognition API was retired on September 30, 2025")]
        public Task<SpeakerRecognitionResult> IdentifySpeakerAsync(
            List<string> profileIds, 
            string audioFilePath)
        {
            throw new NotSupportedException(
                "Speaker Recognition API was retired on September 30, 2025. " +
                "See: https://learn.microsoft.com/en-us/azure/ai-services/speech-service/speaker-recognition-overview");
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Calculate Word Error Rate (WER) for accuracy testing
        /// </summary>
        public double CalculateWordErrorRate(string reference, string hypothesis)
        {
            var refWords = reference.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var hypWords = hypothesis.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            var distance = LevenshteinDistance(refWords, hypWords);
            var wer = (double)distance / refWords.Length;
            
            _logger.Information("Word Error Rate: {WER:P2}", wer);
            
            return wer;
        }

        private int LevenshteinDistance(string[] source, string[] target)
        {
            var n = source.Length;
            var m = target.Length;
            var d = new int[n + 1, m + 1];
            
            for (var i = 0; i <= n; i++) d[i, 0] = i;
            for (var j = 0; j <= m; j++) d[0, j] = j;
            
            for (var i = 1; i <= n; i++)
            {
                for (var j = 1; j <= m; j++)
                {
                    var cost = source[i - 1] == target[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            
            return d[n, m];
        }

        /// <summary>
        /// Validate audio file format
        /// </summary>
        public bool ValidateAudioFile(string audioFilePath)
        {
            try
            {
                if (!File.Exists(audioFilePath))
                {
                    _logger.Warning("Audio file not found: {FilePath}", audioFilePath);
                    return false;
                }
                
                var extension = Path.GetExtension(audioFilePath).ToLower();
                var validExtensions = new[] { ".wav", ".mp3", ".ogg", ".flac" };
                
                if (!validExtensions.Contains(extension))
                {
                    _logger.Warning("Invalid audio format: {Extension}", extension);
                    return false;
                }
                
                var fileInfo = new FileInfo(audioFilePath);
                if (fileInfo.Length == 0)
                {
                    _logger.Warning("Audio file is empty: {FilePath}", audioFilePath);
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error validating audio file");
                return false;
            }
        }

        #endregion
    }
}