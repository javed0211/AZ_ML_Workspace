Feature: Azure Speech Services Integration
    As a developer
    I want to test Azure Speech Services functionality
    So that I can verify speech-to-text, text-to-speech, translation, and speaker recognition work correctly

Background:
    Given I have valid Azure Speech Services credentials
    And the Speech Services endpoint is accessible

@smoke @speech @health
Scenario: Verify Speech Services connectivity
    When I check the Speech Services endpoint health
    Then the service should be accessible
    And I should receive a valid response

# ============================================
# Speech-to-Text (STT) Tests
# ============================================

@speech @stt @basic
Scenario: Transcribe audio file to text
    Given I have an audio file "sample-speech.wav"
    When I send the audio for transcription
    Then I should receive the transcribed text
    And the transcription confidence should be above 0.8
    And the transcription should contain expected keywords

@speech @stt @realtime
Scenario: Real-time speech recognition
    Given I have a streaming audio source
    When I start real-time speech recognition
    And I stream audio data for 10 seconds
    Then I should receive partial transcription results
    And I should receive a final transcription result
    And the recognition should complete within 15 seconds

@speech @stt @custom
Scenario: Use custom speech model for transcription
    Given I have a custom speech model "medical-terminology"
    And I have an audio file with domain-specific terms
    When I transcribe using the custom model
    Then the transcription should correctly identify domain terms
    And the accuracy should be better than the base model

@speech @stt @batch
Scenario: Batch transcription of multiple files
    Given I have 5 audio files for batch processing
    When I submit a batch transcription job
    And I wait for the batch job to complete
    Then all files should be transcribed successfully
    And I should receive transcription results for each file
    And the batch job should complete within 5 minutes

@speech @stt @languages
Scenario Outline: Transcribe audio in different languages
    Given I have an audio file in "<language>"
    When I transcribe with language code "<languageCode>"
    Then the transcription should be in "<language>"
    And the transcription should be accurate

    Examples:
      | language | languageCode |
      | English  | en-US        |
      | Spanish  | es-ES        |
      | French   | fr-FR        |
      | German   | de-DE        |
      | Japanese | ja-JP        |

@speech @stt @formats
Scenario Outline: Support different audio formats
    Given I have an audio file in "<format>" format
    When I transcribe the audio file
    Then the transcription should succeed
    And I should receive valid text output

    Examples:
      | format |
      | WAV    |
      | MP3    |
      | OGG    |
      | FLAC   |

# ============================================
# Text-to-Speech (TTS) Tests
# ============================================

@speech @tts @basic
Scenario: Synthesize text to speech
    Given I have text "Hello, this is a test of Azure Speech Services"
    When I synthesize the text to speech
    Then I should receive an audio file
    And the audio file should be valid
    And the audio duration should match the text length

@speech @tts @voices
Scenario Outline: Use different neural voices
    Given I have text "Testing different voices"
    When I synthesize using voice "<voiceName>"
    Then I should receive audio in the specified voice
    And the audio quality should be high

    Examples:
      | voiceName              |
      | en-US-JennyNeural      |
      | en-US-GuyNeural        |
      | en-GB-SoniaNeural      |
      | en-AU-NatashaNeural    |

@speech @tts @ssml
Scenario: Synthesize speech with SSML markup
    Given I have SSML text with prosody and emphasis tags
    When I synthesize the SSML to speech
    Then the audio should reflect the SSML formatting
    And pauses and emphasis should be audible
    And the audio file should be valid

@speech @tts @customvoice
Scenario: Use custom neural voice
    Given I have a custom neural voice "company-brand-voice"
    And I have text for synthesis
    When I synthesize using the custom voice
    Then the audio should use the custom voice characteristics
    And the audio quality should be consistent

@speech @tts @batch
Scenario: Batch text-to-speech synthesis
    Given I have 10 text strings for synthesis
    When I submit a batch TTS job
    And I wait for synthesis to complete
    Then all text strings should be converted to audio
    And I should receive 10 audio files
    And all audio files should be valid

# ============================================
# Speech Translation Tests
# ============================================

@speech @translation @basic
Scenario: Translate speech from English to Spanish
    Given I have English audio "Hello, how are you today?"
    When I translate the speech to Spanish
    Then I should receive Spanish text "Hola, ¿cómo estás hoy?"
    And the translation confidence should be above 0.8

@speech @translation @realtime
Scenario: Real-time speech translation
    Given I have a streaming audio source in English
    When I start real-time translation to French
    And I stream audio for 30 seconds
    Then I should receive continuous French translations
    And the translations should be contextually accurate

@speech @translation @multilingual
Scenario Outline: Translate between multiple language pairs
    Given I have audio in "<sourceLanguage>"
    When I translate to "<targetLanguage>"
    Then the translation should be accurate
    And the target language should be "<targetLanguage>"

    Examples:
      | sourceLanguage | targetLanguage |
      | English        | Spanish        |
      | English        | French         |
      | Spanish        | English        |
      | German         | English        |
      | Japanese       | English        |

@speech @translation @audio
Scenario: Translate speech and synthesize in target language
    Given I have English audio
    When I translate to Spanish with audio output
    Then I should receive Spanish text translation
    And I should receive Spanish audio output
    And the audio should match the translated text

# ============================================
# Speaker Recognition Tests
# ============================================
# NOTE: Speaker Recognition API was retired on September 30, 2025
# These scenarios are kept for reference but marked as @ignore
# See: https://learn.microsoft.com/en-us/azure/ai-services/speech-service/speaker-recognition-overview

@speech @speaker @verification @ignore @retired
Scenario: Verify speaker identity (RETIRED API)
    Given I have enrolled a speaker profile for "John Doe"
    And I have an audio sample for verification
    When I verify the speaker against the profile
    Then the verification should succeed
    And the confidence score should be above 0.9

@speech @speaker @identification @ignore @retired
Scenario: Identify speaker from multiple profiles (RETIRED API)
    Given I have 5 enrolled speaker profiles
    And I have an audio sample from one of the speakers
    When I identify the speaker
    Then the correct speaker should be identified
    And the identification confidence should be high

@speech @speaker @enrollment @ignore @retired
Scenario: Enroll new speaker profile (RETIRED API)
    Given I have 3 audio samples from a new speaker
    When I create a speaker profile
    And I enroll the audio samples
    Then the speaker profile should be created successfully
    And the profile should be ready for verification

@speech @speaker @rejection @ignore @retired
Scenario: Reject unknown speaker (RETIRED API)
    Given I have enrolled speaker profiles
    And I have audio from an unknown speaker
    When I attempt speaker verification
    Then the verification should fail
    And the system should indicate unknown speaker

# ============================================
# Integration Tests
# ============================================

@speech @integration @pipeline
Scenario: Complete speech processing pipeline
    Given I have an audio file with speech
    When I transcribe the audio to text
    And I translate the text to another language
    And I synthesize the translation to speech
    Then I should have audio in the target language
    And the entire pipeline should complete successfully

@speech @integration @search
Scenario: Transcribe audio and index in Azure AI Search
    Given I have multiple audio files with speech
    When I transcribe all audio files
    And I index the transcriptions in Azure AI Search
    Then I should be able to search the transcribed content
    And search results should return relevant audio files

@speech @integration @document
Scenario: Extract speech from video and analyze with Document Intelligence
    Given I have a video file with speech
    When I extract audio from the video
    And I transcribe the audio to text
    And I analyze the text with Document Intelligence
    Then I should receive structured data from the speech
    And key information should be extracted

# ============================================
# Performance Tests
# ============================================

@speech @performance @concurrent
Scenario: Handle concurrent speech recognition requests
    Given I have 20 audio files
    When I submit all files for transcription concurrently
    Then all transcriptions should complete successfully
    And the average processing time should be under 30 seconds per file
    And no requests should fail

@speech @performance @latency
Scenario: Measure real-time recognition latency
    Given I have a streaming audio source
    When I measure the recognition latency
    Then the latency should be under 500 milliseconds
    And the recognition should be continuous

@speech @performance @throughput
Scenario: Test batch processing throughput
    Given I have 100 audio files
    When I process them in batches of 10
    Then all batches should complete successfully
    And the total processing time should be under 30 minutes

# ============================================
# Error Handling Tests
# ============================================

@speech @error @invalid
Scenario: Handle invalid audio format
    Given I have an invalid audio file
    When I attempt to transcribe the file
    Then I should receive an appropriate error message
    And the error should indicate invalid format

@speech @error @empty
Scenario: Handle empty audio file
    Given I have an empty audio file
    When I attempt to transcribe the file
    Then I should receive an error
    And the error should indicate no audio content

@speech @error @network
Scenario: Handle network timeout gracefully
    Given the Speech Services endpoint is slow
    When I attempt transcription with a short timeout
    Then the request should timeout gracefully
    And I should receive a timeout error

@speech @error @quota
Scenario: Handle quota exceeded error
    Given my Speech Services quota is exceeded
    When I attempt to transcribe audio
    Then I should receive a quota exceeded error
    And the error message should be clear

# ============================================
# Quality Validation Tests
# ============================================

@speech @quality @accuracy
Scenario: Validate transcription accuracy
    Given I have audio with known transcript
    When I transcribe the audio
    Then the transcription should match the known transcript
    And the word error rate should be below 5%

@speech @quality @audio
Scenario: Validate synthesized audio quality
    Given I have text for synthesis
    When I synthesize to audio
    Then the audio should have no distortion
    And the audio should have consistent volume
    And the speech should be natural sounding

@speech @quality @translation
Scenario: Validate translation quality
    Given I have audio with known translation
    When I translate the audio
    Then the translation should match the expected result
    And the translation should preserve meaning
    And the translation should be grammatically correct