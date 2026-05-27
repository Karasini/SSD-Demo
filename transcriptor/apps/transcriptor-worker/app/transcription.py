import logging
import subprocess
import tempfile
from dataclasses import dataclass
from pathlib import Path

from app.config import settings
from app.whisper_model import get as get_whisper_model
from app.whisper_model import get_align

logger = logging.getLogger(__name__)

VIDEO_EXTENSIONS = {".mp4", ".mov", ".webm", ".mkv", ".avi"}


@dataclass(frozen=True)
class TranscriptionSegment:
    index: int
    speaker_key: str
    start_seconds: float
    end_seconds: float
    text: str


@dataclass(frozen=True)
class TranscriptionSpeaker:
    speaker_key: str
    default_label: str


@dataclass(frozen=True)
class TranscriptionResult:
    transcript_text: str
    detected_language: str | None
    segments: list[TranscriptionSegment]
    speakers: list[TranscriptionSpeaker]


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


def _normalize_speaker_key(raw: object) -> str | None:
    if raw is None:
        return None
    key = str(raw).strip()
    return key or None


def _build_structured_result(
    segments: list[dict],
    language: str | None,
) -> TranscriptionResult:
    structured_segments: list[TranscriptionSegment] = []
    speaker_order: list[str] = []

    for index, segment in enumerate(segments):
        text = str(segment.get("text", "")).strip()
        speaker_key = _normalize_speaker_key(segment.get("speaker"))
        if not text or not speaker_key:
            continue

        start = float(segment.get("start", 0.0))
        end = float(segment.get("end", start))
        if end < start:
            end = start

        structured_segments.append(
            TranscriptionSegment(
                index=len(structured_segments),
                speaker_key=speaker_key,
                start_seconds=start,
                end_seconds=end,
                text=text,
            )
        )
        if speaker_key not in speaker_order:
            speaker_order.append(speaker_key)

    if not structured_segments:
        raise RuntimeError("no_labeled_segments")

    speakers = [
        TranscriptionSpeaker(
            speaker_key=key,
            default_label=f"Person {index + 1}",
        )
        for index, key in enumerate(speaker_order)
    ]

    transcript_text = "\n".join(segment.text for segment in structured_segments)
    return TranscriptionResult(
        transcript_text=transcript_text,
        detected_language=language,
        segments=structured_segments,
        speakers=speakers,
    )


def transcribe_file(media_path: Path, file_name: str) -> TranscriptionResult:
    """Transcribe, align, and diarize media. Raises RuntimeError on failure."""
    if not settings.hf_token.strip():
        raise RuntimeError("hf_token_missing")

    try:
        import whisperx  # type: ignore
        from whisperx.diarize import DiarizationPipeline  # type: ignore
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

        if not language or not segments:
            raise RuntimeError("whisperx_failed")

        align_model, metadata = get_align(language)
        aligned = whisperx.align(
            segments,
            align_model,
            metadata,
            audio,
            settings.whisper_device,
        )
        result = aligned
        segments = result.get("segments", [])

        diarize_model = DiarizationPipeline(
            token=settings.hf_token,
            device=settings.whisper_device,
        )
        diarize_segments = diarize_model(audio)
        result = whisperx.assign_word_speakers(diarize_segments, result)
        segments = result.get("segments", [])

        return _build_structured_result(segments, language)
    except RuntimeError:
        raise
    except Exception as exc:
        logger.exception("WhisperX diarization pipeline failed")
        message = str(exc).lower()
        if "401" in message or "403" in message or "gated" in message or "token" in message:
            raise RuntimeError("diarization_auth_failed") from exc
        raise RuntimeError("diarization_failed") from exc
    finally:
        if temp_dir is not None:
            temp_dir.cleanup()


def result_to_callback_payload(result: TranscriptionResult) -> dict:
    return {
        "status": "Completed",
        "transcriptText": result.transcript_text,
        "detectedLanguage": result.detected_language,
        "hasDiarization": True,
        "segments": [
            {
                "index": segment.index,
                "speakerKey": segment.speaker_key,
                "startSeconds": segment.start_seconds,
                "endSeconds": segment.end_seconds,
                "text": segment.text,
            }
            for segment in result.segments
        ],
        "speakers": [
            {
                "speakerKey": speaker.speaker_key,
                "defaultLabel": speaker.default_label,
            }
            for speaker in result.speakers
        ],
    }


def map_failure_reason(exc: Exception) -> str:
    message = str(exc)
    if message == "ffmpeg_failed":
        return "Could not read audio from this video file."
    if message == "hf_token_missing":
        return (
            "Speaker detection could not be completed. "
            "Check service configuration or try again later."
        )
    if message in {"diarization_auth_failed", "diarization_failed"}:
        return (
            "Speaker detection could not be completed. "
            "Check service configuration or try again later."
        )
    if message == "no_labeled_segments":
        return "Speaker detection could not be completed for this file."
    if "out of memory" in message.lower() or "cuda" in message.lower():
        return "Processing failed due to resource limits. Try a smaller file or contact support."
    if message in {"whisperx_not_installed", "whisperx_failed"}:
        return "Audio could not be processed. The file may be corrupted or unsupported."
    return "Audio could not be processed. The file may be corrupted or unsupported."
