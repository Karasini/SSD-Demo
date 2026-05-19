from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", extra="ignore")

    internal_api_key: str = "dev-internal-key"
    api_base_url: str = "http://api:8080"
    whisper_model: str = "base"  # WHISPER_MODEL
    whisper_device: str = "cpu"  # WHISPER_DEVICE
    whisper_compute_type: str = "int8"  # WHISPER_COMPUTE_TYPE
    whisper_batch_size: int = 8
    ffmpeg_path: str = "ffmpeg"
    mock_mode: bool = True
    mock_delay_seconds: float = 3.0


settings = Settings()
