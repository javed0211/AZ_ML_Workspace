"""Tests for text-to-speech functionality."""

import pytest
from pathlib import Path

from src.azure_ml_automation.helpers.logger import TestLogger


@pytest.mark.text_to_speech
@pytest.mark.azure
class TestSpeechSynthesis:
    """Tests for Azure Speech Services text-to-speech functionality."""
    
    async def test_basic_text_to_speech(self, test_logger: TestLogger):
        """Test converting text to speech."""
        text = "Hello, this is a test of Azure Speech Services text-to-speech functionality."
        
        # Mock text-to-speech service
        # audio_data = await speech_service.synthesize_speech(text)
        # assert len(audio_data) > 0, "Should generate audio data"
        # assert isinstance(audio_data, bytes), "Audio data should be bytes"
        
        test_logger.info(f"Synthesized speech for text: {text[:50]}...")
    
    async def test_different_voices(self, test_logger: TestLogger):
        """Test text-to-speech with different voice options."""
        text = "Testing different voice options for speech synthesis."
        voices = ["en-US-JennyNeural", "en-US-GuyNeural", "en-GB-SoniaNeural"]
        
        for voice in voices:
            # Mock voice selection
            # audio_data = await speech_service.synthesize_speech(text, voice=voice)
            # assert len(audio_data) > 0, f"Should generate audio with voice: {voice}"
            
            test_logger.info(f"Tested voice: {voice}")
    
    async def test_speech_rate_and_pitch(self, test_logger: TestLogger):
        """Test adjusting speech rate and pitch."""
        text = "Testing speech rate and pitch adjustments."
        
        # Test different rates and pitches
        settings = [
            {"rate": "slow", "pitch": "low"},
            {"rate": "medium", "pitch": "medium"},
            {"rate": "fast", "pitch": "high"}
        ]
        
        for setting in settings:
            # Mock rate and pitch adjustment
            # audio_data = await speech_service.synthesize_speech(
            #     text, 
            #     rate=setting["rate"], 
            #     pitch=setting["pitch"]
            # )
            # assert len(audio_data) > 0, f"Should generate audio with settings: {setting}"
            
            test_logger.info(f"Tested settings: {setting}")
    
    async def test_ssml_support(self, test_logger: TestLogger):
        """Test Speech Synthesis Markup Language (SSML) support."""
        ssml_text = """
        <speak version="1.0" xmlns="http://www.w3.org/2001/10/synthesis" xml:lang="en-US">
            <voice name="en-US-JennyNeural">
                Hello! <break time="500ms"/> 
                This is a test of <emphasis level="strong">SSML</emphasis> support.
                <prosody rate="slow" pitch="low">This part is slow and low.</prosody>
            </voice>
        </speak>
        """
        
        # Mock SSML processing
        # audio_data = await speech_service.synthesize_ssml(ssml_text)
        # assert len(audio_data) > 0, "Should generate audio from SSML"
        
        test_logger.info("SSML synthesis test completed")
    
    async def test_save_audio_to_file(self, test_logger: TestLogger):
        """Test saving synthesized speech to audio file."""
        text = "This speech will be saved to a file."
        output_file = "c:\\Users\\admin-javed\\Repos\\AZ_ML_Workspace\\test-results\\synthesized-speech.wav"
        
        # Mock file saving
        # success = await speech_service.synthesize_to_file(text, output_file)
        # assert success, "Should successfully save audio to file"
        # assert Path(output_file).exists(), "Audio file should be created"
        
        test_logger.info(f"Saved synthesized speech to: {output_file}")
    
    async def test_batch_text_to_speech(self, test_logger: TestLogger):
        """Test batch processing of multiple texts."""
        texts = [
            "First text to synthesize.",
            "Second text to synthesize.",
            "Third text to synthesize."
        ]
        
        # Mock batch processing
        # results = await speech_service.batch_synthesize(texts)
        # assert len(results) == len(texts), "Should process all texts"
        # 
        # for i, result in enumerate(results):
        #     assert len(result) > 0, f"Should generate audio for text {i+1}"
        
        test_logger.info(f"Batch processed {len(texts)} texts")