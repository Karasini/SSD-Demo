import logging
import subprocess
import tempfile
from pathlib import Path

from app.config import settings
from app.warnings_filter import configure as configure_warnings

configure_warnings()

logger = logging.getLogger(__name__)

VIDEO_EXTENSIONS = {".mp4", ".mov", ".webm", ".mkv", ".avi"}


def _extract_audio_ffmpeg(source: Path, target_wav: Path) -> None:
    cmd = [
        settings.ffmpeg_path,
        "-y",
        "-i",
        str(source),
        "-ac",
        "1",
        "-ar",
        "16000",
        str(target_wav),
    ]
    result = subprocess.run(cmd, capture_output=True, text=True)
    if result.returncode != 0:
        raise RuntimeError("ffmpeg_failed")


def _build_transcript_from_segments(segments: list[dict]) -> str:
    lines = [s.get("text", "").strip() for s in segments if s.get("text")]
    return "\n".join(line for line in lines if line)


def transcribe_file(media_path: Path, file_name: str) -> tuple[str, str | None]:
    """Returns (transcript_text, detected_language)."""
    if settings.mock_mode:
        return (
            f"[Mock transcript for {file_name}]\n\n"
            "This is a development mock transcript. "
            "Set MOCK_MODE=false and install WhisperX for real transcription.",
            "en",
        )

    try:
        import whisperx  # type: ignore
    except ImportError as exc:
        raise RuntimeError("whisperx_not_installed") from exc

    work_path = media_path
    temp_dir: tempfile.TemporaryDirectory[str] | None = None
    extension = Path(file_name).suffix.lower()

    try:
        if extension in VIDEO_EXTENSIONS:
            temp_dir = tempfile.TemporaryDirectory()
            wav_path = Path(temp_dir.name) / "audio.wav"
            _extract_audio_ffmpeg(media_path, wav_path)
            work_path = wav_path

        audio = whisperx.load_audio(str(work_path))
        model = whisperx.load_model(
            settings.whisper_model,
            settings.whisper_device,
            compute_type=settings.whisper_compute_type,
        )
        result = model.transcribe(audio, batch_size=settings.whisper_batch_size)
        language = result.get("language")

        segments = result.get("segments", [])
        if language and segments:
            try:
                align_model, metadata = whisperx.load_align_model(
                    language_code=language, device=settings.whisper_device
                )
                aligned = whisperx.align(
                    segments,
                    align_model,
                    metadata,
                    audio,
                    settings.whisper_device,
                )
                segments = aligned.get("segments", segments)
            except Exception as align_exc:
                logger.warning("Alignment skipped: %s", align_exc)

        text = _build_transcript_from_segments(segments)
        return text, language
    except RuntimeError:
        raise
    except Exception as exc:
        logger.exception("WhisperX failed")
        raise RuntimeError("whisperx_failed") from exc
    finally:
        if temp_dir is not None:
            temp_dir.cleanup()


def map_failure_reason(exc: Exception) -> str:
    message = str(exc)
    if message == "ffmpeg_failed":
        return "Could not read audio from this video file."
    if "out of memory" in message.lower() or "cuda" in message.lower():
        return "Processing failed due to resource limits. Try a smaller file or contact support."
    if message in {"whisperx_not_installed", "whisperx_failed"}:
        return "Audio could not be processed. The file may be corrupted or unsupported."
    return "Audio could not be processed. The file may be corrupted or unsupported."
