# Azure Speech Services Test Audio Files

This directory contains audio files used for testing Azure Speech Services functionality.

## Required Audio Files

To run the complete Speech Services test suite, you need to provide the following audio files:

### Speech-to-Text (STT) Tests
- `sample-english.wav` - English speech sample (5-10 seconds)
- `sample-spanish.wav` - Spanish speech sample
- `sample-french.wav` - French speech sample
- `sample-german.wav` - German speech sample
- `medical-terms.wav` - Audio containing medical terminology
- `legal-terms.wav` - Audio containing legal terminology
- `known-transcript.wav` - Audio with known transcript for accuracy testing

### Audio Format Tests
- `sample.wav` - WAV format audio
- `sample.mp3` - MP3 format audio
- `sample.ogg` - OGG format audio
- `sample.flac` - FLAC format audio

### Streaming/Continuous Recognition Tests
- `streaming-sample.wav` - Longer audio file (30+ seconds) for streaming tests
- `long-audio.wav` - Extended audio for batch processing tests

### Translation Tests
- `english-sample.wav` - Clear English speech for translation
- `multilingual-sample.wav` - Audio containing multiple languages

### Quality Tests
- `high-quality.wav` - High-quality audio recording
- `low-quality.wav` - Lower quality audio for robustness testing
- `noisy-audio.wav` - Audio with background noise

### Error Handling Tests
- `invalid.txt` - Non-audio file for error testing
- `empty.wav` - Empty or corrupted audio file

## Audio File Specifications

### Recommended Format for Best Results
- **Format**: WAV (PCM)
- **Sample Rate**: 16 kHz or 48 kHz
- **Bit Depth**: 16-bit
- **Channels**: Mono (single channel)
- **Duration**: 5-30 seconds for most tests

### Supported Formats
Azure Speech Services supports:
- WAV (PCM, 8 kHz or 16 kHz, 16-bit, mono)
- MP3
- OGG/Opus
- FLAC
- ALAW (in WAV container)
- MULAW (in WAV container)

## Creating Sample Audio Files

### Option 1: Record Your Own
Use any audio recording software to create sample files:
- **macOS**: QuickTime Player, GarageBand, or `afrecord` command
- **Windows**: Voice Recorder, Audacity
- **Linux**: Audacity, `arecord` command

Example using macOS terminal:
```bash
# Record 10 seconds of audio
afrecord -d 10 -r 16000 -c 1 -f 'lpcm' sample-english.wav
```

### Option 2: Text-to-Speech Generation
You can use Azure TTS to generate sample audio files:
```bash
# Using Azure CLI
az cognitiveservices speech synthesize \
  --text "This is a test of speech recognition" \
  --voice "en-US-JennyNeural" \
  --output sample-english.wav
```

### Option 3: Download Sample Audio
Download free speech samples from:
- [Common Voice by Mozilla](https://commonvoice.mozilla.org/)
- [LibriVox](https://librivox.org/)
- [VoxForge](http://www.voxforge.org/)

## Sample Transcripts

For accuracy testing, here are the expected transcripts:

### known-transcript.wav
```
This is a test of speech recognition accuracy
```

### medical-terms.wav
```
The patient presents with hypertension and diabetes mellitus type two
```

### legal-terms.wav
```
The plaintiff hereby submits this affidavit pursuant to the aforementioned statute
```

## File Naming Convention

Follow this naming pattern:
- `{purpose}-{language}.{format}`
- Examples:
  - `sample-english.wav`
  - `medical-terms-spanish.mp3`
  - `streaming-french.wav`

## Testing Without Audio Files

If you don't have audio files, the test suite will:
1. Skip tests that require specific audio files
2. Use placeholder paths for basic validation
3. Log warnings about missing files

To run tests without audio files:
```bash
dotnet test --filter "Category=smoke&Category=speech"
```

## Generating Test Audio Programmatically

You can use the included TTS functionality to generate test audio:

```csharp
var helper = new AzureSpeechServicesHelper(subscriptionKey, region);
var audio = await helper.SynthesizeTextToSpeechAsync("This is a test");
File.WriteAllBytes("sample-english.wav", audio);
```

## Audio File Size Limits

- **Real-time Recognition**: No specific limit (streaming)
- **Batch Transcription**: Up to 1 GB per file
- **Translation**: Recommended under 10 minutes

## Privacy and Security

⚠️ **Important**: 
- Do NOT commit audio files containing sensitive information
- Do NOT commit audio with personally identifiable information (PII)
- Use generic, non-sensitive content for test audio
- This directory is included in `.gitignore` by default

## Troubleshooting

### "Audio file not found" errors
- Ensure files are in the correct directory
- Check file names match exactly (case-sensitive)
- Verify file permissions

### "Invalid audio format" errors
- Confirm audio format is supported
- Check sample rate and bit depth
- Try converting to WAV PCM 16kHz mono

### Poor transcription accuracy
- Ensure clear audio without background noise
- Use recommended audio specifications
- Check language code matches audio language

## Additional Resources

- [Azure Speech Services Documentation](https://docs.microsoft.com/azure/cognitive-services/speech-service/)
- [Audio Format Guidelines](https://docs.microsoft.com/azure/cognitive-services/speech-service/how-to-use-codec-compressed-audio-input-streams)
- [Language Support](https://docs.microsoft.com/azure/cognitive-services/speech-service/language-support)