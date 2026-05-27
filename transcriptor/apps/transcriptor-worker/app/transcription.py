import logging
import subprocess
import tempfile
from pathlib import Path

from app.config import settings
from app.whisper_model import get as get_whisper_model
from app.whisper_model import get_align

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


def _normalize_speaker_segments(segments: list[dict]) -> list[dict]:
    speaker_labels: dict[str, str] = {}
    speaker_counter = 0
    normalized: list[dict] = []

    for segment in segments:
        text = str(segment.get("text", "")).strip()
        if not text:
            continue

        start = float(segment.get("start") or 0.0)
        end = float(segment.get("end") or start)
        if end <= start:
            end = start + 0.01

        raw_speaker = str(segment.get("speaker") or "UNKNOWN")
        if raw_speaker not in speaker_labels:
            speaker_counter += 1
            speaker_labels[raw_speaker] = f"Person {speaker_counter}"

        speaker_label = speaker_labels[raw_speaker]
        speaker_id = f"spk_{speaker_counter}" if raw_speaker == "UNKNOWN" else raw_speaker
        if raw_speaker in speaker_labels:
            # Keep stable IDs by order of first appearance.
            index = list(speaker_labels.keys()).index(raw_speaker) + 1
            speaker_id = f"spk_{index}"

        normalized.append(
            {
                "speakerId": speaker_id,
                "speakerLabel": speaker_label,
                "startSec": round(start, 2),
                "endSec": round(end, 2),
                "text": text,
            }
        )

    if normalized:
        return normalized

    return [
        {
            "speakerId": "spk_1",
            "speakerLabel": "Speaker",
            "startSec": 0.0,
            "endSec": 0.01,
            "text": "",
        }
    ]


def transcribe_file(media_path: Path, file_name: str) -> tuple[str, str | None, list[dict]]:
    """Returns (transcript_text, detected_language, segments)."""
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
        model = get_whisper_model()
        result = model.transcribe(audio, batch_size=settings.whisper_batch_size)
        language = result.get("language")

        segments = result.get("segments", [])
        if language and segments:
            try:
                align_model, metadata = get_align(language)
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

        if settings.hf_token and segments:
            try:
                diarize_model = whisperx.DiarizationPipeline(
                    use_auth_token=settings.hf_token,
                    device=settings.whisper_device,
                )
                diarized = diarize_model(audio)
                speaker_result = whisperx.assign_word_speakers(
                    diarized,
                    {"segments": segments},
                )
                segments = speaker_result.get("segments", segments)
            except Exception as diarize_exc:
                logger.warning("Diarization skipped: %s", diarize_exc)

        text = _build_transcript_from_segments(segments)
        normalized_segments = _normalize_speaker_segments(segments)
        return text, language, normalized_segments
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
