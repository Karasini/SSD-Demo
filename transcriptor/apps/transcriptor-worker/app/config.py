from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", extra="ignore")

    internal_api_key: str = "dev-internal-key"
    api_base_url: str = "http://api:8080"
    hf_token: str = ""  # HF_TOKEN — required for diarization
    whisper_model: str = "base"  # WHISPER_MODEL
    whisper_device: str = "cpu"  # WHISPER_DEVICE
    whisper_compute_type: str = "int8"  # WHISPER_COMPUTE_TYPE
    whisper_batch_size: int = 8
    whisper_warmup_languages: str = "en, pl"  # WHISPER_WARMUP_LANGUAGES (comma-separated)
    ffmpeg_path: str = "ffmpeg"

    @property
    def warmup_language_list(self) -> list[str]:
        return [
            code.strip()
            for code in self.whisper_warmup_languages.split(",")
            if code.strip()
        ]


settings = Settings()
