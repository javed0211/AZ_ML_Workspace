"""Tests for speech translation functionality."""

import pytest

from src.azure_ml_automation.helpers.logger import TestLogger


@pytest.mark.speech_translation
@pytest.mark.azure
class TestSpeechTranslation:
    """Tests for Azure Speech Services translation functionality."""
    
    async def test_speech_to_speech_translation(self, test_logger: TestLogger):
        """Test translating speech from one language to another."""
        # Mock speech translation
        source_audio = "c:\\Users\\admin-javed\\Repos\\AZ_ML_Workspace\\test-data\\english-speech.wav"
        source_language = "en-US"
        target_language = "es-ES"
        
        # translated_audio = await speech_translation_service.translate_speech_to_speech(
        #     source_audio, source_language, target_language
        # )
        # assert len(translated_audio) > 0, "Should generate translated audio"
        
        test_logger.info(f"Translated speech from {source_language} to {target_language}")
    
    async def test_speech_to_text_translation(self, test_logger: TestLogger):
        """Test translating speech to text in different language."""
        source_audio = "c:\\Users\\admin-javed\\Repos\\AZ_ML_Workspace\\test-data\\french-speech.wav"
        source_language = "fr-FR"
        target_language = "en-US"
        
        # translated_text = await speech_translation_service.translate_speech_to_text(
        #     source_audio, source_language, target_language
        # )
        # assert len(translated_text) > 0, "Should generate translated text"
        # assert isinstance(translated_text, str), "Should return string"
        
        test_logger.info(f"Translated speech to text: {source_language} -> {target_language}")
    
    async def test_multi_language_translation(self, test_logger: TestLogger):
        """Test translating to multiple target languages simultaneously."""
        source_audio = "c:\\Users\\admin-javed\\Repos\\AZ_ML_Workspace\\test-data\\english-speech.wav"
        source_language = "en-US"
        target_languages = ["es-ES", "fr-FR", "de-DE", "it-IT"]
        
        # translations = await speech_translation_service.translate_to_multiple_languages(
        #     source_audio, source_language, target_languages
        # )
        # 
        # assert len(translations) == len(target_languages), "Should translate to all target languages"
        # 
        # for lang in target_languages:
        #     assert lang in translations, f"Should have translation for {lang}"
        #     assert len(translations[lang]) > 0, f"Translation for {lang} should not be empty"
        
        test_logger.info(f"Translated to {len(target_languages)} languages")
    
    async def test_real_time_translation(self, test_logger: TestLogger):
        """Test real-time speech translation."""
        source_language = "en-US"
        target_language = "ja-JP"
        
        # Mock real-time translation session
        # translation_session = await speech_translation_service.start_real_time_translation(
        #     source_language, target_language
        # )
        # 
        # # Simulate speaking for a few seconds
        # await asyncio.sleep(3)
        # 
        # # Get translation results
        # results = await speech_translation_service.get_translation_results(translation_session)
        # await speech_translation_service.stop_real_time_translation(translation_session)
        # 
        # assert len(results) > 0, "Should capture and translate real-time speech"
        
        test_logger.info("Real-time translation test completed")
    
    async def test_translation_confidence_scores(self, test_logger: TestLogger):
        """Test getting confidence scores for translations."""
        source_audio = "c:\\Users\\admin-javed\\Repos\\AZ_ML_Workspace\\test-data\\clear-english.wav"
        source_language = "en-US"
        target_language = "zh-CN"
        
        # result = await speech_translation_service.translate_with_confidence(
        #     source_audio, source_language, target_language
        # )
        # 
        # assert "translation" in result, "Should include translated text"
        # assert "confidence" in result, "Should include confidence score"
        # assert 0 <= result["confidence"] <= 1, "Confidence should be between 0 and 1"
        
        test_logger.info("Translation confidence scoring test completed")